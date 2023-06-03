using System.Runtime.Serialization;
using System.Text.Json.Nodes;
using Azure.Storage.Blobs;
using Core.Application.Contracts;
using Core.Application.Entities;
using Dapper;
using Jobs.Models;
using Jobs.Models.Accountant;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Jobs;

public class MidnightJob
{
    private readonly ICdnService _cdnService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly AppConfiguration _config;
    private readonly ILogger<MidnightJob> _logger;

    public MidnightJob(
        ICdnService cdnService,
        IHttpClientFactory httpClientFactory,
        IHostEnvironment hostEnvironment,
        IOptions<AppConfiguration> config,
        ILogger<MidnightJob> logger)
    {
        _cdnService = cdnService;
        _httpClientFactory = httpClientFactory;
        _hostEnvironment = hostEnvironment;
        _config = config.Value;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        var now = DateTime.UtcNow;

        using var conn = new NpgsqlConnection(_config.ConnectionString);
        conn.Open();

        await DeleteOldNotificationsAsync(conn, now);
        await DeleteOldDeletedEntityEntriesAsync(conn, now);
        await GenerateUpcomingExpenses(conn, now);
        await GenerateTransactions(conn, now);

        if (_hostEnvironment.IsProduction())
        {
            await GetAndSaveCurrencyRates(conn, now);
            await DeleteTemporaryCdnResourcesAsync(now);
            await UploadDatabaseBackupAsync();
        }

        _logger.LogInformation("Midnight job run completed.");
    }

