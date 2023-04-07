module AccountHandlers

open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Application.Fs.Models.Accounts
open Accountant.Application.Fs.Services
open Accountant.Persistence.Fs
open Accountant.Persistence.Fs.CommonRepository
open CommonHandlers

let create: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateAccount>()
            let userId = getUserId ctx

            let account = AccountService.prepareForCreate dto userId

            let dbContext = ctx.GetService<AccountantContext>()
            let! id = AccountsRepository.create account dbContext

            return! Successful.CREATED id next ctx
        }
    )

let update: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<UpdateAccount>()
            let userId = getUserId ctx

            let account = AccountService.prepareForUpdate dto userId

            let dbContext = ctx.GetService<AccountantContext>()
            let! _ = AccountsRepository.update account dbContext

            return! Successful.NO_CONTENT next ctx
        }
    )

let delete (id: int) : HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        let connectionString = getConnectionString ctx
        let userId = getUserId ctx

        let isMain = AccountsRepository.isMain id userId connectionString

        task {
            if isMain then
                return! RequestErrors.BAD_REQUEST "Cannot delete main account" next ctx
            else
                let dbContext = ctx.GetService<AccountantContext>()
                let! _ = AccountsRepository.delete id userId dbContext

                return! Successful.NO_CONTENT next ctx
        }
    )
