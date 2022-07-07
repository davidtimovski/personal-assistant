module Routes

open System
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Giraffe
open Models

let private getConnectionString (ctx: HttpContext) =
    let config = ctx.GetService<IConfiguration>()
    config.["ConnectionString"]

let createLog: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            match ctx.TryGetRequestHeader "UserId" with
            | None ->
                ctx.SetStatusCode StatusCodes.Status401Unauthorized
                return! next ctx
            | Some userIdHeader ->
                let! model = ctx.BindJsonAsync<CreateError>()
                let userId = (userIdHeader |> int)

                let connectionString = getConnectionString ctx

                Repository.createError model userId connectionString |> ignore

                ctx.SetStatusCode StatusCodes.Status201Created

                return! next ctx
        }

let webApp: HttpHandler =
    choose [
        POST >=>
            choose [
                route "/logs" >=> createLog
            ]
        setStatusCode StatusCodes.Status404NotFound >=> text "Not Found" ]

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message
