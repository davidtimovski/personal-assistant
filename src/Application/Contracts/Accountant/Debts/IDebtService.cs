using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Application.Contracts.Accountant.Common.Models;
using PersonalAssistant.Application.Contracts.Accountant.Debts.Models;

namespace PersonalAssistant.Application.Contracts.Accountant.Debts
{
    public interface IDebtService
    {
        IEnumerable<DebtDto> GetAll(GetAll model);
        IEnumerable<int> GetDeletedIds(GetDeletedIds model);
        Task<int> CreateAsync(CreateDebt model);
        Task UpdateAsync(UpdateDebt model);
        Task DeleteAsync(int id, int userId);
    }
}
