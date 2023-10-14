namespace Accountant.Api.Sync

open System
open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Persistence
open Accountant.Api
open Api.Common.Fs
open CommonHandlers
open Models

module Handlers =

    let getChanges: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let userId = getUserId ctx

            let tr =
                Metrics.startTransactionWithUser
                    $"{ctx.Request.Method} /api/sync/changes"
                    "Sync/Handlers.getChanges"
                    userId

            let connectionString = getConnectionString ctx

            task {
                let! request = ctx.BindJsonAsync<GetChangesRequest>()

                let! _ =
                    UpcomingExpensesRepository.deleteOld
                        userId
                        UpcomingExpenses.Logic.getFirstDayOfMonth
                        connectionString
                        tr

                let! deletedAccountIds =
                    CommonRepository.getDeletedIds userId request.LastSynced EntityType.Account connectionString

                let! accounts = AccountsRepository.getAll userId request.LastSynced connectionString
                let accountDtos = accounts |> Accounts.Logic.mapAll

                let! deletedCategoryIds =
                    CommonRepository.getDeletedIds userId request.LastSynced EntityType.Category connectionString

                let! categories = CategoriesRepository.getAll userId request.LastSynced connectionString
                let categoryDtos = categories |> Categories.Logic.mapAll

                let! deletedTransactionIds =
                    CommonRepository.getDeletedIds userId request.LastSynced EntityType.Transaction connectionString

                let! transactions = TransactionsRepository.getAll userId request.LastSynced connectionString
                let transactionDtos = transactions |> Transactions.Logic.mapAll

                let! deletedUpcomingExpenseIds =
                    CommonRepository.getDeletedIds userId request.LastSynced EntityType.UpcomingExpense connectionString

                let! upcomingExpenses = UpcomingExpensesRepository.getAll userId request.LastSynced connectionString
                let upcomingExpenseDtos = upcomingExpenses |> UpcomingExpenses.Logic.mapAll

                let! deletedDebtIds =
                    CommonRepository.getDeletedIds userId request.LastSynced EntityType.Debt connectionString

                let! debts = DebtsRepository.getAll userId request.LastSynced connectionString
                let debtDtos = debts |> Debts.Logic.mapAll

                let! deletedAutomaticTransactionIds =
                    CommonRepository.getDeletedIds
                        userId
                        request.LastSynced
                        EntityType.AutomaticTransaction
                        connectionString

                let! automaticTransactions =
                    AutomaticTransactionsRepository.getAll userId request.LastSynced connectionString

                let automaticTransactionDtos =
                    automaticTransactions |> AutomaticTransactions.Logic.mapAll

                let changed =
                    { LastSynced = DateTime.UtcNow
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

                let! result = Successful.OK changed next ctx

                tr.Finish()

                return result
            })

    let createEntities: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let userId = getUserId ctx

            let tr =
                Metrics.startTransactionWithUser
                    $"{ctx.Request.Method} /api/sync/create-entities"
                    "Sync/Handlers.createEntities"
                    userId

            task {
                let! request = ctx.BindJsonAsync<SyncEntitiesRequest>()

                let connectionString = getConnectionString ctx

                let (accountIds, categoryIds, transactionIds, upcomingExpenseIds, debtIds, automaticTransactionIds) =
                    SyncRepository.sync
                        request.Accounts
                        request.Categories
                        request.Transactions
                        request.UpcomingExpenses
                        request.Debts
                        request.AutomaticTransactions
                        userId
                        connectionString
                        tr

                let changedEntitiesDto =
                    { AccountIdPairs =
                        seq { 0 .. accountIds.Length - 1 }
                        |> Seq.map (fun x ->
                            { LocalId = request.Accounts[x].Id
                              Id = accountIds[x] })

                      CategoryIdPairs =
                          seq { 0 .. categoryIds.Length - 1 }
                          |> Seq.map (fun x ->
                              { LocalId = request.Categories[x].Id
                                Id = categoryIds[x] })

                      TransactionIdPairs =
                          seq { 0 .. transactionIds.Length - 1 }
                          |> Seq.map (fun x ->
                              { LocalId = request.Transactions[x].Id
                                Id = transactionIds[x] })

                      UpcomingExpenseIdPairs =
                          seq { 0 .. upcomingExpenseIds.Length - 1 }
                          |> Seq.map (fun x ->
                              { LocalId = request.UpcomingExpenses[x].Id
                                Id = upcomingExpenseIds[x] })

                      DebtIdPairs =
                          seq { 0 .. debtIds.Length - 1 }
                          |> Seq.map (fun x ->
                              { LocalId = request.Debts[x].Id
                                Id = debtIds[x] })

                      AutomaticTransactionIdPairs =
                          seq { 0 .. automaticTransactionIds.Length - 1 }
                          |> Seq.map (fun x ->
                              { LocalId = request.AutomaticTransactions[x].Id
                                Id = automaticTransactionIds[x] }) }

                let! result = Successful.OK changedEntitiesDto next ctx

                tr.Finish()

                return result
            })
