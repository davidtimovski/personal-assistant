module Weatherman.Api.App

open Giraffe
open Core.Infrastructure
open Core.Persistence
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Weatherman.Application
open Weatherman.Infrastructure
open Weatherman.Persistence
open Routes

let private configureServices (services: IServiceCollection) =
    let serviceProvider = services.BuildServiceProvider()
    let settings = serviceProvider.GetService<IConfiguration>()
    Startup.configureServices services

    services.AddWeatherman() |> ignore

    services
        .AddAuth0("https://" + settings["Auth0:Domain"] + "/", settings["Auth0:Audience"])
        .AddWeathermanInfrastructure()
    |> ignore

    services.AddPersistence(settings["ConnectionString"]).AddWeathermanPersistence()
    |> ignore

    services.AddHttpClient("open-meteo") |> ignore

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
