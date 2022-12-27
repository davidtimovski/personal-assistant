using Application.Domain.Accountant;

namespace Accountant.Application.Contracts.UpcomingExpenses;

public interface IUpcomingExpensesRepository
{
    IEnumerable<UpcomingExpense> GetAll(int userId, DateTime fromModifiedDate);
    IEnumerable<int> GetDeletedIds(int userId, DateTime fromDate);
    Task<int> CreateAsync(UpcomingExpense upcomingExpense);
    Task UpdateAsync(UpcomingExpense upcomingExpense);
    Task DeleteAsync(int id, int userId);
    Task DeleteOldAsync(int userId, DateTime before);
}
