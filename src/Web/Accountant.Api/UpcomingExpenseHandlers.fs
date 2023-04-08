module UpcomingExpenseHandlers

open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Application.Fs.Models.UpcomingExpenses
open Accountant.Application.Fs.Services
open Accountant.Persistence.Fs
open Accountant.Persistence.Fs.CommonRepository
open CommonHandlers

let create: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateUpcomingExpense>()
            let userId = getUserId ctx

            let upcomingExpense = UpcomingExpenseService.prepareForCreate dto userId

            let dbContext = ctx.GetService<AccountantContext>()
            let! id = UpcomingExpensesRepository.create upcomingExpense dbContext

            return! Successful.CREATED id next ctx
        }
    )

let update: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<UpdateUpcomingExpense>()
            let userId = getUserId ctx

            let upcomingExpense = UpcomingExpenseService.prepareForUpdate dto userId

            let dbContext = ctx.GetService<AccountantContext>()
            let! _ = UpcomingExpensesRepository.update upcomingExpense dbContext

            return! Successful.NO_CONTENT next ctx
        }
    )

let delete (id: int) : HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let userId = getUserId ctx

            let dbContext = ctx.GetService<AccountantContext>()
            let! _ = UpcomingExpensesRepository.delete id userId dbContext

            return! Successful.NO_CONTENT next ctx
        }
    )
