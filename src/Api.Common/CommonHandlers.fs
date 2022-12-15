module CommonHandlers

open Giraffe
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

let authorize: HttpHandler =
    requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

let successOrLog (handler: HttpHandler) (next: HttpFunc) (ctx: HttpContext) =
    task {
        try
            return! handler next ctx
        with ex ->
            let logger = ctx.GetService<ILogger<HttpContext>>()
            logger.LogError(ex, $"Unexpected error in handler for request: {ctx.Request.Method} {ctx.Request.Path}")

            return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
    }
