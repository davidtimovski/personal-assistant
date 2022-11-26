﻿module DebtHandlers

open Accountant.Application.Contracts.Debts
open Accountant.Application.Contracts.Debts.Models
open Giraffe
open Microsoft.AspNetCore.Http
open HandlerBase

let create: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateDebt>()

            if dto = null then
                return! (RequestErrors.BAD_REQUEST "Bad request") next ctx
            else
                let service = ctx.GetService<IDebtService>()
                dto.UserId <- getUserId ctx

                let! id = service.CreateAsync(dto)

                return! (Successful.CREATED id) next ctx
        }

let createMerged: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateDebt>()

            if dto = null then
                return! (RequestErrors.BAD_REQUEST "Bad request") next ctx
            else
                let service = ctx.GetService<IDebtService>()
                dto.UserId <- getUserId ctx

                let! id = service.CreateMergedAsync(dto)

                return! (Successful.CREATED id) next ctx
        }

let update: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<UpdateDebt>()

            if dto = null then
                return! (RequestErrors.BAD_REQUEST "Bad request") next ctx
            else
                let service = ctx.GetService<IDebtService>()
                dto.UserId <- getUserId ctx

                do! service.UpdateAsync(dto)

                return! Successful.NO_CONTENT next ctx
        }

let delete (id: int) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let service = ctx.GetService<IDebtService>()
            let userId = getUserId ctx

            do! service.DeleteAsync(id, userId)

            return! Successful.NO_CONTENT next ctx
        }