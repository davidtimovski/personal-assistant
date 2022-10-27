using System;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.Config;
using Api.Hubs;
using Application;
using Application.Contracts.CookingAssistant.DietaryProfiles.Models;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


if (builder.Environment.IsProduction())
{
    builder.Host.ConfigureAppConfiguration((context, configBuilder) =>
    {
        var config = configBuilder.Build();

        string url = config["KeyVault:Url"];
        string tenantId = config["KeyVault:TenantId"];
        string clientId = config["KeyVault:ClientId"];
        string clientSecret = config["KeyVault:ClientSecret"];

        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

        var client = new SecretClient(new Uri(url), credential);
        configBuilder.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions());
    });
}

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services
    .AddInfrastructure(builder.Configuration, builder.Environment.EnvironmentName)
    .AddPersistence(builder.Configuration["ConnectionString"])
    .AddApplication(builder.Configuration);

builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
            options.Audience = builder.Configuration["Auth0:Audience"];
            // If the access token does not have a `sub` claim, `User.Identity.Name` will be `null`. Map it to a different claim by setting the NameClaimType below.
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = ClaimTypes.NameIdentifier
            };

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
    var weathermanUrl = builder.Configuration["Urls:Weatherman"];

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

    options.AddPolicy("AllowWeatherman", builder =>
    {
        builder.WithOrigins(weathermanUrl)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .SetPreflightMaxAge(TimeSpan.FromDays(20));
    });

    options.AddPolicy("AllowAllApps", builder =>
    {
        builder.WithOrigins(toDoAssistantUrl, cookingAssistantUrl, accountantUrl, weathermanUrl)
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

builder.Services.AddHttpClient("open-meteo", c =>
{
    c.BaseAddress = new Uri("https://api.open-meteo.com/v1/");
});
builder.Services.AddHttpClient("client-logger", c =>
{
    c.BaseAddress = new Uri("http://clientlogger/");
});
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
