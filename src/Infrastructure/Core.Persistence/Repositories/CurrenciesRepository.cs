using System.Data;
using System.Text.Json;
using Core.Application.Contracts;
using Dapper;

namespace Core.Persistence.Repositories;

public class CurrenciesRepository : BaseRepository, ICurrenciesRepository
{
    private const int DaysSearchLimit = 14;

    public CurrenciesRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public IDictionary<string, decimal> GetAll(DateTime date)
    {
        using IDbConnection conn = OpenConnection();

        return GetCurrencyRates(conn, date, 0);
    }

    public decimal Convert(decimal amount, string fromCurrency, string toCurrency, DateTime date)
    {
        if (fromCurrency == toCurrency)
        {
            return amount;
        }

        using IDbConnection conn = OpenConnection();

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
            var latestRates = conn.QueryFirstOrDefault<string>(@"SELECT rates FROM currency_rates ORDER BY date DESC LIMIT 1");
            return JsonSerializer.Deserialize<Dictionary<string, decimal>>(latestRates);
        }

        var rates = conn.QueryFirstOrDefault<string>(@"SELECT rates FROM currency_rates WHERE date = @Date", new { Date = date });
        if (rates != null)
        {
            return JsonSerializer.Deserialize<Dictionary<string, decimal>>(rates);
        }

        DateTime previous = date.AddDays(-1);
        daysSearched++;

        return GetCurrencyRates(conn, previous, daysSearched);
    }
}
