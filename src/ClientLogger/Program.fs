module ClientLogger.App

open System
open System.IO
open Azure.Extensions.AspNetCore.Configuration.Secrets
open Azure.Identity
open Azure.Security.KeyVault.Secrets
open Giraffe
open global.Infrastructure
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Persistence
open Routes

let addKeyVault (context: WebHostBuilderContext) (configBuilder: IConfigurationBuilder) =
    if context.HostingEnvironment.IsProduction() then
        let keyVaultUri = new Uri(context.Configuration["KeyVault:Url"])
        let tenantId = context.Configuration["KeyVault:TenantId"];
        let clientId = context.Configuration["KeyVault:ClientId"];
        let clientSecret = context.Configuration["KeyVault:ClientSecret"];

        let tokenCredential = new ClientSecretCredential(tenantId, clientId, clientSecret)

        let secretClient = new SecretClient(keyVaultUri, tokenCredential)
        configBuilder.AddAzureKeyVault(secretClient, new AzureKeyVaultConfigurationOptions()) |> ignore

let addDataProtection (isProduction : bool) (services : IServiceCollection) (settings : IConfiguration) =
    if isProduction then
        let keyVaultUri = new Uri(settings["KeyVault:Url"])
        let tenantId = settings["KeyVault:TenantId"];
        let clientId = settings["KeyVault:ClientId"];
        let clientSecret = settings["KeyVault:ClientSecret"];

        services.AddDataProtectionWithCertificate(keyVaultUri, tenantId, clientId, clientSecret) |> ignore

let configureServices (services : IServiceCollection) =
    let serviceProvider = services.BuildServiceProvider()
    let settings = serviceProvider.GetService<IConfiguration>()
    let env = serviceProvider.GetService<IHostEnvironment>()

    addDataProtection (env.IsProduction()) services settings

    services.AddAuth0(
        "https://" + settings["Auth0:Domain"] + "/",
        settings["Auth0:Audience"]
    ) |> ignore

    services.AddPersistence(settings["ConnectionString"]) |> ignore

    services.AddGiraffe() |> ignore

    // Use System.Text.Json serializer
    let serializationOptions = SystemTextJson.Serializer.DefaultOptions
    services.AddSingleton<Json.ISerializer>(SystemTextJson.Serializer(serializationOptions)) |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()

    (match env.IsDevelopment() with
    | true  ->
        app.UseDeveloperExceptionPage()
    | false ->
        app.UseGiraffeErrorHandler(errorHandler)) |> ignore

    app.UseAuthentication()
       .UseGiraffe(webApp)

let configureLogging (builder : ILoggingBuilder) =
    builder.AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main args =
    let contentRoot = Directory.GetCurrentDirectory()

    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .UseContentRoot(contentRoot)
                    .ConfigureAppConfiguration(addKeyVault)
                    .ConfigureServices(configureServices)
                    .Configure(Action<IApplicationBuilder> configureApp)
                    .ConfigureLogging(configureLogging)
                    |> ignore)
        .Build()
        .Run()
    0
