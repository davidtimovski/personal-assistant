namespace Accountant.Api.AutomaticTransactions

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
                Metrics.startTransactionWithUser
                    $"{ctx.Request.Method} /api/automatic-transactions"
                    "AutomaticTransactions/Handlers.create"
                    userId

            task {
                let! dto = ctx.BindJsonAsync<CreateAutomaticTransaction>()
                dto.HttpContext <- ctx

                match Logic.validateCreate dto with
                | Success _ ->
                    let automaticTransaction = Logic.prepareForCreate dto userId

                    let connection = getDbConnection ctx
                    let! id = AutomaticTransactionsRepository.create automaticTransaction connection tr

                    let! result = Successful.CREATED id next ctx

                    tr.Finish()

                    return result
                | Failure error -> return! RequestErrors.BAD_REQUEST error next ctx
            })

    let update: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let userId = getUserId ctx

            let tr =
                Metrics.startTransactionWithUser
                    $"{ctx.Request.Method} /api/automatic-transactions"
                    "AutomaticTransactions/Handlers.update"
                    userId

            task {
                let! dto = ctx.BindJsonAsync<UpdateAutomaticTransaction>()
                dto.HttpContext <- ctx

                match Logic.validateUpdate dto with
                | Success _ ->
                    let automaticTransaction = Logic.prepareForUpdate dto userId

                    let connectionString = getConnectionString ctx
                    let! _ = AutomaticTransactionsRepository.update automaticTransaction connectionString tr

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
                    $"{ctx.Request.Method} /api/automatic-transactions/*"
                    "AutomaticTransactions/Handlers.delete"
                    userId

            task {
                let connectionString = getConnectionString ctx

                let! _ = AutomaticTransactionsRepository.delete id userId connectionString tr

                let! result = Successful.NO_CONTENT next ctx

                tr.Finish()

                return result
            })
