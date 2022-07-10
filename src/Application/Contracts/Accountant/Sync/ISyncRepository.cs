using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities.Accountant;

namespace Application.Contracts.Accountant.Sync;

public interface ISyncRepository
{
    Task SyncAsync(IEnumerable<Account> accounts, IEnumerable<Category> categories, IEnumerable<Transaction> transactions, 
        IEnumerable<UpcomingExpense> upcomingExpenses, IEnumerable<Debt> debts, IEnumerable<AutomaticTransaction> automaticTransactions);
}
