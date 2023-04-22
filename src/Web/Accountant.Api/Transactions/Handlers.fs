namespace Accountant.Api.Transactions

open System
open System.IO
open Giraffe
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Accountant.Application.Contracts.Transactions
open Accountant.Application.Contracts.Transactions.Models
open Accountant.Persistence.Fs
open Accountant.Api
open CommonHandlers
open HandlerBase
open Models

module Handlers =

    let create: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! dto = ctx.BindJsonAsync<CreateTransaction>()
                dto.HttpContext <- ctx

                match Logic.validateCreate dto with
                | Success _ ->
                    let transaction = Logic.prepareForCreate dto
                    let connection = getDbConnection ctx

                    let! id = TransactionsRepository.create transaction connection None

                    return! Successful.CREATED id next ctx
                | Failure error -> return! RequestErrors.BAD_REQUEST error next ctx
            })

    let update: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! dto = ctx.BindJsonAsync<UpdateTransaction>()
                dto.HttpContext <- ctx

                match Logic.validateUpdate dto with
                | Success _ ->
                    let transaction = Logic.prepareForUpdate dto
                    let connection = getDbConnection ctx

                    let! _ = TransactionsRepository.update transaction connection

                    return! Successful.NO_CONTENT next ctx
                | Failure error -> return! RequestErrors.BAD_REQUEST error next ctx
            })

    let delete (id: int) : HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let userId = getUserId ctx

                let connection = getDbConnection ctx
                let! _ = TransactionsRepository.delete id userId connection

                return! Successful.NO_CONTENT next ctx
            })

    let export: HttpHandler =
        successOrLog (fun (_) (ctx: HttpContext) ->
            task {
                let! dto = ctx.BindJsonAsync<ExportDto>()

                let service = ctx.GetService<ITransactionService>()
                let webHostEnvironment = ctx.GetService<IWebHostEnvironment>()
                let userId = getUserId ctx

                let directory = Path.Combine(webHostEnvironment.ContentRootPath, "storage", "temp")

                let uncategorized = localize ctx "Uncategorized"
                let encrypted = localize ctx "Encrypted"

                let exportAsCsvModel =
                    new ExportAsCsv(userId, directory, dto.FileId, uncategorized, encrypted)

                let file = service.ExportAsCsv(exportAsCsvModel)

                ctx.SetHttpHeader("Content-Disposition", "attachment; filename=\"transactions.csv\"")

                return! ctx.WriteStreamAsync(true, file, None, None)
            })

    let deleteExportedFile (fileId: Guid) : HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let webHostEnvironment = ctx.GetService<IWebHostEnvironment>()

                let filePath =
                    Path.Combine(webHostEnvironment.ContentRootPath, "storage", "temp", $"{fileId}.csv")

                File.Delete(filePath)

                return! Successful.NO_CONTENT next ctx
            })
