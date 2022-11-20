using Accountant.Application.Contracts.AutomaticTransactions.Models;
using Accountant.Application.Contracts.Common.Models;

namespace Accountant.Application.Contracts.AutomaticTransactions;

public interface IAutomaticTransactionService
{
    IEnumerable<AutomaticTransactionDto> GetAll(GetAll model);
    IEnumerable<int> GetDeletedIds(GetDeletedIds model);
    Task<int> CreateAsync(CreateAutomaticTransaction model);
    Task UpdateAsync(UpdateAutomaticTransaction model);
    Task DeleteAsync(int id, int userId);
}
