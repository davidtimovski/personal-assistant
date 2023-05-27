module Startup

open System
open System.Collections.Generic
open System.Globalization
open System.IO
open Giraffe
open Core.Infrastructure
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Localization
open Microsoft.AspNetCore.Routing
open Microsoft.Extensions.Logging

let private addDataProtection (isProduction: bool) (services: IServiceCollection) (settings: IConfiguration) =
    if isProduction then
        let keyVaultUri = new Uri(settings["KeyVault:Url"])
        let tenantId = settings["KeyVault:TenantId"]
        let clientId = settings["KeyVault:ClientId"]
        let clientSecret = settings["KeyVault:ClientSecret"]

        services.AddDataProtectionWithCertificate(keyVaultUri, tenantId, clientId, clientSecret)
        |> ignore

let private errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

let private addLocalization (app: WebApplication) =
    let supportedCultures = [| new CultureInfo("en-US"); new CultureInfo("mk-MK") |]
    let localizationOptions = new RequestLocalizationOptions()
    localizationOptions.DefaultRequestCulture <- new RequestCulture(supportedCultures[0])
    localizationOptions.SupportedCultures <- supportedCultures
    localizationOptions.SupportedUICultures <- supportedCultures
    app.UseRequestLocalization(localizationOptions) |> ignore

let configureBuilder (builder: WebApplicationBuilder) (appName: string) =
    builder
        .Host
        .UseContentRoot(Directory.GetCurrentDirectory())
    |> ignore

    match builder.Environment.IsProduction() with
    | true ->
        let keyVaultUri = new Uri(builder.Configuration["KeyVault:Url"])
        let tenantId = builder.Configuration["KeyVault:TenantId"]
        let clientId = builder.Configuration["KeyVault:ClientId"]
        let clientSecret = builder.Configuration["KeyVault:ClientSecret"]
        builder.Host.AddKeyVault(keyVaultUri, tenantId, clientId, clientSecret) |> ignore

        builder.Host.AddSentryLogging(builder.Configuration[$"{appName}:Sentry:Dsn"], HashSet<string>(["GET /health"])) |> ignore
    | false ->
        builder.Host.ConfigureLogging(fun (loggingBuilder: ILoggingBuilder) ->
            loggingBuilder.AddConsole().AddDebug() |> ignore)
        |> ignore

let configureServices (services: IServiceCollection) =
    let serviceProvider = services.BuildServiceProvider()
    let settings = serviceProvider.GetService<IConfiguration>()
    let env = serviceProvider.GetService<IHostEnvironment>()

    addDataProtection (env.IsProduction()) services settings

    services.Configure<RouteOptions>(fun (opt: RouteOptions) ->
        opt.LowercaseUrls <- true
    ) |> ignore

    services.AddGiraffe() |> ignore

    // Use System.Text.Json serializer
    let serializationOptions = SystemTextJson.Serializer.DefaultOptions

    services.AddSingleton<Json.ISerializer>(SystemTextJson.Serializer(serializationOptions))
    |> ignore

    services.AddHealthChecks() |> ignore

let setupApp (app: WebApplication) =
    match app.Environment.IsProduction() with
    | true ->
        app.UseSentryTracing() |> ignore
        app.UseGiraffeErrorHandler(errorHandler) |> ignore
    | false -> app.UseDeveloperExceptionPage() |> ignore

    addLocalization app

    app.MapHealthChecks("/health") |> ignore
