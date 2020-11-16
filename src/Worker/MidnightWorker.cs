using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.WorkerService
{
    public class MidnightWorker : BackgroundService
    {
        private readonly ILogger<MidnightWorker> _logger;
        private readonly ICurrencyRatesRepository _currencyRatesRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _currencyRatesApiKey;

        public MidnightWorker(
            ILogger<MidnightWorker> logger,
            IConfiguration configuration,
            ICurrencyRatesRepository currencyRatesRepository,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _currencyRatesRepository = currencyRatesRepository;
            _httpClientFactory = httpClientFactory;
            _currencyRatesApiKey = configuration["FixerApiAccessKey"];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var currentTime = DateTime.UtcNow;
                if (currentTime.Hour == 0 && currentTime.Minute == 0 && currentTime.Second == 0)
                {
                    await GetAndSaveCurrencyRates(currentTime);
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
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

                var rates = new CurrencyRates
                {
                    Date = new DateTime(date.Year, date.Month, date.Day),
                    Rates = ratesData
                };

                await _currencyRatesRepository.CreateAsync(rates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAndSaveCurrencyRates failed");
                throw;
            }
        }
    }
}
