using System;
using System.Collections.Generic;

namespace Application.Contracts.Common;

public interface ICurrencyService
{
    IDictionary<string, decimal> GetAll(DateTime date);
    decimal Convert(decimal amount, string fromCurrency, string toCurrency, DateTime date);
}