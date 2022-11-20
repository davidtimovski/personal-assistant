using Accountant.Application.Contracts.Common.Models;
using Accountant.Application.Contracts.UpcomingExpenses.Models;

namespace Accountant.Application.Contracts.UpcomingExpenses;

public interface IUpcomingExpenseService
{
    IEnumerable<UpcomingExpenseDto> GetAll(GetAll model);
    IEnumerable<int> GetDeletedIds(GetDeletedIds model);
    Task<int> CreateAsync(CreateUpcomingExpense model);
    Task UpdateAsync(UpdateUpcomingExpense model);
    Task DeleteAsync(int id, int userId);
    Task DeleteOldAsync(int userId);
}
