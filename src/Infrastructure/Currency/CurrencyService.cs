using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Application.Contracts.Common;

namespace Infrastructure.Currency;

public class CurrencyService : ICurrencyService
{
    private readonly string _connectionString;
    private const int DaysSearchLimit = 14;

    public CurrencyService(IConfiguration configuration)
    {
        _connectionString = configuration["ConnectionString"];
    }

    public IDictionary<string, decimal> GetAll(DateTime date)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        return GetCurrencyRates(conn, date, 0);
    }

    public decimal Convert(decimal amount, string fromCurrency, string toCurrency, DateTime date)
    {
        if (fromCurrency == toCurrency)
        {
            return amount;
        }

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        var rates = GetCurrencyRates(conn, date, 0);
        if (rates == null)
        {
            return 0;
        }

        decimal fromRate = rates[fromCurrency];
        decimal eurAmount = amount / fromRate;

        decimal toRate = rates[toCurrency];
        if (toCurrency == "MKD")
        {
            return Math.Round(eurAmount * toRate);
        }

        return eurAmount * toRate;
    }

    private Dictionary<string, decimal> GetCurrencyRates(IDbConnection conn, DateTime date, int daysSearched)
    {
        if (daysSearched == DaysSearchLimit)
        {
            var latestRates = conn.QueryFirstOrDefault<string>(@"SELECT ""Rates"" FROM ""CurrencyRates"" ORDER BY ""Date"" DESC LIMIT 1");
            return JsonSerializer.Deserialize<Dictionary<string, decimal>>(latestRates);
        }

        var rates = conn.QueryFirstOrDefault<string>(@"SELECT ""Rates"" FROM ""CurrencyRates"" WHERE ""Date"" = @Date", new { Date = date });
        if (rates != null)
        {
            return JsonSerializer.Deserialize<Dictionary<string, decimal>>(rates);
        }

        DateTime previous = date.AddDays(-1);
        daysSearched++;

        return GetCurrencyRates(conn, previous, daysSearched);
    }
}
