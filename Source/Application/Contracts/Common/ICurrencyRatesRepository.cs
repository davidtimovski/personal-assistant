using System;
using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Application.Contracts.Common
{
    public interface ICurrencyRatesRepository
    {
        Task<CurrencyRates> GetAsync(DateTime date);
        Task CreateAsync(CurrencyRates rates);
    }
}
