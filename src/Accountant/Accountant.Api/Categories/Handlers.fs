namespace Accountant.Api.Categories

open Giraffe
open Microsoft.AspNetCore.Http
open Accountant.Persistence
open Accountant.Api
open Api.Common.Fs
open CommonHandlers
open HandlerBase
open Models
open Logic
open Sentry

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
                try
                    let! request = ctx.BindJsonAsync<CreateCategoryRequest>()
                    request.HttpContext <- ctx

                    match Logic.validateCreate request with
                    | Success _ ->
                        let category = Logic.createRequestToEntity request userId

                        let connection = getDbConnection ctx
                        let! id = CategoriesRepository.create category connection tr

                        let! result = Successful.CREATED id next ctx

                        return result
                    | Failure error ->
                        tr.Status <- SpanStatus.InvalidArgument;
                        return! RequestErrors.BAD_REQUEST error next ctx
                finally
                    tr.Finish()
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
                try
                    let! request = ctx.BindJsonAsync<UpdateCategoryRequest>()
                    request.HttpContext <- ctx

                    let connectionString = getConnectionString ctx
                    let! existingCategory = CategoriesRepository.get request.Id connectionString
                    let! existingParentCategory = 
                        task {
                            match request.ParentId with
                            | Some parentId -> return! CategoriesRepository.get parentId connectionString
                            | None -> return None
                        }

                    let validationParams =
                        {
                            CurrentUserId = userId
                            Request = request
                            ExistingCategory = existingCategory
                            ExistingParentCategory = existingParentCategory
                        }

                    match Logic.validateUpdate validationParams with
                    | Success _ ->
                        let category = Logic.updateRequestToEntity request userId
                        let! _ = CategoriesRepository.update category connectionString tr

                        let! result = Successful.NO_CONTENT next ctx

                        return result
                    | Failure error ->
                        tr.Status <- SpanStatus.InvalidArgument;
                        return! RequestErrors.BAD_REQUEST error next ctx
                finally
                    tr.Finish()
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
                try
                    let connectionString = getConnectionString ctx

                    let! _ = CategoriesRepository.delete id userId connectionString tr

                    let! result = Successful.NO_CONTENT next ctx

                    return result
                finally
                    tr.Finish()
            })
