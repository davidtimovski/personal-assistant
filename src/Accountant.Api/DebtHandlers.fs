module DebtHandlers

open Accountant.Application.Contracts.Debts
open Accountant.Application.Fs.Models.Debts
open Accountant.Application.Fs.Services
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open HandlerBase

let create: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let! dto = ctx.BindJsonAsync<CreateDebt>()
                let userId = getUserId ctx

                let debt = DebtService.prepareForCreate dto userId

                let repository = ctx.GetService<IDebtsRepository>()
                let! id = repository.CreateAsync(debt)

                return! Successful.CREATED id next ctx
            with ex ->
               let logger = ctx.GetService<ILogger<HttpContext>>()
               logger.LogError(ex, "Unexpected error in create")

               return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }

let createMerged: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let! dto = ctx.BindJsonAsync<CreateDebt>()
                let userId = getUserId ctx

                let debt = DebtService.prepareForCreateMerged dto userId

                let repository = ctx.GetService<IDebtsRepository>()
                let! id = repository.CreateMergedAsync(debt)

                return! Successful.CREATED id next ctx
            with ex ->
               let logger = ctx.GetService<ILogger<HttpContext>>()
               logger.LogError(ex, "Unexpected error in createMerged")

               return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }

let update: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let! dto = ctx.BindJsonAsync<UpdateDebt>()
                let userId = getUserId ctx

                let debt = DebtService.prepareForUpdate dto userId

                let repository = ctx.GetService<IDebtsRepository>()
                do! repository.UpdateAsync(debt)

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
                let repository = ctx.GetService<IDebtsRepository>()
                let userId = getUserId ctx

                do! repository.DeleteAsync(id, userId)
                return! Successful.NO_CONTENT next ctx
            with ex ->
                let logger = ctx.GetService<ILogger<HttpContext>>()
                logger.LogError(ex, "Unexpected error in delete")

                return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }
