﻿namespace Accountant.Persistence.Fs

open Npgsql
open ConnectionUtils
open Models

module SyncRepository =

    let sync
        (accounts: Account list)
        (categories: Category list)
        (transactions: Transaction list)
        (upcomingExpenses: UpcomingExpense list)
        (debts: Debt list)
        (automaticTransactions: AutomaticTransaction list)
        (userId: int)
        connectionString
        =
        task {
            use conn = new NpgsqlConnection(connectionString)
            conn.Open()

            use tr = conn.BeginTransaction()

            let accountIds =
                accounts
                |> List.map (fun account -> { account with UserId = userId } )
                |> List.map (fun account ->
                    AccountsRepository.create account (TransactionConnection(conn)) |> Async.AwaitTask
                )
                |> Async.Sequential
                |> Async.RunSynchronously

            let categoryIds =
                categories
                |> List.map (fun account -> { account with UserId = userId } )
                |> List.map (fun category ->
                    CategoriesRepository.create category (TransactionConnection(conn)) |> Async.AwaitTask
                )
                |> Async.Sequential
                |> Async.RunSynchronously

            let transactionIds =
                transactions
                |> List.map (fun transaction ->
                    TransactionsRepository.create transaction (TransactionConnection(conn)) (Some(tr)) |> Async.AwaitTask
                )
                |> Async.Sequential
                |> Async.RunSynchronously

            let upcomingExpenseIds =
                upcomingExpenses
                |> List.map (fun account -> { account with UserId = userId } )
                |> List.map (fun upcomingExpense ->
                    UpcomingExpensesRepository.create upcomingExpense (TransactionConnection(conn)) |> Async.AwaitTask
                )
                |> Async.Sequential
                |> Async.RunSynchronously

            let debtIds =
                debts
                |> List.map (fun account -> { account with UserId = userId } )
                |> List.map (fun debt ->
                     DebtsRepository.create debt (TransactionConnection(conn)) |> Async.AwaitTask
                )
                |> Async.Sequential
                |> Async.RunSynchronously

            let automaticTransactionIds =
                automaticTransactions
                |> List.map (fun account -> { account with UserId = userId } )
                |> List.map (fun automaticTransaction ->
                     AutomaticTransactionsRepository.create automaticTransaction (TransactionConnection(conn)) |> Async.AwaitTask
                )
                |> Async.Sequential
                |> Async.RunSynchronously

            tr.Commit()

            return (accountIds, categoryIds, transactionIds, upcomingExpenseIds, debtIds, automaticTransactionIds)
        }
