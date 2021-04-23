using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;
using PersonalAssistant.Application.Contracts.Common;

namespace PersonalAssistant.Infrastructure.Currency
{
    public class CurrencyService : ICurrencyService
    {
        private string _connectionString;
        private const int DaysSearchLimit = 14;

        public CurrencyService(IConfiguration configuration)
        {
            _connectionString = configuration["ConnectionString"];
        }

        public string GetAllAsJson(DateTime date)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            return GetCurrencyRatesAsJson(conn, date, 0);
        }

        public decimal Convert(decimal amount, string fromCurrency, string toCurrency, DateTime date)
        {
            if (fromCurrency == toCurrency)
            {
                return amount;
            }

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            string ratesJson = GetCurrencyRatesAsJson(conn, date, 0);
            if (ratesJson == null)
            {
                return 0;
            }

            var ratesLookup = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(ratesJson);

            decimal fromRate = ratesLookup[fromCurrency];
            decimal eurAmount = amount / fromRate;

            decimal toRate = ratesLookup[toCurrency];
            if (toCurrency == "MKD")
            {
                return Math.Round(eurAmount * toRate);
            }

            return eurAmount * toRate;
        }

        private string GetCurrencyRatesAsJson(IDbConnection conn, DateTime date, int daysSearched)
        {
            if (daysSearched == DaysSearchLimit)
            {
                return null;
            }

            var rates = conn.QueryFirstOrDefault<string>(@"SELECT ""Rates"" FROM ""CurrencyRates"" WHERE ""Date"" = @Date", new { Date = date });
            if (rates != null)
            {
                return rates;
            }

            DateTime previous = date.AddDays(-1);
            daysSearched++;

            return GetCurrencyRatesAsJson(conn, previous, daysSearched);
        }
    }
}
