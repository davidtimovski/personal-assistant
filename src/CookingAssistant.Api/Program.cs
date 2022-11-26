﻿using System.Globalization;
using Application;
using CookingAssistant.Application;
using CookingAssistant.Application.Contracts.DietaryProfiles.Models;
using CookingAssistant.Persistence;
using Infrastructure;
using Microsoft.AspNetCore.Localization;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    var keyVaultUri = new Uri(builder.Configuration["KeyVault:Url"]);
    string tenantId = builder.Configuration["KeyVault:TenantId"];
    string clientId = builder.Configuration["KeyVault:ClientId"];
    string clientSecret = builder.Configuration["KeyVault:ClientSecret"];

    builder.Host.AddKeyVault(keyVaultUri, tenantId, clientId, clientSecret);
    builder.Services.AddDataProtectionWithCertificate(keyVaultUri, tenantId, clientId, clientSecret);
}

builder.Services
    .AddApplication()
    .AddCookingAssistant(builder.Configuration);

builder.Services
    .AddAuth0(
        authority: $"https://{builder.Configuration["Auth0:Domain"]}/",
        audience: builder.Configuration["Auth0:Audience"]
    )
    .AddCloudinary(builder.Configuration["Cloudinary:CloudName"],
                   builder.Configuration["Cloudinary:ApiKey"],
                   builder.Configuration["Cloudinary:ApiSecret"],
                   builder.Environment.EnvironmentName,
                   builder.Configuration["Cloudinary:DefaultImageUris:Profile"],
                   builder.Configuration["Cloudinary:DefaultImageUris:Recipe"])
    .AddCurrencies();

builder.Services
    .AddPersistence(builder.Configuration["ConnectionString"])
    .AddCookingAssistantPersistence();

builder.Services
    .AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddMvc(options =>
{
    options.EnableEndpointRouting = false;
});

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.Configure<DailyIntakeReference>(builder.Configuration.GetSection("DietaryProfile:ReferenceDailyIntake"));

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

app.UseAuthentication();
app.UseAuthorization();
app.UseMvc();

app.Run();