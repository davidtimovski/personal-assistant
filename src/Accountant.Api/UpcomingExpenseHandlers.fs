module UpcomingExpenseHandlers

open Accountant.Application.Contracts.UpcomingExpenses
open Accountant.Application.Fs.Models.UpcomingExpenses
open Accountant.Application.Fs.Services
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open HandlerBase

let create: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let! dto = ctx.BindJsonAsync<CreateUpcomingExpense>()
                let userId = getUserId ctx

                let upcomingExpense = UpcomingExpenseService.prepareForCreate dto userId

                let repository = ctx.GetService<IUpcomingExpensesRepository>()
                let! id = repository.CreateAsync(upcomingExpense)

                return! Successful.CREATED id next ctx
            with ex ->
               let logger = ctx.GetService<ILogger<HttpContext>>()
               logger.LogError(ex, "Unexpected error in create")

               return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }

let update: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let! dto = ctx.BindJsonAsync<UpdateUpcomingExpense>()
                let userId = getUserId ctx

                let upcomingExpense = UpcomingExpenseService.prepareForUpdate dto userId

                let repository = ctx.GetService<IUpcomingExpensesRepository>()
                do! repository.UpdateAsync(upcomingExpense)

                return! Successful.NO_CONTENT next ctx
            with ex ->
               let logger = ctx.GetService<ILogger<HttpContext>>()
               logger.LogError(ex, "Unexpected error in update")

               return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }

let delete (id: int) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let repository = ctx.GetService<IUpcomingExpensesRepository>()
                let userId = getUserId ctx

                do! repository.DeleteAsync(id, userId)
                return! Successful.NO_CONTENT next ctx
            with ex ->
                let logger = ctx.GetService<ILogger<HttpContext>>()
                logger.LogError(ex, "Unexpected error in delete")

                return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }
