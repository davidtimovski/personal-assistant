using System.Globalization;
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

    builder.Host.AddKeyVault(keyVaultUri, tenantId, clientId, clientSecret);
    builder.Services.AddDataProtectionWithCertificate(keyVaultUri, tenantId, clientId, clientSecret);

    builder.Host.AddSentryLogging(builder.Configuration["ToDoAssistant:Sentry:Dsn"], new HashSet<string> { "GET /health", "GET /hub", "POST /hub/negotiate" });
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

var connectionString = builder.Configuration["ConnectionString"];
builder.Services
    .AddPersistence(connectionString)
    .AddToDoAssistantPersistence(connectionString);

builder.Services.AddControllers();

builder.Services
    .AddLocalization(opt => opt.ResourcesPath = "Resources")
    .AddSignalR();

builder.Services.Configure<RouteOptions>(opt => opt.LowercaseUrls = true);

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
