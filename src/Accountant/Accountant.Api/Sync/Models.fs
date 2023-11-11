namespace Accountant.Api.Sync

open System
open Accountant.Persistence.Models
open Accountant.Api.Accounts.Models
open Accountant.Api.AutomaticTransactions.Models
open Accountant.Api.Categories.Models
open Accountant.Api.Debts.Models
open Accountant.Api.Transactions.Models
open Accountant.Api.UpcomingExpenses.Models

module Models =

    type GetChangesRequest = { LastSynced: DateTime }

    type ChangedDto =
        { LastSynced: DateTime

          DeletedAccountIds: seq<int>
          Accounts: seq<AccountDto>

          DeletedCategoryIds: seq<int>
          Categories: seq<CategoryDto>

          DeletedTransactionIds: seq<int>
          Transactions: seq<TransactionDto>

          DeletedUpcomingExpenseIds: seq<int>
          UpcomingExpenses: seq<UpcomingExpenseDto>

          DeletedDebtIds: seq<int>
          Debts: seq<DebtDto>

          DeletedAutomaticTransactionIds: seq<int>
          AutomaticTransactions: seq<AutomaticTransactionDto> }

    type SyncEntitiesRequest =
        { Accounts: Account list
          Categories: Category list
          Transactions: Transaction list
          UpcomingExpenses: UpcomingExpense list
          Debts: Debt list
          AutomaticTransactions: AutomaticTransaction list }

    type CreatedEntityIdPair = { LocalId: int; Id: int }

    type CreatedEntityIdsDto =
        { AccountIdPairs: seq<CreatedEntityIdPair>
          CategoryIdPairs: seq<CreatedEntityIdPair>
          TransactionIdPairs: seq<CreatedEntityIdPair>
          UpcomingExpenseIdPairs: seq<CreatedEntityIdPair>
          DebtIdPairs: seq<CreatedEntityIdPair>
          AutomaticTransactionIdPairs: seq<CreatedEntityIdPair> }
