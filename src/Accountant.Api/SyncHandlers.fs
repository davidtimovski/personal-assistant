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
open Accountant.Application.Contracts.Sync.Models
open Giraffe
open Microsoft.AspNetCore.Http
open HandlerBase
open Models

let getChanges: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        let accountService = ctx.GetService<IAccountService>()
        let categoryService = ctx.GetService<ICategoryService>()
        let transactionService = ctx.GetService<ITransactionService>()
        let upcomingExpenseService = ctx.GetService<IUpcomingExpenseService>()
        let debtService = ctx.GetService<IDebtService>()
        let automaticTransactionService = ctx.GetService<IAutomaticTransactionService>()

        task {
            let! dto = ctx.BindJsonAsync<GetChangesDto>()
            let userId = getUserId ctx

            do! upcomingExpenseService.DeleteOldAsync(userId)

            let getAll = new GetAll(userId, dto.LastSynced)

            let accounts = accountService.GetAll(getAll)
            let categories = categoryService.GetAll(getAll)
            let transactions = transactionService.GetAll(getAll)
            let upcomingExpenses = upcomingExpenseService.GetAll(getAll)
            let debts = debtService.GetAll(getAll)
            let automaticTransactions = automaticTransactionService.GetAll(getAll)

            let getDeletedIds = GetDeletedIds(userId, dto.LastSynced)

            let changed =
                { LastSynced = DateTime.Now
                  DeletedAccountIds = accountService.GetDeletedIds(getDeletedIds)
                  Accounts = accounts
                  DeletedCategoryIds = categoryService.GetDeletedIds(getDeletedIds)
                  Categories = categories
                  DeletedTransactionIds = transactionService.GetDeletedIds(getDeletedIds)
                  Transactions = transactions
                  DeletedUpcomingExpenseIds = upcomingExpenseService.GetDeletedIds(getDeletedIds)
                  UpcomingExpenses = upcomingExpenses
                  DeletedDebtIds = debtService.GetDeletedIds(getDeletedIds)
                  Debts = debts
                  DeletedAutomaticTransactionIds = automaticTransactionService.GetDeletedIds(getDeletedIds)
                  AutomaticTransactions = automaticTransactions }

            return! (Successful.OK changed) next ctx
        }

let createEntities: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<SyncEntities>()

            if dto = null then
                return! (RequestErrors.BAD_REQUEST "Bad request") next ctx
            else
                let syncService = ctx.GetService<ISyncService>()
                let userId = getUserId ctx

                dto.Accounts.ForEach(fun x -> x.UserId <- userId)
                dto.Categories.ForEach(fun x -> x.UserId <- userId)
                dto.UpcomingExpenses.ForEach(fun x -> x.UserId <- userId)
                dto.Debts.ForEach(fun x -> x.UserId <- userId)
                dto.AutomaticTransactions.ForEach(fun x -> x.UserId <- userId)

                let! syncedEntityIds = syncService.SyncEntitiesAsync(dto)

                let changedEntitiesDto =
                    { AccountIdPairs =
                        seq { 0 .. syncedEntityIds.AccountIds.Length - 1 }
                        |> Seq.map (fun x ->
                            { LocalId = dto.Accounts[x].Id
                              Id = syncedEntityIds.AccountIds[x] })

                      CategoryIdPairs =
                          seq { 0 .. syncedEntityIds.CategoryIds.Length - 1 }
                          |> Seq.map (fun x ->
                              { LocalId = dto.Categories[x].Id
                                Id = syncedEntityIds.CategoryIds[x] })

                      TransactionIdPairs =
                          seq { 0 .. syncedEntityIds.TransactionIds.Length - 1 }
                          |> Seq.map (fun x ->
                              { LocalId = dto.Transactions[x].Id
                                Id = syncedEntityIds.TransactionIds[x] })

                      UpcomingExpenseIdPairs =
                          seq { 0 .. syncedEntityIds.UpcomingExpenseIds.Length - 1 }
                          |> Seq.map (fun x ->
                              { LocalId = dto.UpcomingExpenses[x].Id
                                Id = syncedEntityIds.UpcomingExpenseIds[x] })

                      DebtIdPairs =
                          seq { 0 .. syncedEntityIds.DebtIds.Length - 1 }
                          |> Seq.map (fun x ->
                              { LocalId = dto.Debts[x].Id
                                Id = syncedEntityIds.DebtIds[x] })

                      AutomaticTransactionIdPairs =
                          seq { 0 .. syncedEntityIds.AutomaticTransactionIds.Length - 1 }
                          |> Seq.map (fun x ->
                              { LocalId = dto.AutomaticTransactions[x].Id
                                Id = syncedEntityIds.AutomaticTransactionIds[x] }) }

                return! (Successful.OK changedEntitiesDto) next ctx
        }
