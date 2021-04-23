using System;

namespace PersonalAssistant.Application.Contracts.Common
{
    public interface ICurrencyService
    {
        string GetAllAsJson(DateTime date);
        decimal Convert(decimal amount, string fromCurrency, string toCurrency, DateTime date);
    }
}
