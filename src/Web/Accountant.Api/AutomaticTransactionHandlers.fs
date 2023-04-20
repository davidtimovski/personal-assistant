module AutomaticTransactionHandlers

open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Application.Fs.Models.AutomaticTransactions
open Accountant.Application.Fs.Services
open Accountant.Persistence.Fs
open CommonHandlers
open HandlerBase

let create: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateAutomaticTransaction>()
            let userId = getUserId ctx

            let automaticTransaction = AutomaticTransactionService.prepareForCreate dto userId

            let connection = getDbConnection ctx
            let! id = AutomaticTransactionsRepository.create automaticTransaction connection

            return! Successful.CREATED id next ctx
        }
    )

let update: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<UpdateAutomaticTransaction>()
            let userId = getUserId ctx

            let automaticTransaction = AutomaticTransactionService.prepareForUpdate dto userId

            let connection = getDbConnection ctx
            let! _ = AutomaticTransactionsRepository.update automaticTransaction connection

            return! Successful.NO_CONTENT next ctx
        }
    )

let delete (id: int) : HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let userId = getUserId ctx

            let connection = getDbConnection ctx
            let! _ = AutomaticTransactionsRepository.delete id userId connection

            return! Successful.NO_CONTENT next ctx
        }
    )
