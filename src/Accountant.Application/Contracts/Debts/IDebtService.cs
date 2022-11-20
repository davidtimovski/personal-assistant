using Accountant.Application.Contracts.Common.Models;
using Accountant.Application.Contracts.Debts.Models;

namespace Accountant.Application.Contracts.Debts;

public interface IDebtService
{
    IEnumerable<DebtDto> GetAll(GetAll model);
    IEnumerable<int> GetDeletedIds(GetDeletedIds model);
    Task<int> CreateAsync(CreateDebt model);
    Task<int> CreateMergedAsync(CreateDebt model);
    Task UpdateAsync(UpdateDebt model);
    Task DeleteAsync(int id, int userId);
}
