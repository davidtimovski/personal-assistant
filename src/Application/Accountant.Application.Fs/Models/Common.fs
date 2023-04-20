namespace Accountant.Application.Fs.Models

open Accountant.Domain.Models

module Common =

    type SyncEntities =
        { Accounts: List<Account>
          Categories: List<Category>
          Transactions: List<Transaction>
          UpcomingExpenses: List<UpcomingExpense>
          Debts: List<Debt>
          AutomaticTransactions: List<AutomaticTransaction> }
