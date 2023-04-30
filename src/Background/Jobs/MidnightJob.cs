using Application.Domain.Accountant;
using Application.Domain.Common;
using Core.Application.Contracts;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Background.Jobs;

public class MidnightJob
{
    private readonly ILogger<MidnightJob> _logger;
    private readonly ICdnService _cdnService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly string _connectionString;
    private readonly string _currencyRatesApiKey;

    public MidnightJob(
        ILogger<MidnightJob> logger,
        ICdnService cdnService,
        IHttpClientFactory httpClientFactory,
        IHostEnvironment hostEnvironment,
        IConfiguration configuration)
    {
        _logger = logger;
        _cdnService = cdnService;
        _httpClientFactory = httpClientFactory;
        _hostEnvironment = hostEnvironment;

        _connectionString = configuration["ConnectionString"];
        _currencyRatesApiKey = configuration["FixerApiAccessKey"];
    }

    public async Task RunAsync()
    {
        var now = DateTime.UtcNow;

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        await DeleteOldNotificationsAsync(conn, now);
        await DeleteOldDeletedEntityEntriesAsync(conn, now);
        await GenerateTransactions(conn, now);
        await GenerateUpcomingExpenses(conn, now);

        if (_hostEnvironment.IsProduction())
        {
            await GetAndSaveCurrencyRates(conn, now);
            await DeleteTemporaryCdnResourcesAsync(now);
        }

        _logger.LogInformation("Midnight job run completed.");
    }

    private async Task DeleteOldNotificationsAsync(NpgsqlConnection conn, DateTime now)
    {
        try
        {
            var aWeekAgo = now.AddDays(-7);
            await conn.ExecuteAsync(@"DELETE FROM todo.notifications WHERE created_date < @DeleteFrom", new { DeleteFrom = aWeekAgo });
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
            var sixMonthsAgo = now.AddMonths(-6);
            await conn.ExecuteAsync(@"DELETE FROM accountant.deleted_entities WHERE deleted_date < @DeleteFrom", new { DeleteFrom = sixMonthsAgo });
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
        try
        {
            var automaticTransactions = conn.Query<AutomaticTransaction>(@"SELECT * FROM accountant.automatic_transactions WHERE day_in_month = @DayInMonth", new { DayInMonth = now.Day });

            var userGroups = automaticTransactions.GroupBy(x => x.UserId);

            foreach (var userGroup in userGroups)
            {
                int userMainAccountId = conn.QueryFirst<int>(@"SELECT id FROM accountant.accounts WHERE user_id = @UserId AND is_main", new { UserId = userGroup.Key });

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
                        (@FromAccountId, @ToAccountId, @CategoryId, @Amount, @Currency, @Description, @Date, FALSE, @Generated, @CreatedDate, @ModifiedDate)", transaction);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(GenerateTransactions)} failed");
        }
    }

    /// <summary>
    /// For the upcoming expenses funcitonality in Accountant.
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
            var categories = conn.Query<Category>(@"SELECT * FROM accountant.categories WHERE generate_upcoming_expense");

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
            var exists = conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM currency_rates WHERE date = @Date", new { Date = today });
            if (exists)
            {
                return;
            }

            using HttpClient httpClient = _httpClientFactory.CreateClient("fixer");
            HttpResponseMessage result = await httpClient.GetAsync($"latest?access_key={_currencyRatesApiKey}");

            string jsonResponse = await result.Content.ReadAsStringAsync();
            var json = JObject.Parse(jsonResponse);
            string ratesData = json["rates"].ToString(Formatting.None);

            // Save to database
            var parameters = new CurrencyRates
            {
                Date = today,
                Rates = ratesData
            };

            await conn.ExecuteAsync(@"INSERT INTO currency_rates (date, rates) VALUES (@Date, CAST(@Rates AS json))", parameters);
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
}
