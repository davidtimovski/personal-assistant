module Accountant.Api.App

open System
open System.Globalization
open System.IO
open Accountant.Application
open Accountant.Persistence
open Azure.Extensions.AspNetCore.Configuration.Secrets
open Azure.Identity
open Azure.Security.KeyVault.Secrets
open Giraffe
open global.Infrastructure
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Localization
open Microsoft.Extensions.Logging
open Persistence
open Routes

let addKeyVault (context: HostBuilderContext) (configBuilder: IConfigurationBuilder) =
    if context.HostingEnvironment.IsProduction() then
        let keyVaultUri = new Uri(context.Configuration["KeyVault:Url"])
        let tenantId = context.Configuration["KeyVault:TenantId"]
        let clientId = context.Configuration["KeyVault:ClientId"]
        let clientSecret = context.Configuration["KeyVault:ClientSecret"]

        let tokenCredential = new ClientSecretCredential(tenantId, clientId, clientSecret)

        let secretClient = new SecretClient(keyVaultUri, tokenCredential)

        configBuilder.AddAzureKeyVault(secretClient, new AzureKeyVaultConfigurationOptions())
        |> ignore

let addDataProtection (isProduction: bool) (services: IServiceCollection) (settings: IConfiguration) =
    if isProduction then
        let keyVaultUri = new Uri(settings["KeyVault:Url"])
        let tenantId = settings["KeyVault:TenantId"]
        let clientId = settings["KeyVault:ClientId"]
        let clientSecret = settings["KeyVault:ClientSecret"]

        services.AddDataProtectionWithCertificate(keyVaultUri, tenantId, clientId, clientSecret)
        |> ignore

let configureServices (services: IServiceCollection) =
    let serviceProvider = services.BuildServiceProvider()
    let settings = serviceProvider.GetService<IConfiguration>()
    let env = serviceProvider.GetService<IHostEnvironment>()

    addDataProtection (env.IsProduction()) services settings

    services.AddAccountant() |> ignore

    services.AddAuth0("https://" + settings["Auth0:Domain"] + "/", settings["Auth0:Audience"])
    |> ignore

    services.AddPersistence(settings["ConnectionString"]).AddAccountantPersistence()
    |> ignore

    services.AddGiraffe() |> ignore

    // Use System.Text.Json serializer
    let serializationOptions = SystemTextJson.Serializer.DefaultOptions
    services.AddSingleton<Json.ISerializer>(SystemTextJson.Serializer(serializationOptions)) |> ignore

let addLocalization (app: WebApplication) =
    let supportedCultures = [| new CultureInfo("en-US"); new CultureInfo("mk-MK") |]
    let localizationOptions = new RequestLocalizationOptions()
    localizationOptions.DefaultRequestCulture <- new RequestCulture(supportedCultures[0])
    localizationOptions.SupportedCultures <- supportedCultures
    localizationOptions.SupportedUICultures <- supportedCultures
    app.UseRequestLocalization(localizationOptions) |> ignore

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

[<EntryPoint>]
let main args =
    let contentRoot = Directory.GetCurrentDirectory()

    let builder = WebApplication.CreateBuilder(args)

    builder.Host.UseContentRoot(contentRoot).ConfigureAppConfiguration(addKeyVault)
    |> ignore

    if builder.Environment.IsProduction() then
        builder.Host.ConfigureLogging(fun (loggingBuilder: ILoggingBuilder) ->
            loggingBuilder.AddConfiguration(builder.Configuration).AddSentry() |> ignore)
        |> ignore

    builder.Host.ConfigureServices(configureServices) |> ignore

    let app = builder.Build()

    (match app.Environment.IsDevelopment() with
     | true -> app.UseDeveloperExceptionPage()
     | false -> app.UseGiraffeErrorHandler(errorHandler))
    |> ignore

    addLocalization app

    app.UseAuthentication().UseGiraffe(webApp)

    app.Run()
    0