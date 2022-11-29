module SyncHandlers

open System
open Accountant.Application.Contracts.Accounts
open Accountant.Application.Contracts.AutomaticTransactions
open Accountant.Application.Contracts.Categories
open Accountant.Application.Contracts.Common.Models
open Accountant.Application.Contracts.Debts
open Accountant.Application.Contracts.Transactions
open Accountant.Application.Contracts.UpcomingExpenses
open Accountant.Application.Contracts.Sync
open Accountant.Application.Fs.Services
open Accountant.Application.Fs.Models.Sync
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open HandlerBase
open Models

let getChanges: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        let accountsRepository = ctx.GetService<IAccountsRepository>()
        let categoryRepository = ctx.GetService<ICategoriesRepository>()
        let transactionService = ctx.GetService<ITransactionService>()
        let upcomingExpensesRepository = ctx.GetService<IUpcomingExpensesRepository>()
        let debtsRepository = ctx.GetService<IDebtsRepository>()

        let automaticTransactionsRepository =
            ctx.GetService<IAutomaticTransactionsRepository>()

        task {
            try
                let! dto = ctx.BindJsonAsync<GetChangesDto>()
                let userId = getUserId ctx

                do! upcomingExpensesRepository.DeleteOldAsync(userId, UpcomingExpenseService.getFirstDayOfMonth)

                let accounts =
                    accountsRepository.GetAll(userId, dto.LastSynced) |> AccountService.mapAll

                let categories =
                    categoryRepository.GetAll(userId, dto.LastSynced) |> CategoryService.mapAll

                let transactions = transactionService.GetAll(new GetAll(userId, dto.LastSynced))

                let upcomingExpenses =
                    upcomingExpensesRepository.GetAll(userId, dto.LastSynced)
                    |> UpcomingExpenseService.mapAll

                let debts = debtsRepository.GetAll(userId, dto.LastSynced) |> DebtService.mapAll

                let automaticTransactions =
                    automaticTransactionsRepository.GetAll(userId, dto.LastSynced)
                    |> AutomaticTransactionService.mapAll

                let getDeletedIds = GetDeletedIds(userId, dto.LastSynced)

                let changed =
                    { LastSynced = DateTime.Now
                      DeletedAccountIds = accountsRepository.GetDeletedIds(userId, dto.LastSynced)
                      Accounts = accounts
                      DeletedCategoryIds = categoryRepository.GetDeletedIds(userId, dto.LastSynced)
                      Categories = categories
                      DeletedTransactionIds = transactionService.GetDeletedIds(getDeletedIds)
                      Transactions = transactions
                      DeletedUpcomingExpenseIds = upcomingExpensesRepository.GetDeletedIds(userId, dto.LastSynced)
                      UpcomingExpenses = upcomingExpenses
                      DeletedDebtIds = debtsRepository.GetDeletedIds(userId, dto.LastSynced)
                      Debts = debts
                      DeletedAutomaticTransactionIds =
                        automaticTransactionsRepository.GetDeletedIds(userId, dto.LastSynced)
                      AutomaticTransactions = automaticTransactions }

                return! Successful.OK changed next ctx
            with ex ->
                let logger = ctx.GetService<ILogger<HttpContext>>()
                logger.LogError(ex, "Unexpected error in create")

                return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }

let createEntities: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
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
            with ex ->
                let logger = ctx.GetService<ILogger<HttpContext>>()
                logger.LogError(ex, "Unexpected error in create")

                return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }
