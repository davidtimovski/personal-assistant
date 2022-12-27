module Routes

open System

open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Giraffe
open Models
open Core.Application.Contracts

let authorize =
    requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

let getUserId (ctx: HttpContext) =
    let userIdLookup = ctx.GetService<IUserIdLookup>()
    let usersRepository = ctx.GetService<IUsersRepository>()
    let auth0Id = ctx.User.Identity.Name

    match userIdLookup.Contains(auth0Id) with
    | true  ->
        userIdLookup.Get(auth0Id)
    | false ->
        let dbId = usersRepository.GetId(auth0Id)
        if dbId.HasValue then
            userIdLookup.Set(auth0Id, dbId.Value);
            dbId.Value
        else
            raise (Exception($"The user with auth0_id '{auth0Id}' does not have a mapping"))

let private getConnectionString (ctx: HttpContext) =
    let config = ctx.GetService<IConfiguration>()
    config["ConnectionString"]

let createLog: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! model = ctx.BindJsonAsync<CreateError>()
            let userId = getUserId ctx
 
            let connectionString = getConnectionString ctx

            Repository.createError model userId connectionString |> ignore

            ctx.SetStatusCode StatusCodes.Status201Created

            return! next ctx
        }

let webApp: HttpHandler =
    choose [
        POST >=>
            choose [
                route "/logs" >=> authorize >=> createLog
            ]
        setStatusCode StatusCodes.Status404NotFound >=> text "Not Found" ]

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message
