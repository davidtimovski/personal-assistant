module TransactionHandlers

open System
open System.IO
open Giraffe
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Accountant.Application.Contracts.Transactions
open Accountant.Application.Contracts.Transactions.Models
open Accountant.Application.Fs.Models.Transactions
open Accountant.Application.Fs.Services
open Accountant.Persistence.Fs
open CommonHandlers
open HandlerBase

let modifyInvalid (fromAccountId: int Option) (toAccountId: int Option) (userId: int) connection =
    (fromAccountId.IsNone && toAccountId.IsNone)
        || (fromAccountId = toAccountId)
        || fromAccountId.IsSome && not (AccountsRepository.exists fromAccountId.Value userId connection)
        || toAccountId.IsSome && not (AccountsRepository.exists toAccountId.Value userId connection)

let create: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateTransaction>()
            let userId = getUserId ctx

            let connection = getDbConnection ctx

            if (modifyInvalid dto.FromAccountId dto.ToAccountId userId connection) then
                return! (RequestErrors.UNPROCESSABLE_ENTITY "Accounts are not valid") next ctx
            else
                let transaction = TransactionService.prepareForCreate dto

                let! id = TransactionsRepository.create transaction connection None

                return! Successful.CREATED id next ctx
        }
    )

let update: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<UpdateTransaction>()
            let userId = getUserId ctx

            let connection = getDbConnection ctx

            if (modifyInvalid dto.FromAccountId dto.ToAccountId userId connection) then
                return! (RequestErrors.UNPROCESSABLE_ENTITY "Accounts are not valid") next ctx
            else
                let transaction = TransactionService.prepareForUpdate dto

                let! _ = TransactionsRepository.update transaction connection

                return! Successful.NO_CONTENT next ctx
        }
    )

let delete (id: int) : HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let userId = getUserId ctx

            let connection = getDbConnection ctx
            let! _ = TransactionsRepository.delete id userId connection

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
