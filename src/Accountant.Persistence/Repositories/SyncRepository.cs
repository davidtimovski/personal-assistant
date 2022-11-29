using Accountant.Application.Contracts.Sync;
using Domain.Accountant;

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

        List<Transaction> transactionsList = transactions.ToList();
        transactionsList.ForEach(x => x.Date = x.Date.ToUniversalTime());
        EFContext.Transactions.AddRange(transactionsList);

        List<UpcomingExpense> upcomingExpensesList = upcomingExpenses.ToList();
        upcomingExpensesList.ForEach(x => x.Date = x.Date.ToUniversalTime());
        EFContext.UpcomingExpenses.AddRange(upcomingExpensesList);

        EFContext.Debts.AddRange(debts);
        EFContext.AutomaticTransactions.AddRange(automaticTransactions);

        await EFContext.SaveChangesAsync();
    }
}
