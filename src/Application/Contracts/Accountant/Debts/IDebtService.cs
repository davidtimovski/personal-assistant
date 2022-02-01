using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Contracts.Accountant.Common.Models;
using Application.Contracts.Accountant.Debts.Models;

namespace Application.Contracts.Accountant.Debts
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
