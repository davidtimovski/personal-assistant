namespace Accountant.Api.Accounts

open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Persistence.Fs
open Accountant.Api
open Api.Common.Fs
open CommonHandlers
open HandlerBase
open Models

module Handlers =

    let create: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let userId = getUserId ctx

            let tr =
                Metrics.startTransactionWithUser $"{ctx.Request.Method} /api/accounts" "Accounts/Handlers.create" userId

            task {
                let! dto = ctx.BindJsonAsync<CreateAccount>()

                match Logic.validateCreate dto with
                | Success _ ->
                    let account = Logic.prepareForCreate dto userId

                    let connection = getDbConnection ctx
                    let! id = AccountsRepository.create account connection tr

                    let! result = Successful.CREATED id next ctx

                    tr.Finish()

                    return result
                | Failure error -> return! RequestErrors.BAD_REQUEST error next ctx
            })

    let update: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let userId = getUserId ctx

            let tr =
                Metrics.startTransactionWithUser $"{ctx.Request.Method} /api/accounts" "Accounts/Handlers.update" userId

            task {
                let! dto = ctx.BindJsonAsync<UpdateAccount>()
                dto.HttpContext <- ctx

                match Logic.validateUpdate dto with
                | Success _ ->
                    let account = Logic.prepareForUpdate dto userId

                    let connectionString = getConnectionString ctx
                    let! _ = AccountsRepository.update account connectionString tr

                    let! result = Successful.NO_CONTENT next ctx

                    tr.Finish()

                    return result
                | Failure error -> return! RequestErrors.BAD_REQUEST error next ctx
            })

    let delete (id: int) : HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let userId = getUserId ctx

            let tr =
                Metrics.startTransactionWithUser
                    $"{ctx.Request.Method} /api/accounts/*"
                    "Accounts/Handlers.delete"
                    userId

            let connectionString = getConnectionString ctx

            let isMain = AccountsRepository.isMain id userId connectionString

            task {
                if isMain then
                    return! RequestErrors.BAD_REQUEST "Cannot delete main account" next ctx
                else
                    let! _ = AccountsRepository.delete id userId connectionString tr

                    let! result = Successful.NO_CONTENT next ctx

                    tr.Finish()

                    return result
            })
