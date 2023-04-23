module Weatherman.Api.App

open System.IO
open Giraffe
open Core.Infrastructure
open Core.Persistence
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Routes

let private configureServices (services: IServiceCollection) =
    let serviceProvider = services.BuildServiceProvider()
    let settings = serviceProvider.GetService<IConfiguration>()

    Startup.configureServices services

    services.AddAuth0("https://" + settings["Auth0:Domain"] + "/", settings["Auth0:Audience"])
    |> ignore

    services.AddPersistence(settings["ConnectionString"]) |> ignore

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

    Startup.configureBuilder builder

    builder.Host.ConfigureServices(configureServices) |> ignore

    let app = builder.Build()

    Startup.setupApp app

    app.UseAuthentication().UseGiraffe(webApp)

    app.Run()
    0
