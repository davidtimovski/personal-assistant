using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using PersonalAssistant.Application.Contracts.Accountant.Common;
using PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications;

namespace PersonalAssistant.WorkerService
{
    public class DailyWorker : BackgroundService
    {
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IDeletedEntitiesRepository _deletedEntitiesRepository;
        private readonly IUpcomingExpenseService _upcomingExpenseService;
        private readonly ICdnService _cdnService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _currencyRatesApiKey;
        private readonly string _currencyRatesFilePath;

        public DailyWorker(
            IConfiguration configuration,
            INotificationsRepository notificationsRepository,
            IDeletedEntitiesRepository deletedEntitiesRepository,
            IUpcomingExpenseService upcomingExpenseService,
            ICdnService cdnService,
            IHttpClientFactory httpClientFactory)
        {
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
#if !DEBUG
                await DeleteTemporaryCdnResourcesAsync();
#endif
                await DeleteOldNotificationsAsync();
                await DeleteOldDeletedEntityEntriesAsync();
                await GetAndSaveCurrencyRates();
                await GenerateUpcomingExpenses();

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }

        private async Task DeleteOldNotificationsAsync()
        {
            var aWeekAgo = DateTime.Now.AddDays(-7);
            await _notificationsRepository.DeleteOldAsync(aWeekAgo);
        }

        private async Task DeleteOldDeletedEntityEntriesAsync()
        {
            var threeMonthsAgo = DateTime.Now.AddMonths(-3);
            await _deletedEntitiesRepository.DeleteOldAsync(threeMonthsAgo);
        }

        private async Task DeleteTemporaryCdnResourcesAsync()
        {
            var olderThan = DateTime.Now.AddDays(-2);
            await _cdnService.DeleteTemporaryResourcesAsync(olderThan);
        }

        private async Task GetAndSaveCurrencyRates()
        {
            using HttpClient httpClient = _httpClientFactory.CreateClient("fixer");
            var result = await httpClient.GetAsync($"latest?access_key={_currencyRatesApiKey}");

            string jsonResponse = await result.Content.ReadAsStringAsync();
            var json = JObject.Parse(jsonResponse);
            string rates = Convert.ToString(json["rates"]);

            using var outputFile = new StreamWriter(_currencyRatesFilePath);
            outputFile.Write(rates);
        }

        private async Task GenerateUpcomingExpenses()
        {
            await _upcomingExpenseService.GenerateAsync();
        }
    }
}
