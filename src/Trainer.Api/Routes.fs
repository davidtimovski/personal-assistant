module Routes

open Giraffe
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Http

let authorize: HttpHandler =
    requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

let webApp: HttpHandler =
    subRoute
        "/api"
        (choose
            [ GET
              >=> authorize
              >=> choose
                      [ route "/exercises" >=> ExerciseHandlers.getAll
                        routef "/exercises/%i" ExerciseHandlers.get

                        routef "/progress/%i/%s" ProgressHandlers.get ]
              POST
              >=> authorize
              >=> choose
                      [ route "/exercises/amount" >=> ExerciseHandlers.createAmount
                        route "/exercises/amountx2" >=> ExerciseHandlers.createAmountX2

                        route "/progress/amount" >=> ProgressHandlers.createAmount ]
              PUT
              >=> authorize
              >=> choose
                      [ route "/exercises/amount" >=> ExerciseHandlers.updateAmount
                        route "/exercises/amountx2" >=> ExerciseHandlers.updateAmountX2

                        route "/progress/amount" >=> ProgressHandlers.createAmount ]
              setStatusCode StatusCodes.Status404NotFound >=> text "Not Found" ])
