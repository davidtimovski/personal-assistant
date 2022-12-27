module AutomaticTransactionHandlers

open Accountant.Application.Contracts.AutomaticTransactions
open Accountant.Application.Fs.Models.AutomaticTransactions
open Accountant.Application.Fs.Services
open Giraffe
open Microsoft.AspNetCore.Http
open CommonHandlers

let create: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateAutomaticTransaction>()
            let userId = getUserId ctx

            let automaticTransaction = AutomaticTransactionService.prepareForCreate dto userId

            let repository = ctx.GetService<IAutomaticTransactionsRepository>()
            let! id = repository.CreateAsync(automaticTransaction)

            return! Successful.CREATED id next ctx
        }
    )

let update: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<UpdateAutomaticTransaction>()
            let userId = getUserId ctx

            let automaticTransaction = AutomaticTransactionService.prepareForUpdate dto userId

            let repository = ctx.GetService<IAutomaticTransactionsRepository>()
            do! repository.UpdateAsync(automaticTransaction)

            return! Successful.NO_CONTENT next ctx
        }
    )

let delete (id: int) : HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let repository = ctx.GetService<IAutomaticTransactionsRepository>()
            let userId = getUserId ctx

            do! repository.DeleteAsync(id, userId)
            return! Successful.NO_CONTENT next ctx
        }
    )
