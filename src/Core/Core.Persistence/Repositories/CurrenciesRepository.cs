using System.Data;
using System.Text.Json;
using Core.Application.Contracts;
using Dapper;

namespace Core.Persistence.Repositories;

public class CurrenciesRepository : BaseRepository, ICurrenciesRepository
{
    private const int DaysSearchLimit = 14;

    public CurrenciesRepository(CoreContext efContext)
        : base(efContext) { }

    public IDictionary<string, decimal> GetAll(DateTime date, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(CurrenciesRepository)}.{nameof(GetAll)}");

        try
        {
            using IDbConnection conn = OpenConnection();
            var rates = GetCurrencyRates(conn, date, 0);
            if (rates is null)
            {
                throw new Exception("No currencies were found");
            }

            return rates;
        }
        finally
        {
            metric.Finish();
        }
    }

    public decimal Convert(decimal amount, string fromCurrency, string toCurrency, DateTime date)
    {
        if (fromCurrency == toCurrency)
        {
            return amount;
        }

        using IDbConnection conn = OpenConnection();

        var rates = GetCurrencyRates(conn, date, 0);
        if (rates is null)
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

    private Dictionary<string, decimal>? GetCurrencyRates(IDbConnection conn, DateTime date, int daysSearched)
    {
        if (daysSearched == DaysSearchLimit)
        {
            var latestRates = conn.QueryFirstOrDefault<string>("SELECT rates FROM currency_rates ORDER BY date DESC LIMIT 1");
            if (latestRates is null)
            {
                return null;
            }

            return JsonSerializer.Deserialize<Dictionary<string, decimal>>(latestRates);
        }

        var rates = conn.QueryFirstOrDefault<string>("SELECT rates FROM currency_rates WHERE date = @Date", new { Date = date });
        if (rates is not null)
        {
            return JsonSerializer.Deserialize<Dictionary<string, decimal>>(rates);
        }

        DateTime previous = date.AddDays(-1);
        daysSearched++;

        return GetCurrencyRates(conn, previous, daysSearched);
    }
}
