module CategoryHandlers

open Accountant.Application.Contracts.Categories
open Accountant.Application.Fs.Models.Categories
open Accountant.Application.Fs.Services
open Giraffe
open Microsoft.AspNetCore.Http
open CommonHandlers
open HandlerBase

let create: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateCategory>()
            let userId = getUserId ctx

            let category = CategoryService.prepareForCreate dto userId

            let repository = ctx.GetService<ICategoriesRepository>()
            let! id = repository.CreateAsync(category)

            return! Successful.CREATED id next ctx
        }
    )

let update: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<UpdateCategory>()
            let userId = getUserId ctx

            let category = CategoryService.prepareForUpdate dto userId

            let repository = ctx.GetService<ICategoriesRepository>()
            do! repository.UpdateAsync(category)

            return! Successful.NO_CONTENT next ctx
        }
    )

let delete (id: int) : HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let repository = ctx.GetService<ICategoriesRepository>()
            let userId = getUserId ctx

            do! repository.DeleteAsync(id, userId)
            return! Successful.NO_CONTENT next ctx
        }
    )
