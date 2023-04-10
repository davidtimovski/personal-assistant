module Models

open System
open System.Collections.Generic
open Accountant.Application.Fs.Models.Accounts
open Accountant.Application.Fs.Models.AutomaticTransactions
open Accountant.Application.Fs.Models.Categories
open Accountant.Application.Fs.Models.Debts
open Accountant.Application.Fs.Models.Transactions
open Accountant.Application.Fs.Models.UpcomingExpenses

type GetChangesDto = { LastSynced: DateTime }

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

type CreatedEntityIdPair = { LocalId: int; Id: int }

type CreatedEntityIdsDto =
    { AccountIdPairs: seq<CreatedEntityIdPair>
      CategoryIdPairs: seq<CreatedEntityIdPair>
      TransactionIdPairs: seq<CreatedEntityIdPair>
      UpcomingExpenseIdPairs: seq<CreatedEntityIdPair>
      DebtIdPairs: seq<CreatedEntityIdPair>
      AutomaticTransactionIdPairs: seq<CreatedEntityIdPair> }

