using System.Threading.Tasks;

namespace PersonalAssistant.Application.Contracts.Common
{
    public interface ICurrencyService
    {
        Task<string> GetAllAsJsonAsync();
        decimal Convert(decimal amount, string fromCurrency, string toCurrency);
    }
}
