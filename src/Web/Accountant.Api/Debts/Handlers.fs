namespace Accountant.Api.Debts

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
                let! dto = ctx.BindJsonAsync<CreateDebt>()

                match Logic.validateCreate dto with
                | Success _ ->
                    let userId = getUserId ctx
                    let debt = Logic.prepareForCreate dto userId

                    let connection = getDbConnection ctx
                    let! id = DebtsRepository.create debt connection

                    return! Successful.CREATED id next ctx
                | Failure error -> return! RequestErrors.BAD_REQUEST error next ctx
            })

    let createMerged: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! dto = ctx.BindJsonAsync<CreateDebt>()

                match Logic.validateCreate dto with
                | Success _ ->
                    let userId = getUserId ctx
                    let debt = Logic.prepareForCreateMerged dto userId

                    let connectionString = getConnectionString ctx
                    let! id = DebtsRepository.createMerged debt connectionString

                    return! Successful.CREATED id next ctx
                | Failure error -> return! RequestErrors.BAD_REQUEST error next ctx
            })

    let update: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! dto = ctx.BindJsonAsync<UpdateDebt>()
                dto.HttpContext <- ctx

                match Logic.validateUpdate dto with
                | Success _ ->
                    let userId = getUserId ctx
                    let debt = Logic.prepareForUpdate dto userId

                    let connectionString = getConnectionString ctx
                    let! _ = DebtsRepository.update debt connectionString

                    return! Successful.NO_CONTENT next ctx
                | Failure error -> return! RequestErrors.BAD_REQUEST error next ctx
            })

    let delete (id: int) : HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let userId = getUserId ctx

                let connectionString = getConnectionString ctx
                let! _ = DebtsRepository.delete id userId connectionString

                return! Successful.NO_CONTENT next ctx
            })
