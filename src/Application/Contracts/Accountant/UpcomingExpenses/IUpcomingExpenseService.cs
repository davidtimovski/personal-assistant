using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Application.Contracts.Accountant.Common.Models;
using PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses.Models;

namespace PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses
{
    public interface IUpcomingExpenseService
    {
        IEnumerable<UpcomingExpenseDto> GetAll(GetAll model);
        IEnumerable<int> GetDeletedIds(GetDeletedIds model);
        Task<int> CreateAsync(CreateUpcomingExpense model);
        Task UpdateAsync(UpdateUpcomingExpense model);
        Task DeleteAsync(int id, int userId);
        Task DeleteOldAsync(int userId);
    }
}
