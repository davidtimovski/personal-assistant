module UpcomingExpenseHandlers

open Accountant.Application.Contracts.UpcomingExpenses
open Accountant.Application.Fs.Models.UpcomingExpenses
open Accountant.Application.Fs.Services
open Giraffe
open Microsoft.AspNetCore.Http
open HandlerBase

let create: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateUpcomingExpense>()
            let userId = getUserId ctx

            let upcomingExpense = UpcomingExpenseService.prepareForCreate dto userId

            let repository = ctx.GetService<IUpcomingExpensesRepository>()
            let! id = repository.CreateAsync(upcomingExpense)

            return! Successful.CREATED id next ctx
        }
    )

let update: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<UpdateUpcomingExpense>()
            let userId = getUserId ctx

            let upcomingExpense = UpcomingExpenseService.prepareForUpdate dto userId

            let repository = ctx.GetService<IUpcomingExpensesRepository>()
            do! repository.UpdateAsync(upcomingExpense)

            return! Successful.NO_CONTENT next ctx
        }
    )

let delete (id: int) : HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let repository = ctx.GetService<IUpcomingExpensesRepository>()
            let userId = getUserId ctx

            do! repository.DeleteAsync(id, userId)
            return! Successful.NO_CONTENT next ctx
        }
    )
