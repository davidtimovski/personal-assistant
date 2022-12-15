module AccountHandlers

open Accountant.Application.Contracts.Accounts
open Accountant.Application.Fs.Models.Accounts
open Accountant.Application.Fs.Services
open Giraffe
open Microsoft.AspNetCore.Http
open CommonHandlers
open HandlerBase

let create: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateAccount>()
            let userId = getUserId ctx

            let account = AccountService.prepareForCreate dto userId

            let repository = ctx.GetService<IAccountsRepository>()
            let! id = repository.CreateAsync(account)

            return! Successful.CREATED id next ctx
        }
    )

let update: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<UpdateAccount>()
            let userId = getUserId ctx

            let account = AccountService.prepareForUpdate dto userId

            let repository = ctx.GetService<IAccountsRepository>()
            do! repository.UpdateAsync(account)

            return! Successful.NO_CONTENT next ctx
        }
    )

let delete (id: int) : HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let repository = ctx.GetService<IAccountsRepository>()
            let userId = getUserId ctx

            if repository.IsMain(id, userId) then
                return! RequestErrors.BAD_REQUEST "Cannot delete main account" next ctx
            else
                do! repository.DeleteAsync(id, userId)
                return! Successful.NO_CONTENT next ctx
        }
    )
