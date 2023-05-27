namespace Accountant.Persistence.Fs

open Npgsql
open ConnectionUtils
open Models
open Sentry
type Transaction = Accountant.Persistence.Fs.Models.Transaction

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
        (metricsSpan: ISpan)
        =
        let metric = metricsSpan.StartChild("SyncRepository.sync")

        use conn = new NpgsqlConnection(connectionString)
        conn.Open()

        use tr = conn.BeginTransaction()

        let accountIds =
            accounts
            |> List.map (fun account -> { account with UserId = userId } )
            |> List.map (fun account ->
                AccountsRepository.create account (TransactionConnection(conn)) metric |> Async.AwaitTask
            )
            |> Async.Sequential
            |> Async.RunSynchronously

        let categoryIds =
            categories
            |> List.map (fun account -> { account with UserId = userId } )
            |> List.map (fun category ->
                CategoriesRepository.create category (TransactionConnection(conn)) metric |> Async.AwaitTask
            )
            |> Async.Sequential
            |> Async.RunSynchronously

        let transactionIds =
            transactions
            |> List.map (fun transaction ->
                TransactionsRepository.create transaction (TransactionConnection(conn)) (Some(tr)) metric |> Async.AwaitTask
            )
            |> Async.Sequential
            |> Async.RunSynchronously

        let upcomingExpenseIds =
            upcomingExpenses
            |> List.map (fun account -> { account with UserId = userId } )
            |> List.map (fun upcomingExpense ->
                UpcomingExpensesRepository.create upcomingExpense (TransactionConnection(conn)) metric |> Async.AwaitTask
            )
            |> Async.Sequential
            |> Async.RunSynchronously

        let debtIds =
            debts
            |> List.map (fun account -> { account with UserId = userId } )
            |> List.map (fun debt ->
                    DebtsRepository.create debt (TransactionConnection(conn)) metric |> Async.AwaitTask
            )
            |> Async.Sequential
            |> Async.RunSynchronously

        let automaticTransactionIds =
            automaticTransactions
            |> List.map (fun account -> { account with UserId = userId } )
            |> List.map (fun automaticTransaction ->
                    AutomaticTransactionsRepository.create automaticTransaction (TransactionConnection(conn)) metric |> Async.AwaitTask
            )
            |> Async.Sequential
            |> Async.RunSynchronously

        tr.Commit()

        metric.Finish()

        (accountIds, categoryIds, transactionIds, upcomingExpenseIds, debtIds, automaticTransactionIds)
