namespace Accountant.Api.Debts

open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Persistence
open Accountant.Api
open Api.Common.Fs
open CommonHandlers
open HandlerBase
open Models
open Logic
open Sentry

module Handlers =

    let create: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let userId = getUserId ctx

            let tr =
                Metrics.startTransactionWithUser $"{ctx.Request.Method} /api/debts" "Debts/Handlers.create" userId

            task {
                try
                    let! request = ctx.BindJsonAsync<CreateDebtRequest>()

                    match Logic.validateCreate request with
                    | Success _ ->
                        let debt = Logic.createRequestToEntity request userId

                        let connection = getDbConnection ctx
                        let! id = DebtsRepository.create debt connection tr

                        return! Successful.CREATED id next ctx
                    | Failure error ->
                        tr.Status <- SpanStatus.InvalidArgument;
                        return! unprocessableEntityResponse error next ctx
                finally
                    tr.Finish()
            })

    let createMerged: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let userId = getUserId ctx

            let tr =
                Metrics.startTransactionWithUser
                    $"{ctx.Request.Method} /api/debts/merged"
                    "Debts/Handlers.create"
                    userId

            task {
                try
                    let! request = ctx.BindJsonAsync<CreateDebtRequest>()

                    match Logic.validateCreate request with
                    | Success _ ->
                        let debt = Logic.createMergedRequestToEntity request userId

                        let connectionString = getConnectionString ctx
                        let! id = DebtsRepository.createMerged debt connectionString tr

                        return! Successful.CREATED id next ctx
                    | Failure error ->
                        tr.Status <- SpanStatus.InvalidArgument;
                        return! unprocessableEntityResponse error next ctx
                finally
                    tr.Finish()
            })

    let update: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let userId = getUserId ctx

            let tr =
                Metrics.startTransactionWithUser $"{ctx.Request.Method} /api/debts" "Debts/Handlers.update" userId

            task {
                try
                    let! request = ctx.BindJsonAsync<UpdateDebtRequest>()
                    request.HttpContext <- ctx

                    let connectionString = getConnectionString ctx
                    let! existingDebt = DebtsRepository.get request.Id connectionString

                    let validationParams =
                        {
                            CurrentUserId = userId
                            Request = request
                            ExistingDebt = existingDebt
                        }

                    match Logic.validateUpdate validationParams with
                    | Success _ ->
                        let debt = Logic.updateRequestToEntity request userId
                        let! _ = DebtsRepository.update debt connectionString tr

                        return! Successful.NO_CONTENT next ctx
                    | Failure error ->
                        tr.Status <- SpanStatus.InvalidArgument;
                        return! unprocessableEntityResponse error next ctx
                finally
                    tr.Finish()
            })

    let delete (id: int) : HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let userId = getUserId ctx

            let tr =
                Metrics.startTransactionWithUser $"{ctx.Request.Method} /api/debts/*" "Debts/Handlers.delete" userId

            task {
                try
                    let connectionString = getConnectionString ctx
                    let! _ = DebtsRepository.delete id userId connectionString tr

                    return! Successful.NO_CONTENT next ctx
                finally
                    tr.Finish()
            })
