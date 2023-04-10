module UpcomingExpenseHandlers

open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Application.Fs.Models.UpcomingExpenses
open Accountant.Application.Fs.Services
open Accountant.Persistence.Fs
open CommonHandlers
open HandlerBase

let create: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateUpcomingExpense>()
            let userId = getUserId ctx

            let upcomingExpense = UpcomingExpenseService.prepareForCreate dto userId

            let connection = getDbConnection ctx
            let! id = UpcomingExpensesRepository.create upcomingExpense connection

            return! Successful.CREATED id next ctx
        }
    )

let update: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<UpdateUpcomingExpense>()
            let userId = getUserId ctx

            let upcomingExpense = UpcomingExpenseService.prepareForUpdate dto userId

            let connection = getDbConnection ctx
            let! _ = UpcomingExpensesRepository.update upcomingExpense connection

            return! Successful.NO_CONTENT next ctx
        }
    )

let delete (id: int) : HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let userId = getUserId ctx

            let connection = getDbConnection ctx
            let! _ = UpcomingExpensesRepository.delete id userId connection

            return! Successful.NO_CONTENT next ctx
        }
    )
