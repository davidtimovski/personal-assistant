module ProgressHandlers

open Giraffe
open Application.Contracts
open Microsoft.AspNetCore.Http
open Models
open HandlerBase

let createAmount: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        let dto = ctx.BindModelAsync<CreateProgressEntry>()

        task {
            // validate exerciseId belongs to user

            // insert in progress_amount

            return! Successful.CREATED 1 next ctx
        }
    )
