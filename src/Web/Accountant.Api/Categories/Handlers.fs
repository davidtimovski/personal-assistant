namespace Accountant.Api.Categories

open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Persistence.Fs
open Accountant.Api
open CommonHandlers
open HandlerBase
open Models

module Handlers =

    let create: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let tr =
                startTransactionWithUser "POST /api/categories" "Categories/Handlers.create" ctx

            task {
                let! dto = ctx.BindJsonAsync<CreateCategory>()
                dto.HttpContext <- ctx

                match Logic.validateCreate dto with
                | Success _ ->
                    let userId = getUserId ctx
                    let category = Logic.prepareForCreate dto userId

                    let connection = getDbConnection ctx
                    let! id = CategoriesRepository.create category connection tr

                    let! result = Successful.CREATED id next ctx

                    tr.Finish()

                    return result
                | Failure error -> return! RequestErrors.BAD_REQUEST error next ctx
            })

    let update: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let tr =
                startTransactionWithUser "PUT /api/categories" "Categories/Handlers.update" ctx

            task {
                let! dto = ctx.BindJsonAsync<UpdateCategory>()
                dto.HttpContext <- ctx

                match Logic.validateUpdate dto with
                | Success _ ->
                    let userId = getUserId ctx
                    let category = Logic.prepareForUpdate dto userId

                    let connectionString = getConnectionString ctx
                    let! _ = CategoriesRepository.update category connectionString tr

                    let! result = Successful.NO_CONTENT next ctx

                    tr.Finish()

                    return result
                | Failure error -> return! RequestErrors.BAD_REQUEST error next ctx
            })

    let delete (id: int) : HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            let tr =
                startTransactionWithUser "DELETE /api/categories" "Categories/Handlers.delete" ctx

            task {
                let userId = getUserId ctx
                let connectionString = getConnectionString ctx

                let! _ = CategoriesRepository.delete id userId connectionString tr

                let! result = Successful.NO_CONTENT next ctx

                tr.Finish()

                return result
            })
