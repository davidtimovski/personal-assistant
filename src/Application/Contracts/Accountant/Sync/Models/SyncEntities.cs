using System.Collections.Generic;

namespace Application.Contracts.Accountant.Sync.Models;

public class SyncEntities
{
    public List<SyncAccount> Accounts { get; set; }
    public List<SyncCategory> Categories { get; set; }
    public List<SyncTransaction> Transactions { get; set; }
    public List<SyncUpcomingExpense> UpcomingExpenses { get; set; }
    public List<SyncDebt> Debts { get; set; }
    public List<SyncAutomaticTransaction> AutomaticTransactions { get; set; }
}
