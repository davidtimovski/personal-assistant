module AutomaticTransactionHandlers

open Accountant.Application.Contracts.AutomaticTransactions
open Accountant.Application.Fs.Models.AutomaticTransactions
open Accountant.Application.Fs.Services
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open HandlerBase

let create: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let! dto = ctx.BindJsonAsync<CreateAutomaticTransaction>()
                let userId = getUserId ctx

                let automaticTransaction = AutomaticTransactionService.prepareForCreate dto userId

                let repository = ctx.GetService<IAutomaticTransactionsRepository>()
                let! id = repository.CreateAsync(automaticTransaction)

                return! Successful.CREATED id next ctx
            with ex ->
               let logger = ctx.GetService<ILogger>()
               logger.LogError(ex, "Unexpected error in create")

               return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }

let update: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let! dto = ctx.BindJsonAsync<UpdateAutomaticTransaction>()
                let userId = getUserId ctx

                let automaticTransaction = AutomaticTransactionService.prepareForUpdate dto userId

                let repository = ctx.GetService<IAutomaticTransactionsRepository>()
                do! repository.UpdateAsync(automaticTransaction)

                return! Successful.NO_CONTENT next ctx
            with ex ->
                let logger = ctx.GetService<ILogger>()
                logger.LogError(ex, "Unexpected error in update")

                return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }

let delete (id: int) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let repository = ctx.GetService<IAutomaticTransactionsRepository>()
                let userId = getUserId ctx

                do! repository.DeleteAsync(id, userId)
                return! Successful.NO_CONTENT next ctx
            with ex ->
                let logger = ctx.GetService<ILogger>()
                logger.LogError(ex, "Unexpected error in delete")

                return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }
