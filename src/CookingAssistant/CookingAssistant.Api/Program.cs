using System.Globalization;
using Cdn;
using Cdn.Configuration;
using CookingAssistant.Application;
using CookingAssistant.Application.Contracts.DietaryProfiles.Models;
using CookingAssistant.Persistence;
using Core.Application;
using Core.Infrastructure;
using Core.Persistence;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.Host.AddKeyVault();
    builder.Services.AddDataProtectionWithCertificate(builder.Configuration);
}

builder.Services
    .AddApplication()
    .AddCookingAssistant(builder.Configuration);

var config = builder.Configuration.GetSection("Cloudinary").Get<CloudinaryConfig>();
if (config is null)
{
    throw new ArgumentNullException("Cloudinary configuration is missing");
}

builder.Services
    .AddAuth0(builder.Configuration)
    .AddCdn(config, builder.Environment.EnvironmentName)
    .AddSender();

builder.Services
    .AddPersistence(builder.Configuration)
    .AddCookingAssistantPersistence(builder.Configuration);

builder.Services
    .AddLocalization(opt => opt.ResourcesPath = "Resources");

builder.Services.AddControllers();
builder.Services.Configure<RouteOptions>(opt => opt.LowercaseUrls = true);
builder.Services.Configure<DailyIntakeReference>(builder.Configuration.GetSection("DietaryProfile:ReferenceDailyIntake"));

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

app.Run();
