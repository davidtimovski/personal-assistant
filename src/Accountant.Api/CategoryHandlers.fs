module CategoryHandlers

open Accountant.Application.Contracts.Categories
open Accountant.Application.Fs.Models.Categories
open Accountant.Application.Fs.Services
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open HandlerBase

let create: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let! dto = ctx.BindJsonAsync<CreateCategory>()
                let userId = getUserId ctx

                let category = CategoryService.prepareForCreate dto userId

                let repository = ctx.GetService<ICategoriesRepository>()
                let! id = repository.CreateAsync(category)

                return! Successful.CREATED id next ctx
            with ex ->
               let logger = ctx.GetService<ILogger>()
               logger.LogError(ex, "Unexpected error in create")

               return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }

let update: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let! dto = ctx.BindJsonAsync<UpdateCategory>()
                let userId = getUserId ctx

                let category = CategoryService.prepareForUpdate dto userId

                let repository = ctx.GetService<ICategoriesRepository>()
                do! repository.UpdateAsync(category)

                return! Successful.NO_CONTENT next ctx
            with ex ->
               let logger = ctx.GetService<ILogger>()
               logger.LogError(ex, "Unexpected error in update")

               return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }

let delete (id: int) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            try
                let repository = ctx.GetService<ICategoriesRepository>()
                let userId = getUserId ctx

                do! repository.DeleteAsync(id, userId)
                return! Successful.NO_CONTENT next ctx
            with ex ->
                let logger = ctx.GetService<ILogger>()
                logger.LogError(ex, "Unexpected error in delete")

                return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
        }
