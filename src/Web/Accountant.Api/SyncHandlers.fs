module SyncHandlers

open System
open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Domain.Models
open Accountant.Application.Contracts.Transactions
open Accountant.Application.Fs.Services
open Accountant.Application.Fs.Models.Common
open Accountant.Persistence.Fs
open Accountant.Persistence.Fs.CommonRepository
open CommonHandlers
open Models

let getChanges: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        let connectionString = getConnectionString ctx
        let dbContext = ctx.GetService<AccountantContext>()

        task {
            let! dto = ctx.BindJsonAsync<GetChangesDto>()
            let userId = getUserId ctx

            let! _ = UpcomingExpensesRepository.deleteOld userId UpcomingExpenseService.getFirstDayOfMonth dbContext

            let! deletedAccountIds =
                CommonRepository.getDeletedIds userId dto.LastSynced EntityType.Account connectionString

            let! accounts = AccountsRepository.getAll userId dto.LastSynced connectionString
            let accountDtos = accounts |> AccountService.mapAll

            let! deletedCategoryIds =
                CommonRepository.getDeletedIds userId dto.LastSynced EntityType.Category connectionString

            let! categories = CategoriesRepository.getAll userId dto.LastSynced connectionString
            let categoryDtos = categories |> CategoryService.mapAll

            let! deletedTransactionIds =
                CommonRepository.getDeletedIds userId dto.LastSynced EntityType.Transaction connectionString

            let! transactions = TransactionsRepository.getAll userId dto.LastSynced connectionString
            let transactionDtos = transactions |> TransactionService.mapAll

            let! deletedUpcomingExpenseIds =
                CommonRepository.getDeletedIds userId dto.LastSynced EntityType.UpcomingExpense connectionString

            let! upcomingExpenses = UpcomingExpensesRepository.getAll userId dto.LastSynced connectionString
            let upcomingExpenseDtos = upcomingExpenses |> UpcomingExpenseService.mapAll

            let! deletedDebtIds = CommonRepository.getDeletedIds userId dto.LastSynced EntityType.Debt connectionString
            let! debts = DebtsRepository.getAll userId dto.LastSynced connectionString
            let debtDtos = debts |> DebtService.mapAll

            let! deletedAutomaticTransactionIds =
                CommonRepository.getDeletedIds userId dto.LastSynced EntityType.AutomaticTransaction connectionString

            let! automaticTransactions = AutomaticTransactionsRepository.getAll userId dto.LastSynced connectionString

            let automaticTransactionDtos =
                automaticTransactions |> AutomaticTransactionService.mapAll

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
        })

let createEntities: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<SyncEntities>()

            let userId = getUserId ctx
            let dbContext = ctx.GetService<AccountantContext>()

            let! (accountIds, categoryIds, transactionIds, upcomingExpenseIds, debtIds, automaticTransactionIds) =
                CommonRepository.sync
                    dto.Accounts
                    dto.Categories
                    dto.Transactions
                    dto.UpcomingExpenses
                    dto.Debts
                    dto.AutomaticTransactions
                    userId
                    dbContext

            let changedEntitiesDto =
                { AccountIdPairs =
                    seq { 0 .. accountIds.Length - 1 }
                    |> Seq.map (fun x ->
                        { LocalId = dto.Accounts[x].Id
                          Id = accountIds[x] })

                  CategoryIdPairs =
                      seq { 0 .. categoryIds.Length - 1 }
                      |> Seq.map (fun x ->
                          { LocalId = dto.Categories[x].Id
                            Id = categoryIds[x] })

                  TransactionIdPairs =
                      seq { 0 .. transactionIds.Length - 1 }
                      |> Seq.map (fun x ->
                          { LocalId = dto.Transactions[x].Id
                            Id = transactionIds[x] })

                  UpcomingExpenseIdPairs =
                      seq { 0 .. upcomingExpenseIds.Length - 1 }
                      |> Seq.map (fun x ->
                          { LocalId = dto.UpcomingExpenses[x].Id
                            Id = upcomingExpenseIds[x] })

                  DebtIdPairs =
                      seq { 0 .. debtIds.Length - 1 }
                      |> Seq.map (fun x ->
                          { LocalId = dto.Debts[x].Id
                            Id = debtIds[x] })

                  AutomaticTransactionIdPairs =
                      seq { 0 .. automaticTransactionIds.Length - 1 }
                      |> Seq.map (fun x ->
                          { LocalId = dto.AutomaticTransactions[x].Id
                            Id = automaticTransactionIds[x] }) }

            return! Successful.OK changedEntitiesDto next ctx
        })
