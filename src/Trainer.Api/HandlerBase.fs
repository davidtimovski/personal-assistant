module HandlerBase

open System
open Application.Contracts
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Localization
open Microsoft.Extensions.Logging

let getUserId (ctx: HttpContext) =
    let userIdLookup = ctx.GetService<IUserIdLookup>()
    let usersRepository = ctx.GetService<IUsersRepository>()
    let auth0Id = ctx.User.Identity.Name

    match userIdLookup.Contains(auth0Id) with
    | true -> userIdLookup.Get(auth0Id)
    | false ->
        let dbId = usersRepository.GetId(auth0Id)

        if dbId.HasValue then
            userIdLookup.Set(auth0Id, dbId.Value)
            dbId.Value
        else
            raise (Exception($"The user with auth0_id '{auth0Id}' does not have a mapping"))

let successOrLog (handler: HttpHandler) (next: HttpFunc) (ctx: HttpContext) =
    task {
        try
            return! handler next ctx
        with ex ->
            let logger = ctx.GetService<ILogger<HttpContext>>()
            logger.LogError(ex, $"Unexpected error in handler for request: {ctx.Request.Method} {ctx.Request.Path}")

            return! ServerErrors.INTERNAL_ERROR "An unexpected error occurred" next ctx
    }

let localize (ctx: HttpContext) text =
    let enTranslations = Map ["Encrypted", "[ Encrypted ]"; "Uncategorized", "Uncategorized"]
    let mkTranslations = Map ["Encrypted", "[ Шифриран ]"; "Uncategorized", "Некатегоризирано"]
    let lookup = Map ["en-US", enTranslations; "mk-MK", mkTranslations]

    let rqf = ctx.Request.HttpContext.Features.Get<IRequestCultureFeature>();

    lookup[rqf.RequestCulture.Culture.Name][text]
