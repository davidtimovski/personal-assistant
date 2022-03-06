using System;
using System.Collections.Generic;
using Application.Contracts.Accountant.Accounts.Models;
using Application.Contracts.Accountant.Categories.Models;
using Application.Contracts.Accountant.Debts.Models;
using Application.Contracts.Accountant.Transactions.Models;
using Application.Contracts.Accountant.UpcomingExpenses.Models;

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
}
