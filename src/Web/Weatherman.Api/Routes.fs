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
            [ GET >=> authorize >=> choose [ route "/forecasts" >=> ForecastHandlers.get ]
              setStatusCode StatusCodes.Status404NotFound >=> text "Not Found" ])
