module AccountHandlers

open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Application.Fs.Models.Accounts
open Accountant.Application.Fs.Services
open Accountant.Persistence.Fs
open CommonHandlers
open HandlerBase

let create: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateAccount>()
            let userId = getUserId ctx

            let account = AccountService.prepareForCreate dto userId

            let connection = getDbConnection ctx
            let! id = AccountsRepository.create account connection

            return! Successful.CREATED id next ctx
        }
    )

let update: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<UpdateAccount>()
            let userId = getUserId ctx

            let account = AccountService.prepareForUpdate dto userId

            let connection = getDbConnection ctx
            let! _ = AccountsRepository.update account connection

            return! Successful.NO_CONTENT next ctx
        }
    )

let delete (id: int) : HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        let connection = getDbConnection ctx
        let userId = getUserId ctx

        let isMain = AccountsRepository.isMain id userId connection

        task {
            if isMain then
                return! RequestErrors.BAD_REQUEST "Cannot delete main account" next ctx
            else
                let! _ = AccountsRepository.delete id userId connection

                return! Successful.NO_CONTENT next ctx
        }
    )
