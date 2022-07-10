using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Contracts.Accountant.Sync;
using Domain.Entities.Accountant;

namespace Persistence.Repositories.Accountant;

public class SyncRepository : BaseRepository, ISyncRepository
{
    public SyncRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public async Task SyncAsync(IEnumerable<Account> accounts, IEnumerable<Category> categories, IEnumerable<Transaction> transactions, 
        IEnumerable<UpcomingExpense> upcomingExpenses, IEnumerable<Debt> debts, IEnumerable<AutomaticTransaction> automaticTransactions)
    {
        EFContext.Accounts.AddRange(accounts);
        EFContext.Categories.AddRange(categories);

        foreach (var transaction in transactions)
        {
            transaction.Date = transaction.Date.ToUniversalTime();
        }
        EFContext.Transactions.AddRange(transactions);

        foreach (var upcomingExpense in upcomingExpenses)
        {
            upcomingExpense.Date = upcomingExpense.Date.ToUniversalTime();
        }
        EFContext.UpcomingExpenses.AddRange(upcomingExpenses);

        EFContext.Debts.AddRange(debts);
        EFContext.AutomaticTransactions.AddRange(automaticTransactions);

        await EFContext.SaveChangesAsync();
    }
}
