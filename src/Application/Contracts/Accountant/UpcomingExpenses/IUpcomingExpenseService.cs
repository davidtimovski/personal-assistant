using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Contracts.Accountant.Common.Models;
using Application.Contracts.Accountant.UpcomingExpenses.Models;

namespace Application.Contracts.Accountant.UpcomingExpenses;

public interface IUpcomingExpenseService
{
    IEnumerable<UpcomingExpenseDto> GetAll(GetAll model);
    IEnumerable<int> GetDeletedIds(GetDeletedIds model);
    Task<int> CreateAsync(CreateUpcomingExpense model);
    Task UpdateAsync(UpdateUpcomingExpense model);
    Task DeleteAsync(int id, int userId);
    Task DeleteOldAsync(int userId);
}