namespace Accountant.Api.UpcomingExpenses

open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Persistence
open Accountant.Api
open Api.Common.Fs
open CommonHandlers
open HandlerBase
open Models
open Logic

module Handlers =

    let create: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let userId = getUserId ctx

            let tr =
                Metrics.startTransactionWithUser
                    $"{ctx.Request.Method} /api/upcoming-expenses"
                    "UpcomingExpenses/Handlers.create"
                    userId

            task {
                let! request = ctx.BindJsonAsync<CreateUpcomingExpenseRequest>()
                request.HttpContext <- ctx

                match Logic.validateCreate request with
                | Success _ ->
                    let upcomingExpense = Logic.createRequestToEntity request userId

                    let connection = getDbConnection ctx
                    let! id = UpcomingExpensesRepository.create upcomingExpense connection tr

                    let! result = Successful.CREATED id next ctx

                    tr.Finish()

                    return result
                | Failure error ->
                    tr.Finish()
                    return! RequestErrors.BAD_REQUEST error next ctx
            })

    let update: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let userId = getUserId ctx

            let tr =
                Metrics.startTransactionWithUser
                    $"{ctx.Request.Method} /api/upcoming-expenses"
                    "UpcomingExpenses/Handlers.update"
                    userId

            task {
                let! request = ctx.BindJsonAsync<UpdateUpcomingExpenseRequest>()
                request.HttpContext <- ctx

                let connectionString = getConnectionString ctx
                let! existingUpcomingExpense = UpcomingExpensesRepository.get request.Id connectionString
                let! existingCategory = 
                    task {
                        match request.CategoryId with
                        | Some categoryId -> return! CategoriesRepository.get categoryId connectionString
                        | None -> return None
                    }

                let validationParams =
                    {
                        CurrentUserId = userId
                        Request = request
                        ExistingUpcomingExpense = existingUpcomingExpense
                        ExistingCategory = existingCategory
                    }

                match Logic.validateUpdate validationParams with
                | Success _ ->
                    let upcomingExpense = Logic.updateRequestToEntity request userId
                    let! _ = UpcomingExpensesRepository.update upcomingExpense connectionString tr

                    let! result = Successful.NO_CONTENT next ctx

                    tr.Finish()

                    return result
                | Failure error ->
                    tr.Finish()
                    return! RequestErrors.BAD_REQUEST error next ctx
            })

    let delete (id: int) : HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let userId = getUserId ctx

            let tr =
                Metrics.startTransactionWithUser
                    $"{ctx.Request.Method} /api/upcoming-expenses/*"
                    "UpcomingExpenses/Handlers.delete"
                    userId

            task {
                let connectionString = getConnectionString ctx
                let! _ = UpcomingExpensesRepository.delete id userId connectionString tr

                let! result = Successful.NO_CONTENT next ctx

                tr.Finish()

                return result
            })
