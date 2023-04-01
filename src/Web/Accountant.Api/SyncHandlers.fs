module SyncHandlers

open System
open Accountant.Domain.Models
open Accountant.Application.Contracts.Transactions
open Accountant.Application.Contracts.UpcomingExpenses
open Accountant.Application.Contracts.Sync
open Accountant.Application.Fs.Services
open Accountant.Application.Fs.Models.Sync
open Accountant.Persistence.Fs
open Giraffe
open Microsoft.AspNetCore.Http
open CommonHandlers
open Models

let getChanges: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        let upcomingExpensesRepository = ctx.GetService<IUpcomingExpensesRepository>()

        let connectionString = getConnectionString ctx

        task {
            let! dto = ctx.BindJsonAsync<GetChangesDto>()
            let userId = getUserId ctx

            do! upcomingExpensesRepository.DeleteOldAsync(userId, UpcomingExpenseService.getFirstDayOfMonth)

            let! deletedAccountIds = CommonRepository.getDeletedIds(userId, dto.LastSynced, EntityType.Account, connectionString) |> Async.AwaitTask
            let! accounts = AccountsRepository.getAll(userId, dto.LastSynced, connectionString) |> Async.AwaitTask
            let accountDtos =
                accounts
                |> AccountService.mapAll

            let! deletedCategoryIds = CommonRepository.getDeletedIds(userId, dto.LastSynced, EntityType.Category, connectionString) |> Async.AwaitTask
            let! categories = CategoriesRepository.getAll(userId, dto.LastSynced, connectionString) |> Async.AwaitTask
            let categoryDtos =
                categories
                |> CategoryService.mapAll

            let! deletedTransactionIds = CommonRepository.getDeletedIds(userId, dto.LastSynced, EntityType.Transaction, connectionString) |> Async.AwaitTask
            let! transactions = TransactionsRepository.getAll(userId, dto.LastSynced, connectionString) |> Async.AwaitTask
            let transactionDtos = 
                transactions
                |> TransactionService.mapAll

            let! deletedUpcomingExpenseIds = CommonRepository.getDeletedIds(userId, dto.LastSynced, EntityType.UpcomingExpense, connectionString) |> Async.AwaitTask
            let! upcomingExpenses = UpcomingExpensesRepository.getAll(userId, dto.LastSynced, connectionString) |> Async.AwaitTask
            let upcomingExpenseDtos =
                upcomingExpenses
                |> UpcomingExpenseService.mapAll

            let! deletedDebtIds = CommonRepository.getDeletedIds(userId, dto.LastSynced, EntityType.Debt, connectionString) |> Async.AwaitTask
            let! debts = DebtsRepository.getAll(userId, dto.LastSynced, connectionString) |> Async.AwaitTask
            let debtDtos = 
                debts
                |> DebtService.mapAll

            let! deletedAutomaticTransactionIds = CommonRepository.getDeletedIds(userId, dto.LastSynced, EntityType.AutomaticTransaction, connectionString) |> Async.AwaitTask
            let! automaticTransactions = AutomaticTransactionsRepository.getAll(userId, dto.LastSynced, connectionString) |> Async.AwaitTask
            let automaticTransactionDtos =
                automaticTransactions
                |> AutomaticTransactionService.mapAll

            let changed =
                { LastSynced = DateTime.Now
                  DeletedAccountIds = deletedAccountIds
                  Accounts = accountDtos
                  DeletedCategoryIds = deletedCategoryIds
                  Categories = categoryDtos
                  DeletedTransactionIds = deletedTransactionIds
                  Transactions = transactionDtos
                  DeletedUpcomingExpenseIds = deletedUpcomingExpenseIds
                  UpcomingExpenses = upcomingExpenseDtos
                  DeletedDebtIds = deletedDebtIds
                  Debts = debtDtos
                  DeletedAutomaticTransactionIds = deletedAutomaticTransactionIds
                  AutomaticTransactions = automaticTransactionDtos }

            return! Successful.OK changed next ctx
        }
    )

let createEntities: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<SyncEntities>()

            let syncRepository = ctx.GetService<ISyncRepository>()
            let userId = getUserId ctx

            let (accounts, categories, transactions, upcomingExpenses, debts, automaticTransactions) =
                SyncService.mapEntitiesForSync userId dto

            do!
                syncRepository.SyncAsync(
                    accounts,
                    categories,
                    transactions,
                    upcomingExpenses,
                    debts,
                    automaticTransactions
                )

            let syncedAccountIds = accounts |> Seq.map (fun x -> x.Id) |> Seq.toArray
            let syncedCategoryIds = categories |> Seq.map (fun x -> x.Id) |> Seq.toArray
            let syncedTransactionIds = transactions |> Seq.map (fun x -> x.Id) |> Seq.toArray

            let syncedUpcomingExpenseIds =
                upcomingExpenses |> Seq.map (fun x -> x.Id) |> Seq.toArray

            let syncedDebtIds = debts |> Seq.map (fun x -> x.Id) |> Seq.toArray

            let syncedAutomaticTransactionIds =
                automaticTransactions |> Seq.map (fun x -> x.Id) |> Seq.toArray

            let changedEntitiesDto =
                { AccountIdPairs =
                    seq { 0 .. syncedAccountIds.Length - 1 }
                    |> Seq.map (fun x ->
                        { LocalId = dto.Accounts[x].Id
                          Id = syncedAccountIds[x] })

                  CategoryIdPairs =
                      seq { 0 .. syncedCategoryIds.Length - 1 }
                      |> Seq.map (fun x ->
                          { LocalId = dto.Categories[x].Id
                            Id = syncedCategoryIds[x] })

                  TransactionIdPairs =
                      seq { 0 .. syncedTransactionIds.Length - 1 }
                      |> Seq.map (fun x ->
                          { LocalId = dto.Transactions[x].Id
                            Id = syncedTransactionIds[x] })

                  UpcomingExpenseIdPairs =
                      seq { 0 .. syncedUpcomingExpenseIds.Length - 1 }
                      |> Seq.map (fun x ->
                          { LocalId = dto.UpcomingExpenses[x].Id
                            Id = syncedUpcomingExpenseIds[x] })

                  DebtIdPairs =
                      seq { 0 .. syncedDebtIds.Length - 1 }
                      |> Seq.map (fun x ->
                          { LocalId = dto.Debts[x].Id
                            Id = syncedDebtIds[x] })

                  AutomaticTransactionIdPairs =
                      seq { 0 .. syncedAutomaticTransactionIds.Length - 1 }
                      |> Seq.map (fun x ->
                          { LocalId = dto.AutomaticTransactions[x].Id
                            Id = syncedAutomaticTransactionIds[x] }) }

            return! Successful.OK changedEntitiesDto next ctx
        }
    )
