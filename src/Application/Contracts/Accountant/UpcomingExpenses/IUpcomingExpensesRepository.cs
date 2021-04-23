using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities.Accountant;

namespace PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses
{
    public interface IUpcomingExpensesRepository
    {
        Task<IEnumerable<UpcomingExpense>> GetAllAsync(int userId, DateTime fromModifiedDate);
        Task<IEnumerable<int>> GetDeletedIdsAsync(int userId, DateTime fromDate);
        Task<int> CreateAsync(UpcomingExpense upcomingExpense, IDbConnection uowConn = null, IDbTransaction uowTransaction = null);
        Task UpdateAsync(UpcomingExpense upcomingExpense);
        Task DeleteAsync(int id, int userId);
        Task DeleteOldAsync(int userId, DateTime before);
    }
}
