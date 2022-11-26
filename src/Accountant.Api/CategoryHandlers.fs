module CategoryHandlers

open Accountant.Application.Contracts.Categories
open Accountant.Application.Contracts.Categories.Models
open Giraffe
open Microsoft.AspNetCore.Http
open HandlerBase

let create: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateCategory>()

            if dto = null then
                return! (RequestErrors.BAD_REQUEST "Bad request") next ctx
            else
                let service = ctx.GetService<ICategoryService>()
                dto.UserId <- getUserId ctx

                let! id = service.CreateAsync(dto)

                return! (Successful.CREATED id) next ctx
        }

let update: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<UpdateCategory>()

            if dto = null then
                return! (RequestErrors.BAD_REQUEST "Bad request") next ctx
            else
                let service = ctx.GetService<ICategoryService>()
                dto.UserId <- getUserId ctx

                do! service.UpdateAsync(dto)

                return! Successful.NO_CONTENT next ctx
        }

let delete (id: int) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let service = ctx.GetService<ICategoryService>()
            let userId = getUserId ctx

            do! service.DeleteAsync(id, userId)

            return! Successful.NO_CONTENT next ctx
        }
