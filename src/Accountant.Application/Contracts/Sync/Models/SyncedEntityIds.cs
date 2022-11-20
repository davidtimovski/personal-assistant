namespace Accountant.Application.Contracts.Sync.Models;

public class SyncedEntityIds
{
    public SyncedEntityIds(int[] accountIds, int[] categoryIds, int[] transactionIds, int[] upcomingExpenseIds, int[] debtIds, int[] automaticTransactionIds)
    {
        AccountIds = accountIds;
        CategoryIds = categoryIds;
        TransactionIds = transactionIds;
        UpcomingExpenseIds = upcomingExpenseIds;
        DebtIds = debtIds;
        AutomaticTransactionIds = automaticTransactionIds;
    }

    public int[] AccountIds { get; }
    public int[] CategoryIds { get; }
    public int[] TransactionIds { get; }
    public int[] UpcomingExpenseIds { get; }
    public int[] DebtIds { get; }
    public int[] AutomaticTransactionIds { get; }
}
