using Accountant.Application.Contracts.Accounts.Models;
using Accountant.Application.Contracts.AutomaticTransactions.Models;
using Accountant.Application.Contracts.Categories.Models;
using Accountant.Application.Contracts.Debts.Models;
using Accountant.Application.Contracts.Transactions.Models;
using Accountant.Application.Contracts.UpcomingExpenses.Models;

namespace Api.Models.Accountant.Sync;

public class ChangedVm
{
    public DateTime LastSynced { get; set; }

    public IEnumerable<int> DeletedAccountIds { get; set; }
    public IEnumerable<AccountDto> Accounts { get; set; }

    public IEnumerable<int> DeletedCategoryIds { get; set; }
    public IEnumerable<CategoryDto> Categories { get; set; }

    public IEnumerable<int> DeletedTransactionIds { get; set; }
    public IEnumerable<TransactionDto> Transactions { get; set; }

    public IEnumerable<int> DeletedUpcomingExpenseIds { get; set; }
    public IEnumerable<UpcomingExpenseDto> UpcomingExpenses { get; set; }

    public IEnumerable<int> DeletedDebtIds { get; set; }
    public IEnumerable<DebtDto> Debts { get; set; }

    public IEnumerable<int> DeletedAutomaticTransactionIds { get; set; }
    public IEnumerable<AutomaticTransactionDto> AutomaticTransactions { get; set; }
}
