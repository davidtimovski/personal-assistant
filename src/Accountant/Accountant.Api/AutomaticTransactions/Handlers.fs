namespace Accountant.Api.AutomaticTransactions

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
                Metrics.startTransactionWithUser
                    $"{ctx.Request.Method} /api/automatic-transactions"
                    "AutomaticTransactions/Handlers.create"
                    userId

            task {
                try
                    let! request = ctx.BindJsonAsync<CreateAutomaticTransactionRequest>()
                    request.HttpContext <- ctx

                    match Logic.validateCreate request with
                    | Success _ ->
                        let automaticTransaction = Logic.createRequestToEntity request userId

                        let connection = getDbConnection ctx
                        let! id = AutomaticTransactionsRepository.create automaticTransaction connection tr

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
                Metrics.startTransactionWithUser
                    $"{ctx.Request.Method} /api/automatic-transactions"
                    "AutomaticTransactions/Handlers.update"
                    userId

            task {
                try
                    let! request = ctx.BindJsonAsync<UpdateAutomaticTransactionRequest>()
                    request.HttpContext <- ctx

                    let connectionString = getConnectionString ctx
                    let! existingAutomaticTransaction = AutomaticTransactionsRepository.get request.Id connectionString
                    let! existingCategory = 
                        task {
                            match request.CategoryId with
                            | Some categoryId -> return! CategoriesRepository.get categoryId connectionString
                            | None -> return None
                        }

                    let validationParams =
                        {
                            CurrentUserId = userId
                            Request = request
                            ExistingAutomaticTransaction = existingAutomaticTransaction
                            ExistingCategory = existingCategory
                        }

                    match Logic.validateUpdate validationParams with
                    | Success _ ->
                        let automaticTransaction = Logic.updateRequestToEntity request userId
                        let! _ = AutomaticTransactionsRepository.update automaticTransaction connectionString tr

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
                Metrics.startTransactionWithUser
                    $"{ctx.Request.Method} /api/automatic-transactions/*"
                    "AutomaticTransactions/Handlers.delete"
                    userId

            task {
                try
                    let connectionString = getConnectionString ctx

                    let! _ = AutomaticTransactionsRepository.delete id userId connectionString tr

                    return! Successful.NO_CONTENT next ctx
                finally
                    tr.Finish()
            })
