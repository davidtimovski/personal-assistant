module ExerciseHandlers

open System
open Giraffe
open Core.Application.Contracts
open Microsoft.AspNetCore.Http
open Models
open CommonHandlers

let exercises: Object array = [|
    {
        id = 1
        name = "Running"
        sets = 2
        amountUnit = "meters"
        ofType = ExerciseType.Amount
    }
    {
        id = 2
        name = "Lifting weights"
        sets = 1
        amount1Unit = "meters"
        amount2Unit = "haha"
        ofType = ExerciseType.AmountX2
    }
|]

let getAll: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            return! Successful.OK exercises next ctx
        }
    )

let get (id: int) : HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            if id = 1 then
                return! Successful.OK exercises[0] next ctx
            else
                return! Successful.OK exercises[1] next ctx
        }
    )

let createAmount: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        let dto = ctx.BindModelAsync<CreateAmountExercise>()

        task {
            // validate exerciseId belongs to user

            // insert in progress_amount

            return! Successful.CREATED 1 next ctx
        }
    )

let updateAmount: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        let dto = ctx.BindModelAsync<EditAmountExercise>()

        task {
            // validate exerciseId belongs to user

            // insert in progress_amount

            return! Successful.NO_CONTENT next ctx
        }
    )

let createAmountX2: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        let dto = ctx.BindModelAsync<CreateAmountX2Exercise>()

        task {
            // validate exerciseId belongs to user

            // insert in progress_amount

            return! Successful.CREATED 1 next ctx
        }
    )

let updateAmountX2: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        let dto = ctx.BindModelAsync<EditAmountX2Exercise>()

        task {
            // validate exerciseId belongs to user

            // insert in progress_amount

            return! Successful.NO_CONTENT next ctx
        }
    )
