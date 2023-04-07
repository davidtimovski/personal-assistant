module TransactionHandlers

open System
open System.IO
open Accountant.Application.Contracts.Transactions
open Accountant.Application.Contracts.Transactions.Models
open Accountant.Application.Fs.Services
open Giraffe
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open CommonHandlers
open HandlerBase
open Models

let create: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateTransaction>()
            dto.UserId <- getUserId ctx

            let connectionString = getConnectionString ctx

            if dto = null || (TransactionService.modifyIsInvalid dto.FromAccountId dto.ToAccountId dto.UserId connectionString) then
                return! (RequestErrors.BAD_REQUEST "Bad request") next ctx
            else
                let service = ctx.GetService<ITransactionService>()

                let! id = service.CreateAsync(dto)

                return! Successful.CREATED id next ctx
        }
    )

let update: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<UpdateTransaction>()
            dto.UserId <- getUserId ctx

            let connectionString = getConnectionString ctx

            if dto = null || (TransactionService.modifyIsInvalid dto.FromAccountId dto.ToAccountId dto.UserId connectionString) then
                return! (RequestErrors.BAD_REQUEST "Bad request") next ctx
            else
                let service = ctx.GetService<ITransactionService>()
                dto.UserId <- getUserId ctx

                do! service.UpdateAsync(dto)

                return! Successful.NO_CONTENT next ctx
        }
    )

let delete (id: int) : HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let repository = ctx.GetService<ITransactionsRepository>()
            let userId = getUserId ctx

            do! repository.DeleteAsync(id, userId)

            return! Successful.NO_CONTENT next ctx
        }
    )

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
        }
    )

let deleteExportedFile (fileId: Guid) : HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let webHostEnvironment = ctx.GetService<IWebHostEnvironment>()

            let filePath = Path.Combine(webHostEnvironment.ContentRootPath, "storage", "temp", $"{fileId}.csv")
            File.Delete(filePath);

            return! Successful.NO_CONTENT next ctx
        }
    )
