using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Application;
using Account.Models;
using Account.Services;
using Auth0.AspNetCore.Authentication;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FluentValidation.AspNetCore;
using Infrastructure;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
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

if (builder.Environment.IsProduction())
{
    builder.Host.UseSerilog();
}

builder.Services
    .AddInfrastructure(builder.Configuration, builder.Environment.EnvironmentName)
    .AddPersistence(builder.Configuration["ConnectionString"])
    .AddApplication(builder.Configuration);

builder.Services.AddDbContext<AccountContext>(options =>
{
    options.UseNpgsql(builder.Configuration["ConnectionString"]);
});
// https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Cookie configuration for HTTPS
if (builder.Environment.EnvironmentName == Environments.Production)
{
    builder.Services.Configure<CookiePolicyOptions>(options =>
    {
        options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
    });
}

builder.Services
    .AddAuth0WebAppAuthentication(options => {
        options.Domain = builder.Configuration["Auth0:Domain"];
        options.ClientId = builder.Configuration["Auth0:ClientId"];
    });

builder.Services.AddApplicationInsightsTelemetry()
    .AddLocalization(options => options.ResourcesPath = "Resources")
    .AddCors();

builder.Services.AddMvc(options =>
{
    options.EnableEndpointRouting = false;
})
    .AddViewLocalization()
    .AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<Program>());

builder.Services.AddHttpClient();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddTransient<IEmailTemplateService, EmailTemplateService>();

if (builder.Environment.EnvironmentName == Environments.Development)
{
    // SameSite cookie workaround for Chrome
    builder.Services.ConfigureNonBreakingSameSiteCookies();
}

var app = builder.Build();

if (builder.Environment.EnvironmentName == Environments.Production)
{
    app.Use((context, next) =>
    {
        context.Request.Scheme = "https";
        return next();
    });

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
}

var supportedCultures = new[] {
    new CultureInfo("en-US"), // Default
    new CultureInfo("mk-MK")
};
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture: supportedCultures[0].Name, uiCulture: supportedCultures[0].Name),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

var toDoAssistantUrl = builder.Configuration["Urls:ToDoAssistant"];
var toDoAssistant2Url = builder.Configuration["Urls:ToDoAssistant2"];
var cookingAssistantUrl = builder.Configuration["Urls:CookingAssistant"];
var accountantUrl = builder.Configuration["Urls:Accountant"];
var accountant2Url = builder.Configuration["Urls:Accountant2"];

app.UseCors(builder =>
{
    builder.WithOrigins(
        toDoAssistantUrl,
        toDoAssistant2Url,
        cookingAssistantUrl,
        accountantUrl,
        accountant2Url
    );
    builder.WithMethods("GET");
    builder.WithHeaders("Authorization");
});

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers[HeaderNames.CacheControl] = $"public,max-age={60 * 60 * 24 * 7}";
    }
});
app.UseCookiePolicy();
app.UseStatusCodePagesWithReExecute("/error", "?code={0}");

app.UseMvcWithDefaultRoute();

app.Run();
