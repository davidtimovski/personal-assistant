module AccountHandlers

open Accountant.Application.Contracts.Accounts
open Accountant.Application.Fs.Models.Accounts
open Accountant.Application.Fs.Services
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open HandlerBase

let create: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let! dto = ctx.BindJsonAsync<CreateAccount>()
                let userId = getUserId ctx

                let account = AccountService.prepareForCreate dto userId

                let repository = ctx.GetService<IAccountsRepository>()
                let! id = repository.CreateAsync(account)

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
                let! dto = ctx.BindJsonAsync<UpdateAccount>()
                let userId = getUserId ctx

                let account = AccountService.prepareForUpdate dto userId

                let repository = ctx.GetService<IAccountsRepository>()
                do! repository.UpdateAsync(account)

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
                let repository = ctx.GetService<IAccountsRepository>()
                let userId = getUserId ctx

                if repository.IsMain(id, userId) then
                    return! RequestErrors.BAD_REQUEST "Cannot delete main account." next ctx
                else
                    do! repository.DeleteAsync(id, userId)
                    return! Successful.NO_CONTENT next ctx
            with ex ->
                let logger = ctx.GetService<ILogger>()
                logger.LogError(ex, "Unexpected error in delete")

                return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }
