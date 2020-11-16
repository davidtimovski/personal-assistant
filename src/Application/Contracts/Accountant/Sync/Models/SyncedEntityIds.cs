using System;
using System.Collections.Generic;
using System.Text;

namespace PersonalAssistant.Application.Contracts.Accountant.Sync.Models
{
    public class SyncedEntityIds
    {
        public SyncedEntityIds(int[] accountIds, int[] categoryIds, int[] transactionIds, int[] upcomingExpenseIds, int[] debtIds)
        {
            AccountIds = accountIds;
            CategoryIds = categoryIds;
            TransactionIds = transactionIds;
            UpcomingExpenseIds = upcomingExpenseIds;
            DebtIds = debtIds;
        }

        public int[] AccountIds { get; set; }
        public int[] CategoryIds { get; set; }
        public int[] TransactionIds { get; set; }
        public int[] UpcomingExpenseIds { get; set; }
        public int[] DebtIds { get; set; }
    }
}
