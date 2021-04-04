using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PersonalAssistant.Application.Contracts.Common;

namespace PersonalAssistant.Infrastructure.Currency
{
    public class CurrencyService : ICurrencyService
    {
        private readonly string _currencyRatesFilePath;
        private string _currencyRatesJson;
        private DateTime _lastLoaded;

        public CurrencyService(string currencyRatesFilePath)
        {
            _currencyRatesFilePath = currencyRatesFilePath;

            _currencyRatesJson = File.ReadAllText(_currencyRatesFilePath);
            _lastLoaded = _currencyRatesJson == string.Empty ? DateTime.MinValue : DateTime.UtcNow;
        }

        public async Task<string> GetAllAsJsonAsync()
        {
            var anHourAgo = DateTime.UtcNow.AddHours(-1);
            if (_lastLoaded < anHourAgo)
            {
                _currencyRatesJson = await File.ReadAllTextAsync(_currencyRatesFilePath);
                _lastLoaded = DateTime.UtcNow;
            }

            return _currencyRatesJson;
        }

        public decimal Convert(decimal amount, string fromCurrency, string toCurrency)
        {
            if (fromCurrency == toCurrency)
            {
                return amount;
            }

            var rates = GetAllAsDictionary();

            decimal fromRate = rates[fromCurrency];
            decimal eurAmount = amount / fromRate;

            decimal toRate = rates[toCurrency];
            if (toCurrency == "MKD")
            {
                return Math.Round(eurAmount * toRate);
            }

            return eurAmount * toRate;
        }

        private Dictionary<string, decimal> GetAllAsDictionary()
        {
            var anHourAgo = DateTime.UtcNow.AddHours(-1);
            if (_lastLoaded < anHourAgo)
            {
                _currencyRatesJson = File.ReadAllText(_currencyRatesFilePath);
                _lastLoaded = DateTime.UtcNow;
            }

            return JsonConvert.DeserializeObject<Dictionary<string, decimal>>(_currencyRatesJson);
        }
    }
}
