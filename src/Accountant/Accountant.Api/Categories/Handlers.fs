namespace Accountant.Api.Categories

open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Persistence
open Accountant.Api
open Api.Common.Fs
open CommonHandlers
open HandlerBase
open Models

module Handlers =

    let create: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let userId = getUserId ctx

            let tr =
                Metrics.startTransactionWithUser
                    $"{ctx.Request.Method} /api/categories"
                    "Categories/Handlers.create"
                    userId

            task {
                let! request = ctx.BindJsonAsync<CreateCategoryRequest>()
                request.HttpContext <- ctx

                match Logic.validateCreate request with
                | Success _ ->
                    let category = Logic.prepareForCreate request userId

                    let connection = getDbConnection ctx
                    let! id = CategoriesRepository.create category connection tr

                    let! result = Successful.CREATED id next ctx

                    tr.Finish()

                    return result
                | Failure error ->
                    tr.Finish()
                    return! RequestErrors.BAD_REQUEST error next ctx
            })

    let update: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let userId = getUserId ctx

            let tr =
                Metrics.startTransactionWithUser
                    $"{ctx.Request.Method} /api/categories"
                    "Categories/Handlers.update"
                    userId

            task {
                let! request = ctx.BindJsonAsync<UpdateCategoryRequest>()
                request.HttpContext <- ctx

                match Logic.validateUpdate request with
                | Success _ ->
                    let category = Logic.prepareForUpdate request userId

                    let connectionString = getConnectionString ctx
                    let! _ = CategoriesRepository.update category connectionString tr

                    let! result = Successful.NO_CONTENT next ctx

                    tr.Finish()

                    return result
                | Failure error ->
                    tr.Finish()
                    return! RequestErrors.BAD_REQUEST error next ctx
            })

    let delete (id: int) : HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let userId = getUserId ctx

            let tr =
                Metrics.startTransactionWithUser
                    $"{ctx.Request.Method} /api/categories/*"
                    "Categories/Handlers.delete"
                    userId

            task {
                let connectionString = getConnectionString ctx

                let! _ = CategoriesRepository.delete id userId connectionString tr

                let! result = Successful.NO_CONTENT next ctx

                tr.Finish()

                return result
            })
