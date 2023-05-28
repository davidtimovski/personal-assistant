using System.Globalization;
using Cdn;
using Cdn.Configuration;
using Core.Application;
using Core.Infrastructure;
using Core.Persistence;
using Microsoft.AspNetCore.Localization;
using ToDoAssistant.Api.Hubs;
using ToDoAssistant.Application;
using ToDoAssistant.Persistence;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.Host.AddKeyVault();
    builder.Services.AddDataProtectionWithCertificate(builder.Configuration);

    builder.Host.AddSentryLogging(builder.Configuration, "ToDoAssistant", new HashSet<string> { "GET /health", "GET /hub", "POST /hub/negotiate" });
}

builder.Services
    .AddApplication()
    .AddToDoAssistant();

var config = builder.Configuration.GetSection("Cloudinary").Get<CloudinaryConfig>();
if (config is null)
{
    throw new ArgumentNullException("Cloudinary configuration is missing");
}

builder.Services
    .AddAuth0(builder.Configuration, signalrHub: "/hub")
    .AddCdn(config, builder.Environment.EnvironmentName)
    .AddSender();

builder.Services
    .AddPersistence(builder.Configuration)
    .AddToDoAssistantPersistence(builder.Configuration);

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
