module CategoryHandlers

open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Application.Fs.Models.Categories
open Accountant.Application.Fs.Services
open Accountant.Persistence.Fs
open Accountant.Persistence.Fs.CommonRepository
open CommonHandlers

let create: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<CreateCategory>()
            let userId = getUserId ctx

            let category = CategoryService.prepareForCreate dto userId

            let dbContext = ctx.GetService<AccountantContext>()
            let! id = CategoriesRepository.create category dbContext

            return! Successful.CREATED id next ctx
        }
    )

let update: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! dto = ctx.BindJsonAsync<UpdateCategory>()
            let userId = getUserId ctx

            let category = CategoryService.prepareForUpdate dto userId

            let dbContext = ctx.GetService<AccountantContext>()
            let! _ = CategoriesRepository.update category dbContext

            return! Successful.NO_CONTENT next ctx
        }
    )

let delete (id: int) : HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let userId = getUserId ctx

            let dbContext = ctx.GetService<AccountantContext>()
            let! _ = CategoriesRepository.delete id userId dbContext

            return! Successful.NO_CONTENT next ctx
        }
    )