    private async Task DeleteOldNotificationsAsync(NpgsqlConnection conn, DateTime now)
    {
        try
        {
            var aWeekAgo = now.AddDays(-7);
            await conn.ExecuteAsync("DELETE FROM todo.notifications WHERE created_date < @DeleteTo", new { DeleteTo = aWeekAgo });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(DeleteOldNotificationsAsync)} failed");
        }
    }

    private async Task DeleteOldDeletedEntityEntriesAsync(NpgsqlConnection conn, DateTime now)
    {
        try
        {
            var oneYearAgo = now.AddYears(-1);
            await conn.ExecuteAsync("DELETE FROM accountant.deleted_entities WHERE deleted_date < @DeleteTo", new { DeleteTo = oneYearAgo });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(DeleteOldDeletedEntityEntriesAsync)} failed");
        }
    }

    /// <summary>
    /// For the automatic transactions functionality in Accountant.
    /// </summary>
    private async Task GenerateTransactions(NpgsqlConnection conn, DateTime now)
    {
        var dbTransaction = conn.BeginTransaction();

        try
        {
            var automaticTransactions = conn.Query<AutomaticTransaction>("SELECT * FROM accountant.automatic_transactions WHERE day_in_month = @DayInMonth", new { DayInMonth = now.Day });

            var userGroups = automaticTransactions.GroupBy(x => x.UserId);

            foreach (var userGroup in userGroups)
            {
                var userId = userGroup.Key;
                int userMainAccountId = conn.QueryFirst<int>("SELECT id FROM accountant.accounts WHERE user_id = @UserId AND is_main", new { UserId = userId });

                foreach (AutomaticTransaction automaticTransaction in userGroup)
                {
                    string expenseOrDepositClause = automaticTransaction.IsDeposit
                        ? "to_account_id = @MainAccountId AND from_account_id IS NULL"
                        : "from_account_id = @MainAccountId AND to_account_id IS NULL";

                    bool exists = conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                                             FROM accountant.transactions
                                                             WHERE generated
                                                                 AND ((category_id IS NULL AND @CategoryId IS NULL) OR category_id = @CategoryId)
                                                                 AND amount = @Amount
                                                                 AND currency = @Currency
                                                                 AND description = @Description
                                                                 AND date = @Date
                                                                 AND " + expenseOrDepositClause,
                        new
                        {
                            automaticTransaction.Amount,
                            automaticTransaction.Currency,
                            automaticTransaction.CategoryId,
                            automaticTransaction.Description,
                            Date = now.Date,
                            MainAccountId = userMainAccountId
                        });

                    if (exists)
                    {
                        continue;
                    }

                    var transaction = new Transaction
                    {
                        CategoryId = automaticTransaction.CategoryId,
                        Amount = automaticTransaction.Amount,
                        Currency = automaticTransaction.Currency,
                        Description = automaticTransaction.Description,
                        Date = now.Date,
                        Generated = true,
                        CreatedDate = now,
                        ModifiedDate = now
                    };

                    if (automaticTransaction.IsDeposit)
                    {
                        transaction.ToAccountId = userMainAccountId;
                    }
                    else
                    {
                        transaction.FromAccountId = userMainAccountId;
                    }

                    await conn.ExecuteAsync(@"INSERT INTO accountant.transactions 
                        (from_account_id, to_account_id, category_id, amount, currency, description, date, is_encrypted, generated, created_date, modified_date)
                        VALUES 
                        (@FromAccountId, @ToAccountId, @CategoryId, @Amount, @Currency, @Description, @Date, FALSE, @Generated, @CreatedDate, @ModifiedDate)", transaction, dbTransaction);

                    if (transaction.FromAccountId.HasValue && transaction.ToAccountId == null)
                    {
                        var relatedUpcomingExpenses = conn.Query<UpcomingExpense>(@"SELECT * FROM accountant.upcoming_expenses
                            WHERE category_id = @CategoryId 
                                AND EXTRACT(year FROM date) = @Year
                                AND EXTRACT(month FROM date) = @Month", new { transaction.CategoryId, transaction.Date.Year, transaction.Date.Month });

                        foreach (var ue in relatedUpcomingExpenses)
                        {
                            var bothWithDescriptionsAndTheyMatch =
                                ue.Description != null
                                && transaction.Description != null
                                && string.Equals(ue.Description, transaction.Description, StringComparison.OrdinalIgnoreCase);

                            if (ue.Description == null || bothWithDescriptionsAndTheyMatch)
                            {
                                if (ue.Amount > transaction.Amount)
                                {
                                    await conn.ExecuteAsync(@"UPDATE accountant.upcoming_expenses
                                        SET amount = amount - @Amount, modified_date = @ModifiedDate
                                        WHERE id = @Id",
                                        new { ue.Id, transaction.Amount, ModifiedDate = now }, dbTransaction);
                                }
                                else
                                {
                                    await conn.ExecuteAsync(@"INSERT INTO accountant.deleted_entities
                                        (user_id, entity_type, entity_id, deleted_date) VALUES
                                        (@UserId, @EntityType, @EntityId, @DeletedDate)",
                                        new DeletedEntity { UserId = userId, EntityType = EntityType.UpcomingExpense, EntityId = ue.Id, DeletedDate = now }, dbTransaction);

                                    await conn.ExecuteAsync("DELETE FROM accountant.upcoming_expenses WHERE id = @Id AND user_id = @UserId",
                                        new { ue.Id, UserId = userId }, dbTransaction);
                                }
                            }
                        }
                    }
                }
            }

            await dbTransaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(GenerateTransactions)} failed");
        }
    }

    /// <summary>
    /// For the upcoming expenses functionality in Accountant.
    /// </summary>
    private async Task GenerateUpcomingExpenses(NpgsqlConnection conn, DateTime now)
    {
        string getMostFrequentCurrency(IEnumerable<Transaction> expenses)
        {
            if (expenses.Count() == 1)
            {
                return expenses.First().Currency;
            }

            var groupedByCurrency = expenses.GroupBy(x => x.Currency);
            var biggestCurrencyGroup = groupedByCurrency.OrderByDescending(g => g.Count()).First();

            var groupsWithMaxExpenses = groupedByCurrency.Where(g => g.Count() == biggestCurrencyGroup.Count());

            // There are multiple biggest groups
            if (groupsWithMaxExpenses.Count() > 1)
            {
                // Use currency from last expense that belongs to biggest groups
                var expensesFromBiggestGroups = groupsWithMaxExpenses.SelectMany(x => x.ToList());
                return expensesFromBiggestGroups.OrderByDescending(x => x.Date).First().Currency;
            }

            // Use currency from biggest group
            return biggestCurrencyGroup.First().Currency;
        }

        bool ShouldGenerate(IReadOnlyCollection<Transaction> expenses)
        {
            if (!expenses.Any())
            {
                return false;
            }

            Transaction earliest = expenses.OrderBy(x => x.Date).First();
            var twoMonthsAgo = new DateTime(now.Year, now.Month, 1).AddMonths(-2);

            return earliest.Date < twoMonthsAgo;
        }

        try
        {
            var categories = conn.Query<Category>("SELECT * FROM accountant.categories WHERE generate_upcoming_expense");

            var userGroups = categories.GroupBy(x => x.UserId);

            foreach (var userGroup in userGroups)
            {
                foreach (Category category in userGroup)
                {
                    bool exists = conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                                             FROM accountant.upcoming_expenses
                                                             WHERE generated AND category_id = @CategoryId
                                                                 AND to_char(created_date, 'YYYY-MM') = to_char(@Now, 'YYYY-MM')",
                        new { CategoryId = category.Id, Now = now });
                    if (exists)
                    {
                        continue;
                    }

                    var firstOfThisMonth = new DateTime(now.Year, now.Month, 1);
                    var transactionsExistThisMonth = conn.ExecuteScalar<bool>(@"SELECT COUNT(*) 
                                                                            FROM accountant.transactions AS t 
                                                                            INNER JOIN accountant.accounts AS a ON a.id = t.from_account_id 
                                                                                OR a.id = t.to_account_id 
                                                                            WHERE a.user_id = @UserId 
                                                                                AND category_id = @CategoryId 
                                                                                AND date >= @From 
                                                                                AND from_account_id IS NOT NULL AND to_account_id IS NULL",
                        new { category.UserId, CategoryId = category.Id, From = firstOfThisMonth });
                    if (transactionsExistThisMonth)
                    {
                        continue;
                    }

                    var threeMonthsAgo = new DateTime(now.Year, now.Month, 1).AddMonths(-3);
                    var expenses = conn.Query<Transaction>(@"SELECT t.* 
                                                        FROM accountant.transactions AS t 
                                                        INNER JOIN accountant.accounts AS a ON a.id = t.from_account_id 
                                                            OR a.id = t.to_account_id 
                                                        WHERE a.user_id = @UserId 
                                                            AND category_id = @CategoryId 
                                                            AND date >= @From AND date < @To 
                                                            AND from_account_id IS NOT NULL AND to_account_id IS NULL",
                        new { category.UserId, CategoryId = category.Id, From = threeMonthsAgo, To = firstOfThisMonth }).ToList();

                    if (!ShouldGenerate(expenses))
                    {
                        continue;
                    }

                    decimal sum = expenses.Sum(x => x.Amount);
                    int months = expenses.GroupBy(x => x.Date.ToString("yyyy-MM")).Count();
                    decimal amount = sum / months;
                    var currency = getMostFrequentCurrency(expenses);
                    if (currency == "MKD")
                    {
                        amount = Math.Round(amount);
                        amount -= amount % 10;
                    }

                    var upcomingExpense = new UpcomingExpense
                    {
                        UserId = category.UserId,
                        CategoryId = category.Id,
                        Amount = amount,
                        Currency = currency,
                        Date = new DateTime(now.Year, now.Month, 1),
                        Generated = true,
                        CreatedDate = now,
                        ModifiedDate = now
                    };

                    await conn.ExecuteAsync(@"INSERT INTO accountant.upcoming_expenses (user_id, category_id, amount, currency, description, date, generated, created_date, modified_date)
                                              VALUES (@UserId, @CategoryId, @Amount, @Currency, @Description, @Date, @Generated, @CreatedDate, @ModifiedDate)", upcomingExpense);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(GenerateUpcomingExpenses)} failed");
        }
    }

    private async Task GetAndSaveCurrencyRates(NpgsqlConnection conn, DateTime now)
    {
        var today = new DateTime(now.Year, now.Month, now.Day);

        try
        {
            var exists = conn.ExecuteScalar<bool>("SELECT COUNT(*) FROM currency_rates WHERE date = @Date", new { Date = today });
            if (exists)
            {
                return;
            }

            using HttpClient httpClient = _httpClientFactory.CreateClient("fixer");
            using var result = await httpClient.GetAsync($"latest?access_key={_config.FixerApiAccessKey}");

            string jsonResponse = await result.Content.ReadAsStringAsync();

            var json = JsonNode.Parse(jsonResponse);
            if (json is null)
            {
                throw new SerializationException("JSON response for currency rates couldn't be deserialized");
            }

            var rates = json["rates"];
            if (rates is null)
            {
                throw new SerializationException("JSON response for currency rates couldn't be deserialized");
            }

            string ratesData = rates.ToJsonString();

            // Save to database
            var parameters = new CurrencyRates
            {
                Date = today,
                Rates = ratesData
            };

            await conn.ExecuteAsync("INSERT INTO currency_rates (date, rates) VALUES (@Date, CAST(@Rates AS json))", parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(GetAndSaveCurrencyRates)} failed");
        }
    }

    private async Task DeleteTemporaryCdnResourcesAsync(DateTime now)
    {
        try
        {
            var olderThan = now.AddDays(-2);
            await _cdnService.DeleteTemporaryResourcesAsync(olderThan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(DeleteTemporaryCdnResourcesAsync)} failed");
        }
    }

    private async Task UploadDatabaseBackupAsync()
    {
        try
        {
            var blobServiceClient = new BlobServiceClient(_config.DbBackup.AzureStorageConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_config.DbBackup.AzureStorageContainerName);

            var backupFileDate = DateTime.UtcNow.AddDays(-1);
            var backupFileName = $"{backupFileDate.ToString("yyyy-MM-dd")}.sql";

            BlobClient blobClient = containerClient.GetBlobClient(backupFileName);

            var path = Path.Combine(_config.DbBackup.BackupsPath, backupFileName);
            await blobClient.UploadAsync(path, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(UploadDatabaseBackupAsync)} failed");
        }
    }
}
