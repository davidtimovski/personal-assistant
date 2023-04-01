using Application.Domain.Accountant;

namespace Accountant.Application.Contracts.UpcomingExpenses;

public interface IUpcomingExpensesRepository
{
    Task<int> CreateAsync(UpcomingExpense upcomingExpense);
    Task UpdateAsync(UpcomingExpense upcomingExpense);
    Task DeleteAsync(int id, int userId);
    Task DeleteOldAsync(int userId, DateTime before);
}
