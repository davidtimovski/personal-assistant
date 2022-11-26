module Models

open System
open System.Collections.Generic
open Accountant.Application.Contracts.Accounts.Models
open Accountant.Application.Contracts.Categories.Models
open Accountant.Application.Contracts.Transactions.Models
open Accountant.Application.Contracts.UpcomingExpenses.Models
open Accountant.Application.Contracts.Debts.Models
open Accountant.Application.Contracts.AutomaticTransactions.Models

type GetChangesDto = { LastSynced: DateTime }

type ChangedDto =
    { LastSynced: DateTime

      DeletedAccountIds: IEnumerable<int>
      Accounts: IEnumerable<AccountDto>

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

type CreatedEntityIdPair = { LocalId: int; Id: int }

type CreatedEntityIdsDto =
    { AccountIdPairs: seq<CreatedEntityIdPair>
      CategoryIdPairs: seq<CreatedEntityIdPair>
      TransactionIdPairs: seq<CreatedEntityIdPair>
      UpcomingExpenseIdPairs: seq<CreatedEntityIdPair>
      DebtIdPairs: seq<CreatedEntityIdPair>
      AutomaticTransactionIdPairs: seq<CreatedEntityIdPair> }

type ExportDto = { FileId: Guid }
