module DebtHandlers

open Accountant.Application.Contracts.Debts
open Accountant.Application.Fs.Models.Debts
open Accountant.Application.Fs.Services
open Giraffe
open Microsoft.AspNetCore.Http
open HandlerBase

let create: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateDebt>()
            let userId = getUserId ctx

            let debt = DebtService.prepareForCreate dto userId

            let repository = ctx.GetService<IDebtsRepository>()
            let! id = repository.CreateAsync(debt)

            return! Successful.CREATED id next ctx
        }
    )

let createMerged: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateDebt>()
            let userId = getUserId ctx

            let debt = DebtService.prepareForCreateMerged dto userId

            let repository = ctx.GetService<IDebtsRepository>()
            let! id = repository.CreateMergedAsync(debt)

            return! Successful.CREATED id next ctx
        }
    )

let update: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<UpdateDebt>()
            let userId = getUserId ctx

            let debt = DebtService.prepareForUpdate dto userId

            let repository = ctx.GetService<IDebtsRepository>()
            do! repository.UpdateAsync(debt)

            return! Successful.NO_CONTENT next ctx
        }
    )

let delete (id: int) : HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let repository = ctx.GetService<IDebtsRepository>()
            let userId = getUserId ctx

            do! repository.DeleteAsync(id, userId)
            return! Successful.NO_CONTENT next ctx
        }
    )
