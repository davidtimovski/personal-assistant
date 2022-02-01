﻿using System;
using System.Globalization;
using System.Threading.Tasks;
using Api.Config;
using Api.Hubs;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Application;
using Application.Contracts.CookingAssistant.DietaryProfiles.Models;
using Infrastructure;
using Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureAppConfiguration((context, configBuilder) =>
{
    if (context.HostingEnvironment.EnvironmentName == Environments.Production)
    {
        var config = configBuilder.Build();

        string url = config["KeyVault:Url"];
        string tenantId = config["KeyVault:TenantId"];
        string clientId = config["KeyVault:ClientId"];
        string clientSecret = config["KeyVault:ClientSecret"];

        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

        var client = new SecretClient(new Uri(url), credential);
        configBuilder.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions());
    }
});
#if !DEBUG
builder.Host.UseSerilog();
#endif

builder.Services
    .AddInfrastructure(builder.Configuration, builder.Environment.EnvironmentName)
    .AddPersistence(builder.Configuration["ConnectionString"])
    .AddApplication(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = builder.Configuration["Urls:Authority"];
        options.Audience = "personal-assistant-api";

#if DEBUG
        options.RequireHttpsMetadata = false;
#endif

        // For SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/toDoAssistantHub")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddCors(options =>
{
    var toDoAssistantUrl = builder.Configuration["Urls:ToDoAssistant"];
    var cookingAssistantUrl = builder.Configuration["Urls:CookingAssistant"];
    var accountantUrl = builder.Configuration["Urls:Accountant"];

    options.AddPolicy("AllowToDoAssistant", builder =>
    {
        builder.WithOrigins(toDoAssistantUrl)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials() // For SignalR
               .SetPreflightMaxAge(TimeSpan.FromDays(20));
    });

    options.AddPolicy("AllowCookingAssistant", builder =>
    {
        builder.WithOrigins(cookingAssistantUrl)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .SetPreflightMaxAge(TimeSpan.FromDays(20));
    });

    options.AddPolicy("AllowAccountant", builder =>
    {
        builder.WithOrigins(accountantUrl)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .SetPreflightMaxAge(TimeSpan.FromDays(20));
    });

    options.AddPolicy("AllowAllApps", builder =>
    {
        builder.WithOrigins(toDoAssistantUrl, cookingAssistantUrl, accountantUrl)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .SetPreflightMaxAge(TimeSpan.FromDays(20));
    });
});

builder.Services.AddApplicationInsightsTelemetry()
    .AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies())
    .AddLocalization(options => options.ResourcesPath = "Resources")
    .AddSignalR();

builder.Services.AddMvc(options =>
{
    options.EnableEndpointRouting = false;
})
    .AddNewtonsoftJson();

builder.Services.AddHttpClient();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

builder.Services.Configure<Urls>(builder.Configuration.GetSection("Urls"));
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

app.UseCors("AllowAllApps");

app.UseAuthentication();
app.UseAuthorization();
app.UseMvc();

app.MapHub<ToDoAssistantHub>("/toDoAssistantHub");

app.Run();
