namespace Accountant.Api.Accounts

open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Persistence.Fs
open Accountant.Api
open CommonHandlers
open HandlerBase
open Models

module Handlers =

    let create: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! dto = ctx.BindJsonAsync<CreateAccount>()

                match Logic.validateCreate dto with
                | Success _ ->
                    let userId = getUserId ctx
                    let account = Logic.prepareForCreate dto userId

                    let connection = getDbConnection ctx
                    let! id = AccountsRepository.create account connection

                    return! Successful.CREATED id next ctx
                | Failure error -> return! RequestErrors.BAD_REQUEST error next ctx
            })

    let update: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! dto = ctx.BindJsonAsync<UpdateAccount>()
                dto.HttpContext <- ctx

                match Logic.validateUpdate dto with
                | Success _ ->
                    let userId = getUserId ctx
                    let account = Logic.prepareForUpdate dto userId

                    let connection = getDbConnection ctx
                    let! _ = AccountsRepository.update account connection

                    return! Successful.NO_CONTENT next ctx
                | Failure error -> return! RequestErrors.BAD_REQUEST error next ctx
            })

    let delete (id: int) : HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let userId = getUserId ctx
            let connection = getDbConnection ctx

            let isMain = AccountsRepository.isMain id userId connection

            task {
                if isMain then
                    return! RequestErrors.BAD_REQUEST "Cannot delete main account" next ctx
                else
                    let! _ = AccountsRepository.delete id userId connection

                    return! Successful.NO_CONTENT next ctx
            })
