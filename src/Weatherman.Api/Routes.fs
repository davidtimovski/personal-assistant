module Routes

open Giraffe
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Http

let authorize: HttpHandler =
    requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

let webApp: HttpHandler =
    choose
        [ GET >=> authorize >=> route "/api/forecasts" >=> ForecastHandlers.get
          setStatusCode StatusCodes.Status404NotFound >=> text "Not Found" ]
