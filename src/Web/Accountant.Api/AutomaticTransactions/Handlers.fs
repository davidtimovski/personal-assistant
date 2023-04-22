namespace Accountant.Api.AutomaticTransactions

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
                let! dto = ctx.BindJsonAsync<CreateAutomaticTransaction>()
                dto.HttpContext <- ctx

                match Logic.validateCreate dto with
                | Success _ ->
                    let userId = getUserId ctx
                    let automaticTransaction = Logic.prepareForCreate dto userId

                    let connection = getDbConnection ctx
                    let! id = AutomaticTransactionsRepository.create automaticTransaction connection

                    return! Successful.CREATED id next ctx
                | Failure error -> return! RequestErrors.BAD_REQUEST error next ctx
            })

    let update: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! dto = ctx.BindJsonAsync<UpdateAutomaticTransaction>()
                dto.HttpContext <- ctx

                match Logic.validateUpdate dto with
                | Success _ ->
                    let userId = getUserId ctx
                    let automaticTransaction = Logic.prepareForUpdate dto userId

                    let connection = getDbConnection ctx
                    let! _ = AutomaticTransactionsRepository.update automaticTransaction connection

                    return! Successful.NO_CONTENT next ctx
                | Failure error -> return! RequestErrors.BAD_REQUEST error next ctx
            })

    let delete (id: int) : HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let userId = getUserId ctx
                let connection = getDbConnection ctx

                let! _ = AutomaticTransactionsRepository.delete id userId connection

                return! Successful.NO_CONTENT next ctx
            })
