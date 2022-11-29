module TransactionHandlers

open System
open System.IO
open Accountant.Application.Contracts.Transactions
open Accountant.Application.Contracts.Transactions.Models
open Giraffe
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open HandlerBase
open Models

let create: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let! dto = ctx.BindJsonAsync<CreateTransaction>()

                if dto = null then
                    return! (RequestErrors.BAD_REQUEST "Bad request") next ctx
                else
                    let service = ctx.GetService<ITransactionService>()
                    dto.UserId <- getUserId ctx

                    let! id = service.CreateAsync(dto)

                    return! Successful.CREATED id next ctx
            with ex ->
                let logger = ctx.GetService<ILogger<HttpContext>>()
                logger.LogError(ex, "Unexpected error in create")

                return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }

let update: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let! dto = ctx.BindJsonAsync<UpdateTransaction>()

                if dto = null then
                    return! (RequestErrors.BAD_REQUEST "Bad request") next ctx
                else
                    let service = ctx.GetService<ITransactionService>()
                    dto.UserId <- getUserId ctx

                    do! service.UpdateAsync(dto)

                    return! Successful.NO_CONTENT next ctx
            with ex ->
                let logger = ctx.GetService<ILogger<HttpContext>>()
                logger.LogError(ex, "Unexpected error in update")

                return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }

let delete (id: int) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let service = ctx.GetService<ITransactionService>()
                let userId = getUserId ctx

                do! service.DeleteAsync(id, userId)

                return! Successful.NO_CONTENT next ctx
            with ex ->
                let logger = ctx.GetService<ILogger<HttpContext>>()
                logger.LogError(ex, "Unexpected error in delete")

                return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }

let export: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
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
            with ex ->
                let logger = ctx.GetService<ILogger<HttpContext>>()
                logger.LogError(ex, "Unexpected error in export")

                return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }


let deleteExportedFile (fileId: Guid) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let service = ctx.GetService<ITransactionService>()
                let webHostEnvironment = ctx.GetService<IWebHostEnvironment>()

                let directory = Path.Combine(webHostEnvironment.ContentRootPath, "storage", "temp")
                let deleteExportedFileModel = new DeleteExportedFile(directory, fileId)

                service.DeleteExportedFile(deleteExportedFileModel)

                return! Successful.NO_CONTENT next ctx
            with ex ->
                let logger = ctx.GetService<ILogger<HttpContext>>()
                logger.LogError(ex, "Unexpected error in deleteExportedFile")

                return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }
