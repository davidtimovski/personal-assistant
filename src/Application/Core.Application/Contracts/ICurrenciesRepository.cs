namespace Core.Application.Contracts;

public interface ICurrenciesRepository
{
    IDictionary<string, decimal> GetAll(DateTime date);
    decimal Convert(decimal amount, string fromCurrency, string toCurrency, DateTime date);
}
