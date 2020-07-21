using System.Collections.Generic;
using PersonalAssistant.Application.Contracts.Accountant.Accounts.Models;
using PersonalAssistant.Application.Contracts.Accountant.Categories.Models;
using PersonalAssistant.Application.Contracts.Accountant.Debts.Models;
using PersonalAssistant.Application.Contracts.Accountant.Transactions.Models;
using PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses.Models;

namespace PersonalAssistant.Application.Contracts.Accountant.Sync.Models
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
