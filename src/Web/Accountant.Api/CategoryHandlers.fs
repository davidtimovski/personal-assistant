module CategoryHandlers

open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Application.Fs.Models.Categories
open Accountant.Application.Fs.Services
open Accountant.Persistence.Fs
open CommonHandlers
open HandlerBase

let create: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateCategory>()
            let userId = getUserId ctx

            let category = CategoryService.prepareForCreate dto userId

            let connection = getDbConnection ctx
            let! id = CategoriesRepository.create category connection

            return! Successful.CREATED id next ctx
        }
    )

let update: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<UpdateCategory>()
            let userId = getUserId ctx

            let category = CategoryService.prepareForUpdate dto userId

            let connection = getDbConnection ctx
            let! _ = CategoriesRepository.update category connection

            return! Successful.NO_CONTENT next ctx
        }
    )

let delete (id: int) : HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let userId = getUserId ctx

            let connection = getDbConnection ctx
            let! _ = CategoriesRepository.delete id userId connection

            return! Successful.NO_CONTENT next ctx
        }
    )
