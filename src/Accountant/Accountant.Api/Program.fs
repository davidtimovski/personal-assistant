module Accountant.Api.App

open Giraffe
open Core.Application
open Core.Infrastructure
open Core.Persistence
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Routes

let private configureServices (services: IServiceCollection) =
    let serviceProvider = services.BuildServiceProvider()
    let settings = serviceProvider.GetService<IConfiguration>()

    Startup.configureServices services

    services.AddApplication() |> ignore

    services.AddAuth0(settings)
    |> ignore

    services.AddPersistence(settings) |> ignore

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

    Startup.configureBuilder builder "Accountant"

    builder.Host.ConfigureServices(configureServices) |> ignore

    let app = builder.Build()

    Startup.setupApp app

    app.UseAuthentication().UseGiraffe(webApp)

    app.Run()
    0
