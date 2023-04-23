using System.Globalization;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Cdn;
using Core.Application;
using Core.Infrastructure;
using Core.Persistence;
using Infrastructure.Sender;
using Microsoft.AspNetCore.Localization;
using ToDoAssistant.Api.Hubs;
using ToDoAssistant.Application;
using ToDoAssistant.Persistence;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    var keyVaultUri = new Uri(builder.Configuration["KeyVault:Url"]);
    string tenantId = builder.Configuration["KeyVault:TenantId"];
    string clientId = builder.Configuration["KeyVault:ClientId"];
    string clientSecret = builder.Configuration["KeyVault:ClientSecret"];

    var tokenCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);

    builder.Host.ConfigureAppConfiguration((context, configBuilder) =>
    {
        var secretClient = new SecretClient(keyVaultUri, tokenCredential);
        configBuilder.AddAzureKeyVault(secretClient, new AzureKeyVaultConfigurationOptions());
    });

    builder.Services.AddDataProtectionWithCertificate(keyVaultUri, tenantId, clientId, clientSecret);

    builder.Host.ConfigureLogging((context, loggingBuilder) =>
    {
        loggingBuilder.AddConfiguration(context.Configuration);
        loggingBuilder.AddSentry();
    });
}

builder.Services
    .AddApplication()
    .AddToDoAssistant();

builder.Services
    .AddAuth0(
        authority: $"https://{builder.Configuration["Auth0:Domain"]}/",
        audience: builder.Configuration["Auth0:Audience"],
        signalrHub: "/hub"
    )
    .AddCdn(builder.Configuration["Cloudinary:CloudName"],
            builder.Configuration["Cloudinary:ApiKey"],
            builder.Configuration["Cloudinary:ApiSecret"],
            builder.Environment.EnvironmentName,
            builder.Configuration["Cloudinary:DefaultImageUris:Profile"],
            builder.Configuration["Cloudinary:DefaultImageUris:Recipe"])
    .AddSender();

builder.Services
    .AddPersistence(builder.Configuration["ConnectionString"])
    .AddToDoAssistantPersistence();

builder.Services.AddControllers();

builder.Services
    .AddLocalization(options => options.ResourcesPath = "Resources")
    .AddSignalR();

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler("/error");

var supportedCultures = new[] {
    new CultureInfo("en-US"),
    new CultureInfo("mk-MK")
};
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture: supportedCultures[0].Name, uiCulture: supportedCultures[0].Name),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.MapHealthChecks("/health");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ListActionsHub>("/hub");

if (app.Environment.IsProduction())
{
    app.UseSentryTracing();
}

app.Run();
