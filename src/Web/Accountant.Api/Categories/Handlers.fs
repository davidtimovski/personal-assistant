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
            task {
                let! dto = ctx.BindJsonAsync<CreateCategory>()
                let userId = getUserId ctx
                let connection = getDbConnection ctx

                match Validation.categoryBelongsTo dto.ParentId userId connection with
                | true ->
                    let category = Logic.prepareForCreate dto userId

                    let! id = CategoriesRepository.create category connection

                    return! Successful.CREATED id next ctx
                | false -> return! (RequestErrors.BAD_REQUEST "Currency is not valid") next ctx
            })

    let update: HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! dto = ctx.BindJsonAsync<UpdateCategory>()
                let userId = getUserId ctx
                let connection = getDbConnection ctx

                match
                    (CategoriesRepository.exists dto.Id userId connection)
                    && (Validation.categoryBelongsTo dto.ParentId userId connection)
                with
                | true ->
                    let category = Logic.prepareForUpdate dto userId

                    let! _ = CategoriesRepository.update category connection

                    return! Successful.NO_CONTENT next ctx
                | false -> return! (RequestErrors.BAD_REQUEST "Currency is not valid") next ctx
            })

    let delete (id: int) : HttpHandler =
        successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let userId = getUserId ctx
                let connection = getDbConnection ctx

                let! _ = CategoriesRepository.delete id userId connection

                return! Successful.NO_CONTENT next ctx
            })
