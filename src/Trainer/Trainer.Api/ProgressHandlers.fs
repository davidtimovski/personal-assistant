module ProgressHandlers

open System
open Giraffe
open Core.Application.Contracts
open Microsoft.AspNetCore.Http
open Models
open CommonHandlers

let progresses: EditProgressAmount array = [|
    {
        exerciseId = 1
        date = DateOnly.FromDateTime(DateTime.Now.Date)
        sets = [| 
            {
                set = 1
                amount = 5
            }
            {
                set = 2
                amount = 0
            }
        |]
    }
    {
        exerciseId = 1
        date = DateOnly.FromDateTime(DateTime.Now.AddDays(-1))
        sets = [| 
            {
                set = 1
                amount = 10
            }
            {
                set = 2
                amount = 15
            }
        |]
    }
|]

let get (exerciseId: int, date: string) : HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let dateOnly = DateOnly.FromDateTime(DateTime.Parse(date));
            let progressOption = progresses |> Seq.tryFind (fun p -> p.exerciseId = exerciseId && p.date = dateOnly)

            return! (
                match progressOption with
                | Some progress -> (Successful.OK progress next ctx)
                | None -> 
                    let newProgress: EditProgressAmount = {
                        exerciseId = 1
                        date = dateOnly
                        sets = [| 
                            {
                                set = 1
                                amount = 0
                            }
                            {
                                set = 2
                                amount = 0
                            }
                        |]
                    }

                    (Successful.OK newProgress next ctx)
            )
        }
    )

let createAmount: HttpHandler =
    successOrLog (fun (next: HttpFunc) (ctx: HttpContext) ->
        let dto = ctx.BindModelAsync<CreateProgressAmount>()

        task {
            // validate exerciseId belongs to user

            // insert in progress_amount

            return! Successful.CREATED 1 next ctx
        }
    )
