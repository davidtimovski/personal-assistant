module Startup

open System
open System.Globalization
open System.IO
open Azure.Extensions.AspNetCore.Configuration.Secrets
open Azure.Identity
open Azure.Security.KeyVault.Secrets
open Giraffe
open Core.Infrastructure
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Localization
open Microsoft.Extensions.Logging

let private addKeyVault (context: HostBuilderContext) (configBuilder: IConfigurationBuilder) =
    if context.HostingEnvironment.IsProduction() then
        let keyVaultUri = new Uri(context.Configuration["KeyVault:Url"])
        let tenantId = context.Configuration["KeyVault:TenantId"]
        let clientId = context.Configuration["KeyVault:ClientId"]
        let clientSecret = context.Configuration["KeyVault:ClientSecret"]

        let tokenCredential = new ClientSecretCredential(tenantId, clientId, clientSecret)

        let secretClient = new SecretClient(keyVaultUri, tokenCredential)

        configBuilder.AddAzureKeyVault(secretClient, new AzureKeyVaultConfigurationOptions())
        |> ignore

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

let configureBuilder (builder: WebApplicationBuilder) =
    builder
        .Host
        .UseContentRoot(Directory.GetCurrentDirectory())
        .ConfigureAppConfiguration(addKeyVault)
    |> ignore

    match builder.Environment.IsProduction() with
    | true ->
        builder.Host.ConfigureLogging(fun (loggingBuilder: ILoggingBuilder) ->
            loggingBuilder.AddConfiguration(builder.Configuration).AddSentry() |> ignore)
        |> ignore
    | false ->
        builder.Host.ConfigureLogging(fun (loggingBuilder: ILoggingBuilder) ->
            loggingBuilder.AddConsole().AddDebug() |> ignore)
        |> ignore

let configureServices (services: IServiceCollection) =
    let serviceProvider = services.BuildServiceProvider()
    let settings = serviceProvider.GetService<IConfiguration>()
    let env = serviceProvider.GetService<IHostEnvironment>()

    addDataProtection (env.IsProduction()) services settings

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
