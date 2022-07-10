using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Application.Contracts.Common;
using Dapper;
using Domain.Entities.Accountant;
using Domain.Entities.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Worker;

public class MidnightWorker : BackgroundService
{
    private readonly ILogger<MidnightWorker> _logger;
    private readonly ICdnService _cdnService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _connectionString;
    private readonly string _currencyRatesApiKey;

    public MidnightWorker(
        ILogger<MidnightWorker> logger,
        ICdnService cdnService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _cdnService = cdnService;
        _httpClientFactory = httpClientFactory;

        _connectionString = configuration["ConnectionString"];
        _currencyRatesApiKey = configuration["FixerApiAccessKey"];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bool isProd = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Production";

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            if (now.Hour != 0 || now.Minute != 0)
            {
                continue;
            }

            await GetAndSaveCurrencyRates(now);
            await DeleteOldNotificationsAsync(now);
            await DeleteOldDeletedEntityEntriesAsync(now);
            await GenerateTransactions(now);
            await GenerateUpcomingExpenses(now);

            if (isProd)
            {
                await DeleteTemporaryCdnResourcesAsync(now);
            }

            _logger.LogInformation("Midnight worker run successful.");

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task GetAndSaveCurrencyRates(DateTime now)
    {
        try
        {
            using HttpClient httpClient = _httpClientFactory.CreateClient("fixer");
            HttpResponseMessage result = await httpClient.GetAsync($"latest?access_key={_currencyRatesApiKey}");

            string jsonResponse = await result.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(jsonResponse);
            string ratesData = json["rates"].ToString(Formatting.None);

            // Save to database
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var parameters = new CurrencyRates
            {
                Date = new DateTime(now.Year, now.Month, now.Day),
                Rates = ratesData
            };

            var exists = conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM currency_rates WHERE date = @Date", new { parameters.Date });
            if (exists)
            {
                return;
            }

            await conn.ExecuteAsync(@"INSERT INTO currency_rates (date, rates) VALUES (@Date, CAST(@Rates AS json))", parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(GetAndSaveCurrencyRates)} failed");
            throw;
        }
    }

    private async Task DeleteOldNotificationsAsync(DateTime now)
    {
        try
        {
            var aWeekAgo = now.AddDays(-7);

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            await conn.ExecuteAsync(@"DELETE FROM todo_notifications WHERE created_date < @DeleteFrom", new { DeleteFrom = aWeekAgo });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(DeleteOldNotificationsAsync)} failed");
            throw;
        }
    }

    private async Task DeleteOldDeletedEntityEntriesAsync(DateTime now)
    {
        try
        {
            var sixMonthsAgo = now.AddMonths(-6);

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            await conn.ExecuteAsync(@"DELETE FROM accountant_deleted_entities WHERE deleted_date < @DeleteFrom", new { DeleteFrom = sixMonthsAgo });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(DeleteOldDeletedEntityEntriesAsync)} failed");
            throw;
        }
    }

    /// <summary>
    /// For the automatic transactions functionality in Accountant.
    /// </summary>
    private async Task GenerateTransactions(DateTime now)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var automaticTransactions = conn.Query<AutomaticTransaction>(@"SELECT * FROM accountant_automatic_transactions WHERE day_in_month = @DayInMonth", new { DayInMonth = now.Day });

            var userGroups = automaticTransactions.GroupBy(x => x.UserId);

            foreach (var userGroup in userGroups)
            {
                int userMainAccountId = conn.QueryFirst<int>(@"SELECT id FROM accountant_accounts WHERE user_id = @UserId AND is_main", new { UserId = userGroup.Key });

                foreach (AutomaticTransaction automaticTransaction in userGroup)
                {
                    string expenseOrDepositClause = automaticTransaction.IsDeposit
                        ? "to_account_id = @MainAccountId AND from_account_id IS NULL"
                        : "from_account_id = @MainAccountId AND to_account_id IS NULL";

                    bool exists = conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                                             FROM accountant_transactions
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

                    await conn.ExecuteAsync(@"INSERT INTO accountant_transactions 
                        (from_account_id, to_account_id, category_id, amount, currency, description, date, is_encrypted, generated, created_date, modified_date)
                        VALUES 
                        (@FromAccountId, @ToAccountId, @CategoryId, @Amount, @Currency, @Description, @Date, FALSE, @Generated, @CreatedDate, @ModifiedDate)", transaction);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(GenerateTransactions)} failed");
            throw;
        }
    }

    /// <summary>
    /// For the upcoming expenses funcitonality in Accountant.
    /// </summary>
    private async Task GenerateUpcomingExpenses(DateTime now)
    {
        string getMostFrequentCurrency(IEnumerable<Transaction> expenses)
        {
            if (expenses.Count() == 1)
            {
                return expenses.First().Currency;
            }

            var currency = expenses
                .GroupBy(x => x.Currency)
                .Where(g => g.Count() > 1)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key).First();

            return currency;
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
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var categories = conn.Query<Category>(@"SELECT * FROM accountant_categories WHERE generate_upcoming_expense");

            var userGroups = categories.GroupBy(x => x.UserId);

            foreach (var userGroup in userGroups)
            {
                foreach (Category category in userGroup)
                {
                    bool exists = conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                                             FROM accountant_upcoming_expenses
                                                             WHERE generated AND category_id = @CategoryId
                                                                 AND to_char(created_date, 'YYYY-MM') = to_char(@Now, 'YYYY-MM')",
                        new { CategoryId = category.Id, Now = now });
                    if (exists)
                    {
                        continue;
                    }

                    var firstOfThisMonth = new DateTime(now.Year, now.Month, 1);
                    var transactionsExistThisMonth = conn.ExecuteScalar<bool>(@"SELECT COUNT(*) 
                                                                            FROM accountant_transactions AS t 
                                                                            INNER JOIN accountant_accounts AS a ON a.id = t.from_account_id 
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
                                                        FROM accountant_transactions AS t 
                                                        INNER JOIN accountant_accounts AS a ON a.id = t.from_account_id 
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

                    await conn.ExecuteAsync(@"INSERT INTO accountant_upcoming_expenses (user_id, category_id, amount, currency, description, date, generated, created_date, modified_date)
                                              VALUES (@UserId, @CategoryId, @Amount, @Currency, @Description, @Date, @Generated, @CreatedDate, @ModifiedDate)", upcomingExpense);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(GenerateUpcomingExpenses)} failed");
            throw;
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
            throw;
        }
    }
}
