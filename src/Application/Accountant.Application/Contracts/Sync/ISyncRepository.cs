using Application.Domain.Accountant;

namespace Accountant.Application.Contracts.Sync;

public interface ISyncRepository
{
    Task SyncAsync(IEnumerable<Account> accounts, IEnumerable<Category> categories, IEnumerable<Transaction> transactions,
        IEnumerable<UpcomingExpense> upcomingExpenses, IEnumerable<Debt> debts, IEnumerable<AutomaticTransaction> automaticTransactions);
}
