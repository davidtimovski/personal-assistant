module Routes

open System
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Giraffe
open Models
open Repository

let private getConnectionString (ctx: HttpContext) =
    let config = ctx.GetService<IConfiguration>()
    config.["ConnectionString"]

let createLog: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! error = ctx.BindJsonAsync<LogError>()
            let connectionString = getConnectionString ctx

            createError error connectionString |> ignore

            ctx.SetStatusCode 201

            return! next ctx
        }

let webApp: HttpHandler =
    choose [
        POST >=>
             choose [
                route "/logs"
                >=> createLog
            ]
        setStatusCode 404 >=> text "Not Found" ]

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message
