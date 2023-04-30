namespace Accountant.Api.UpcomingExpenses

open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Persistence.Fs
open Accountant.Api
open CommonHandlers
open HandlerBase
open Models

module Handlers =

    let create: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let tr =
                startTransactionWithUser "POST /api/upcoming-expenses" "UpcomingExpenses/Handlers.create" ctx

            task {
                let! dto = ctx.BindJsonAsync<CreateUpcomingExpense>()
                dto.HttpContext <- ctx

                match Logic.validateCreate dto with
                | Success _ ->
                    let userId = getUserId ctx
                    let upcomingExpense = Logic.prepareForCreate dto userId

                    let connection = getDbConnection ctx
                    let! id = UpcomingExpensesRepository.create upcomingExpense connection tr

                    let! result = Successful.CREATED id next ctx

                    tr.Finish()

                    return result
                | Failure error -> return! RequestErrors.BAD_REQUEST error next ctx
            })

    let update: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let tr =
                startTransactionWithUser "PUT /api/upcoming-expenses" "UpcomingExpenses/Handlers.update" ctx

            task {
                let! dto = ctx.BindJsonAsync<UpdateUpcomingExpense>()
                dto.HttpContext <- ctx

                match Logic.validateUpdate dto with
                | Success _ ->
                    let userId = getUserId ctx
                    let upcomingExpense = Logic.prepareForUpdate dto userId

                    let connectionString = getConnectionString ctx
                    let! _ = UpcomingExpensesRepository.update upcomingExpense connectionString tr

                    let! result = Successful.NO_CONTENT next ctx

                    tr.Finish()

                    return result
                | Failure error -> return! RequestErrors.BAD_REQUEST error next ctx
            })

    let delete (id: int) : HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let tr =
                startTransactionWithUser "DELETE /api/upcoming-expenses" "UpcomingExpenses/Handlers.delete" ctx

            task {
                let userId = getUserId ctx

                let connectionString = getConnectionString ctx
                let! _ = UpcomingExpensesRepository.delete id userId connectionString tr

                let! result = Successful.NO_CONTENT next ctx

                tr.Finish()

                return result
            })
