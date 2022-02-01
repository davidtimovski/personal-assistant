using System.Collections.Generic;
using Application.Contracts.Accountant.Accounts.Models;
using Application.Contracts.Accountant.Categories.Models;
using Application.Contracts.Accountant.Debts.Models;
using Application.Contracts.Accountant.Transactions.Models;
using Application.Contracts.Accountant.UpcomingExpenses.Models;

namespace Application.Contracts.Accountant.Sync.Models
{
    public class SyncEntities
    {
        public List<SyncAccount> Accounts { get; set; }
        public List<SyncCategory> Categories { get; set; }
        public List<SyncTransaction> Transactions { get; set; }
        public List<SyncUpcomingExpense> UpcomingExpenses { get; set; }
        public List<SyncDebt> Debts { get; set; }
    }
}
