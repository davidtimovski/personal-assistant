using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Contracts.Accountant.AutomaticTransactions.Models;
using Application.Contracts.Accountant.Common.Models;

namespace Application.Contracts.Accountant.AutomaticTransactions;

public interface IAutomaticTransactionService
{
    IEnumerable<AutomaticTransactionDto> GetAll(GetAll model);
    IEnumerable<int> GetDeletedIds(GetDeletedIds model);
    Task<int> CreateAsync(CreateAutomaticTransaction model);
    Task UpdateAsync(UpdateAutomaticTransaction model);
    Task DeleteAsync(int id, int userId);
}
