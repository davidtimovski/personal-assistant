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
open Accountant.Application.Fs.Services

let getChanges: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        let accountsRepository = ctx.GetService<IAccountsRepository>()
        let categoryRepository = ctx.GetService<ICategoriesRepository>()
        let transactionService = ctx.GetService<ITransactionService>()
        let upcomingExpensesRepository = ctx.GetService<IUpcomingExpensesRepository>()
        let debtsRepository = ctx.GetService<IDebtsRepository>()
        let automaticTransactionsRepository = ctx.GetService<IAutomaticTransactionsRepository>()

        task {
            let! dto = ctx.BindJsonAsync<GetChangesDto>()
            let userId = getUserId ctx

            do! upcomingExpensesRepository.DeleteOldAsync(userId, UpcomingExpenseService.getFirstDayOfMonth)

            let getAll = new GetAll(userId, dto.LastSynced)

            let accounts = accountsRepository.GetAll(userId, dto.LastSynced) |> AccountService.mapAll
            let categories = categoryRepository.GetAll(userId, dto.LastSynced) |> CategoryService.mapAll
            let transactions = transactionService.GetAll(getAll)
            let upcomingExpenses = upcomingExpensesRepository.GetAll(userId, dto.LastSynced) |> UpcomingExpenseService.mapAll
            let debts = debtsRepository.GetAll(userId, dto.LastSynced) |> DebtService.mapAll
            let automaticTransactions = automaticTransactionsRepository.GetAll(userId, dto.LastSynced) |> AutomaticTransactionService.mapAll

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
                  DeletedAutomaticTransactionIds = automaticTransactionsRepository.GetDeletedIds(userId, dto.LastSynced)
                  AutomaticTransactions = automaticTransactions }

            return! Successful.OK changed next ctx
        }

let createEntities: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<SyncEntities>()

            if dto = null then
                return! RequestErrors.BAD_REQUEST "Bad request" next ctx
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

                return! Successful.OK changedEntitiesDto next ctx
        }
