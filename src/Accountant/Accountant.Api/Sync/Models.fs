namespace Accountant.Api.Sync

open System
open System.Collections.Generic
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

          DeletedAccountIds: IEnumerable<int>
          Accounts: seq<AccountDto>

          DeletedCategoryIds: IEnumerable<int>
          Categories: IEnumerable<CategoryDto>

          DeletedTransactionIds: IEnumerable<int>
          Transactions: IEnumerable<TransactionDto>

          DeletedUpcomingExpenseIds: IEnumerable<int>
          UpcomingExpenses: IEnumerable<UpcomingExpenseDto>

          DeletedDebtIds: IEnumerable<int>
          Debts: IEnumerable<DebtDto>

          DeletedAutomaticTransactionIds: IEnumerable<int>
          AutomaticTransactions: IEnumerable<AutomaticTransactionDto> }

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
