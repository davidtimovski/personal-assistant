using System.Globalization;
using Account.Services;
using Accountant.Application;
using Accountant.Persistence;
using Application;
using Auth0.AspNetCore.Authentication;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CookingAssistant.Application;
using CookingAssistant.Persistence;
using FluentValidation.AspNetCore;
using Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.Net.Http.Headers;
using Persistence;
using Serilog;
using ToDoAssistant.Application;
using ToDoAssistant.Persistence;
using Weatherman.Application;
using Weatherman.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureAppConfiguration((context, configBuilder) =>
{
    if (context.HostingEnvironment.IsProduction())
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

builder.Services
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration, builder.Environment.EnvironmentName)
    .AddPersistence(builder.Configuration["ConnectionString"])
    .AddToDoAssistantPersistence(builder.Configuration["ConnectionString"])
    .AddCookingAssistantPersistence(builder.Configuration["ConnectionString"])
    .AddAccountantPersistence(builder.Configuration["ConnectionString"])
    .AddWeathermanPersistence(builder.Configuration["ConnectionString"])
    .AddToDoAssistant(builder.Configuration)
    .AddCookingAssistant(builder.Configuration)
    .AddAccountant(builder.Configuration)
    .AddWeatherman(builder.Configuration);

// Cookie configuration for HTTPS
if (builder.Environment.EnvironmentName == Environments.Production)
{
    builder.Services.Configure<CookiePolicyOptions>(options =>
    {
        options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
    });
}

builder.Services
    .AddAuth0WebAppAuthentication(options =>
    {
        options.Domain = builder.Configuration["Auth0:Domain"];
        options.ClientId = builder.Configuration["Auth0:ClientId"];
    });

builder.Services
    .AddLocalization(options => options.ResourcesPath = "Resources")
    .AddCors();

builder.Services.AddMvc(options =>
{
    options.EnableEndpointRouting = false;
})
    .AddViewLocalization();

builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

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
var cookingAssistantUrl = builder.Configuration["Urls:CookingAssistant"];
var accountantUrl = builder.Configuration["Urls:Accountant"];
var weathermanUrl = builder.Configuration["Urls:Weatherman"];

app.UseCors(builder =>
{
    builder.WithOrigins(
        toDoAssistantUrl,
        cookingAssistantUrl,
        accountantUrl,
        weathermanUrl
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
