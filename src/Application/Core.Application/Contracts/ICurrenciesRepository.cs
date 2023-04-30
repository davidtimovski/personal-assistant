using Sentry;

namespace Core.Application.Contracts;

public interface ICurrenciesRepository
{
    IDictionary<string, decimal> GetAll(DateTime date, ISpan metricsSpan);
    decimal Convert(decimal amount, string fromCurrency, string toCurrency, DateTime date);
}
