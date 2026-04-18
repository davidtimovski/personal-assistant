namespace Accountant.Api.Accounts

open System
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
                Metrics.startTransactionWithUser $"{ctx.Request.Method} /api/accounts" "Accounts/Handlers.create" userId

            task {
                try
                    let! request = ctx.BindJsonAsync<CreateAccountRequest>()

                    match Logic.validateCreate request with
                    | Success _ ->
                        let account = Logic.createRequestToEntity request userId

                        let connection = getDbConnection ctx
                        let! id = AccountsRepository.create account connection tr

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
                Metrics.startTransactionWithUser $"{ctx.Request.Method} /api/accounts" "Accounts/Handlers.update" userId

            task {
                try
                    let! request = ctx.BindJsonAsync<UpdateAccountRequest>()
                    request.HttpContext <- ctx

                    let connectionString = getConnectionString ctx
                    let! existingAccount = AccountsRepository.get request.Id connectionString

                    let validationParams =
                        {
                            CurrentUserId = userId
                            Request = request
                            ExistingAccount = existingAccount
                        }

                    match Logic.validateUpdate validationParams with
                    | Success _ ->
                        let account = Logic.updateRequestToEntity request userId
                        let! _ = AccountsRepository.update account connectionString tr

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
                    $"{ctx.Request.Method} /api/accounts/*"
                    "Accounts/Handlers.delete"
                    userId

            task {
                try
                    let connectionString = getConnectionString ctx

                    let isMain = AccountsRepository.isMain id userId connectionString

                    if isMain then
                        tr.Status <- SpanStatus.InvalidArgument;
                        let error = { Field = String.Empty; ErrorMessage = "Cannot delete main account" }
                        return! unprocessableEntityResponse error next ctx
                    else
                        let! _ = AccountsRepository.delete id userId connectionString tr

                        return! Successful.NO_CONTENT next ctx
                finally
                    tr.Finish()
            })
