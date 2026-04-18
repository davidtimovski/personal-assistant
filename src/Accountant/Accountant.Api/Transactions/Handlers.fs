namespace Accountant.Api.Transactions

open System
open System.IO
open Giraffe
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Core.Application.Contracts
open Accountant.Persistence
open Accountant.Api
open Api.Common.Fs
open CommonHandlers
open HandlerBase
open Models
open Sentry

module Handlers =

    let create: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let userId = getUserId ctx

            let tr =
                Metrics.startTransactionWithUser
                    $"{ctx.Request.Method} /api/transactions"
                    "Transactions/Handlers.create"
                    userId

            task {
                try
                    let! request = ctx.BindJsonAsync<CreateTransactionRequest>()
                    request.HttpContext <- ctx

                    match Logic.validateCreate request with
                    | Success _ ->
                        let transaction = Logic.prepareForCreate request
                        let connection = getDbConnection ctx

                        let! id = TransactionsRepository.create transaction connection None tr

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
                    $"{ctx.Request.Method} /api/transactions"
                    "Transactions/Handlers.update"
                    userId

            task {
                try
                    let! request = ctx.BindJsonAsync<UpdateTransactionRequest>()
                    request.HttpContext <- ctx

                    match Logic.validateUpdate request with
                    | Success _ ->
                        let transaction = Logic.prepareForUpdate request
                        let connectionString = getConnectionString ctx

                        let! _ = TransactionsRepository.update transaction connectionString tr

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
                    $"{ctx.Request.Method} /api/transactions/*"
                    "Transactions/Handlers.delete"
                    userId

            task {
                try
                    let connectionString = getConnectionString ctx
                    let! _ = TransactionsRepository.delete id userId connectionString tr

                    return! Successful.NO_CONTENT next ctx
                finally
                    tr.Finish()
            })

    let export: HttpHandler =
        successOrLog (fun (_) (ctx: HttpContext) ->
            let userId = getUserId ctx

            let tr =
                Metrics.startTransactionWithUser
                    $"{ctx.Request.Method} /api/transactions/export"
                    "Transactions/Handlers.export"
                    userId

            task {
                try
                    let! dto = ctx.BindJsonAsync<ExportDto>()

                    let service = ctx.GetService<ICsvService>()
                    let webHostEnvironment = ctx.GetService<IWebHostEnvironment>()

                    let directory = Path.Combine(webHostEnvironment.ContentRootPath, "storage", "temp")

                    let uncategorized = localize ctx "Uncategorized"
                    let encrypted = localize ctx "Encrypted"

                    let exportAsCsvModel =
                        new ExportTransactionsAsCsv(userId, directory, dto.FileId, uncategorized, encrypted)

                    let file = service.ExportTransactionsAsCsv(exportAsCsvModel, tr)

                    ctx.SetHttpHeader("Content-Disposition", "attachment; filename=\"transactions.csv\"")

                    return! ctx.WriteStreamAsync(true, file, None, None)
                finally
                    tr.Finish()
            })

    let deleteExportedFile (fileId: Guid) : HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let userId = getUserId ctx

            let tr =
                Metrics.startTransactionWithUser
                    $"{ctx.Request.Method} /api/transactions/exported-file/*"
                    "Transactions/Handlers.deleteExportedFile"
                    userId

            task {
                try
                    let webHostEnvironment = ctx.GetService<IWebHostEnvironment>()

                    let filePath =
                        Path.Combine(webHostEnvironment.ContentRootPath, "storage", "temp", $"{fileId}.csv")

                    File.Delete(filePath)

                    return! Successful.NO_CONTENT next ctx
                finally
                    tr.Finish()
            })
