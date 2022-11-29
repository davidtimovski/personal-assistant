namespace Accountant.Application.Fs.Models

open System
open Accounts
open AutomaticTransactions
open Categories
open Debts
open Transactions
open UpcomingExpenses

module Sync =

    type SyncEntities =
        { Accounts: List<SyncAccount>
          Categories: List<SyncCategory>
          Transactions: List<SyncTransaction>
          UpcomingExpenses: List<SyncUpcomingExpense>
          Debts: List<SyncDebt>
          AutomaticTransactions: List<SyncAutomaticTransaction> }
