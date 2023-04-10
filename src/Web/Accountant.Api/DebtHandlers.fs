module DebtHandlers

open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Application.Fs.Models.Debts
open Accountant.Application.Fs.Services
open Accountant.Persistence.Fs
open CommonHandlers
open HandlerBase

let create: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateDebt>()
            let userId = getUserId ctx

            let debt = DebtService.prepareForCreate dto userId

            let connection = getDbConnection ctx
            let! id = DebtsRepository.create debt connection

            return! Successful.CREATED id next ctx
        }
    )

let createMerged: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateDebt>()
            let userId = getUserId ctx

            let debt = DebtService.prepareForCreateMerged dto userId

            let connection = getDbConnection ctx
            let! id = DebtsRepository.createMerged debt connection

            return! Successful.CREATED id next ctx
        }
    )

let update: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<UpdateDebt>()
            let userId = getUserId ctx

            let debt = DebtService.prepareForUpdate dto userId

            let connection = getDbConnection ctx
            let! _ = DebtsRepository.update debt connection

            return! Successful.NO_CONTENT next ctx
        }
    )

let delete (id: int) : HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let userId = getUserId ctx

            let connection = getDbConnection ctx
            let! _ = DebtsRepository.delete id userId connection

            return! Successful.NO_CONTENT next ctx
        }
    )
