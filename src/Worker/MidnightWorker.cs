using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Domain.Entities.Accountant;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.WorkerService
{
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
            while (!stoppingToken.IsCancellationRequested)
            {
                var currentTime = DateTime.UtcNow;
                if (currentTime.Hour == 0 && currentTime.Minute == 0)
                {
                    await GetAndSaveCurrencyRates(currentTime);

#if !DEBUG
                    await DeleteTemporaryCdnResourcesAsync();
#endif

                    await DeleteOldNotificationsAsync();
                    await DeleteOldDeletedEntityEntriesAsync();
                    await GenerateUpcomingExpenses();

                    _logger.LogInformation("Midnight worker run successful.");

                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
            }
        }

        private async Task GetAndSaveCurrencyRates(DateTime date)
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
                    Date = new DateTime(date.Year, date.Month, date.Day),
                    Rates = ratesData
                };

                var exists = conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM ""CurrencyRates"" WHERE ""Date"" = @Date", new { parameters.Date });
                if (exists)
                {
                    return;
                }

                await conn.ExecuteAsync(@"INSERT INTO ""CurrencyRates"" (""Date"", ""Rates"") VALUES (@Date, CAST(@Rates AS json))", parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAndSaveCurrencyRates failed");
                throw;
            }
        }

        private async Task DeleteTemporaryCdnResourcesAsync()
        {
            var olderThan = DateTime.UtcNow.AddDays(-2);
            await _cdnService.DeleteTemporaryResourcesAsync(olderThan);
        }

        private async Task DeleteOldNotificationsAsync()
        {
            var aWeekAgo = DateTime.UtcNow.AddDays(-7);

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            await conn.ExecuteAsync(@"DELETE FROM ""ToDoAssistant.Notifications"" WHERE ""CreatedDate"" < @DeleteFrom", new { DeleteFrom = aWeekAgo });
        }

        private async Task DeleteOldDeletedEntityEntriesAsync()
        {
            var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            await conn.ExecuteAsync(@"DELETE FROM ""Accountant.DeletedEntities"" WHERE ""DeletedDate"" < @DeleteFrom", new { DeleteFrom = threeMonthsAgo });
        }

        private async Task GenerateUpcomingExpenses()
        {
            var now = DateTime.UtcNow;

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

            bool shouldGenerate(IEnumerable<Transaction> expenses)
            {
                if (!expenses.Any())
                {
                    return false;
                }

                Transaction earliest = expenses.OrderBy(x => x.Date).First();
                var twoMonthsAgo = new DateTime(now.Year, now.Month, 1).AddMonths(-2);

                return earliest.Date < twoMonthsAgo;
            }

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var categories = conn.Query<Category>(@"SELECT * FROM ""Accountant.Categories"" WHERE ""GenerateUpcomingExpense""");

            var userGroups = categories.GroupBy(x => x.UserId);

            foreach (var userGroup in userGroups)
            {
                foreach (Category category in userGroup)
                {
                    var exists = conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                                            FROM ""Accountant.UpcomingExpenses""
                                                            WHERE ""CategoryId"" = @CategoryId AND ""Generated"" 
                                                            AND to_char(""CreatedDate"", 'YYYY-MM') = to_char(@Now, 'YYYY-MM')",
                                                            new { CategoryId = category.Id, Now = now });
                    if (exists)
                    {
                        continue;
                    }

                    var firstOfThisMonth = new DateTime(now.Year, now.Month, 1);
                    var transactionsExistThisMonth = conn.ExecuteScalar<bool>(@"SELECT COUNT(*) 
                                                                                FROM ""Accountant.Transactions"" AS t 
                                                                                INNER JOIN ""Accountant.Accounts"" AS a ON a.""Id"" = t.""FromAccountId"" 
                                                                                    OR a.""Id"" = t.""ToAccountId"" 
                                                                                WHERE a.""UserId"" = @UserId 
                                                                                    AND ""CategoryId"" = @CategoryId 
                                                                                    AND ""Date"" >= @From 
                                                                                    AND ""FromAccountId"" IS NOT NULL AND ""ToAccountId"" IS NULL",
                                                                                new { category.UserId, CategoryId = category.Id, From = firstOfThisMonth });
                    if (transactionsExistThisMonth)
                    {
                        continue;
                    }

                    var threeMonthsAgo = new DateTime(now.Year, now.Month, 1).AddMonths(-3);
                    var expenses = conn.Query<Transaction>(@"SELECT t.* 
                                                             FROM ""Accountant.Transactions"" AS t 
                                                             INNER JOIN ""Accountant.Accounts"" AS a ON a.""Id"" = t.""FromAccountId"" 
                                                                 OR a.""Id"" = t.""ToAccountId"" 
                                                             WHERE a.""UserId"" = @UserId 
                                                                 AND ""CategoryId"" = @CategoryId 
                                                                 AND ""Date"" >= @From AND ""Date"" < @To 
                                                                 AND ""FromAccountId"" IS NOT NULL AND ""ToAccountId"" IS NULL",
                                                             new { category.UserId, CategoryId = category.Id, From = threeMonthsAgo, To = firstOfThisMonth });

                    if (shouldGenerate(expenses))
                    {
                        decimal sum = expenses.Sum(x => x.Amount);
                        int months = expenses.GroupBy(x => x.Date.ToString("yyyy-MM")).Count();
                        decimal amount = sum / months;
                        var currency = getMostFrequentCurrency(expenses);
                        if (currency == "MKD")
                        {
                            amount = Math.Round(amount);
                            amount -= amount % 10;
                        }
                        var date = new DateTime(now.Year, now.Month, 1);

                        var upcomingExpense = new UpcomingExpense
                        {
                            UserId = category.UserId,
                            CategoryId = category.Id,
                            Amount = amount,
                            Currency = currency,
                            Date = date,
                            Generated = true,
                            CreatedDate = now,
                            ModifiedDate = now
                        };

                        await conn.ExecuteAsync(@"INSERT INTO ""Accountant.UpcomingExpenses"" (""UserId"", ""CategoryId"", ""Amount"", ""Currency"", ""Description"", ""Date"", ""Generated"", ""CreatedDate"", ""ModifiedDate"")
                                                  VALUES (@UserId, @CategoryId, @Amount, @Currency, @Description, @Date, @Generated, @CreatedDate, @ModifiedDate)", upcomingExpense);
                    }
                }
            }
        }
    }
}
