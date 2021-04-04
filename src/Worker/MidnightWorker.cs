using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PersonalAssistant.Application.Contracts.Accountant.Common;
using PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.WorkerService
{
    public class MidnightWorker : BackgroundService
    {
        private readonly ILogger<MidnightWorker> _logger;
        private readonly ICurrencyRatesRepository _currencyRatesRepository;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IDeletedEntitiesRepository _deletedEntitiesRepository;
        private readonly IUpcomingExpenseService _upcomingExpenseService;
        private readonly ICdnService _cdnService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _currencyRatesApiKey;
        private readonly string _currencyRatesFilePath;

        public MidnightWorker(
            ILogger<MidnightWorker> logger,
            IConfiguration configuration,
            ICurrencyRatesRepository currencyRatesRepository,
            INotificationsRepository notificationsRepository,
            IDeletedEntitiesRepository deletedEntitiesRepository,
            IUpcomingExpenseService upcomingExpenseService,
            ICdnService cdnService,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _currencyRatesRepository = currencyRatesRepository;
            _notificationsRepository = notificationsRepository;
            _deletedEntitiesRepository = deletedEntitiesRepository;
            _upcomingExpenseService = upcomingExpenseService;
            _cdnService = cdnService;
            _httpClientFactory = httpClientFactory;
            _currencyRatesApiKey = configuration["FixerApiAccessKey"];
            _currencyRatesFilePath = configuration["Currencies:RatesFilePath"];
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

                // Save to db
                await _currencyRatesRepository.CreateAsync(new CurrencyRates
                {
                    Date = new DateTime(date.Year, date.Month, date.Day),
                    Rates = ratesData
                });

                // Save to file
                using var outputFile = new StreamWriter(_currencyRatesFilePath);
                outputFile.Write(ratesData);
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
            await _notificationsRepository.DeleteOldAsync(aWeekAgo);
        }

        private async Task DeleteOldDeletedEntityEntriesAsync()
        {
            var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);
            await _deletedEntitiesRepository.DeleteOldAsync(threeMonthsAgo);
        }

        private async Task GenerateUpcomingExpenses()
        {
            await _upcomingExpenseService.GenerateAsync();
        }
    }
}
