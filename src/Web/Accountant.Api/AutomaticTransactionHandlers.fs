module AutomaticTransactionHandlers

open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Application.Fs.Models.AutomaticTransactions
open Accountant.Application.Fs.Services
open Accountant.Persistence.Fs
open Accountant.Persistence.Fs.CommonRepository
open CommonHandlers

let create: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateAutomaticTransaction>()
            let userId = getUserId ctx

            let automaticTransaction = AutomaticTransactionService.prepareForCreate dto userId

            let dbContext = ctx.GetService<AccountantContext>()
            let! id = AutomaticTransactionsRepository.create automaticTransaction dbContext

            return! Successful.CREATED id next ctx
        }
    )

let update: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<UpdateAutomaticTransaction>()
            let userId = getUserId ctx

            let automaticTransaction = AutomaticTransactionService.prepareForUpdate dto userId

            let dbContext = ctx.GetService<AccountantContext>()
            let! _ = AutomaticTransactionsRepository.update automaticTransaction dbContext

            return! Successful.NO_CONTENT next ctx
        }
    )

let delete (id: int) : HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let userId = getUserId ctx

            let dbContext = ctx.GetService<AccountantContext>()
            let! _ = AutomaticTransactionsRepository.delete id userId dbContext

            return! Successful.NO_CONTENT next ctx
        }
    )
