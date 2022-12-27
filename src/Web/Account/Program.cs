using System.Globalization;
using System.Reflection;
using Account.Services;
using Accountant.Application;
using Accountant.Persistence;
using Auth0.AspNetCore.Authentication;
using Cdn;
using CookingAssistant.Application;
using CookingAssistant.Persistence;
using Core.Application;
using Core.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Sender;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.Net.Http.Headers;
using Persistence;
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

    builder.Host.ConfigureLogging((context, loggingBuilder) =>
    {
        loggingBuilder.AddConfiguration(context.Configuration);
        loggingBuilder.AddSentry();
    });
}

builder.Services
    .AddApplication()
    .AddToDoAssistant()
    .AddCookingAssistant(builder.Configuration)
    .AddAccountant();

builder.Services
    .AddUserIdMapper()
    .AddCdn(builder.Configuration["Cloudinary:CloudName"],
            builder.Configuration["Cloudinary:ApiKey"],
            builder.Configuration["Cloudinary:ApiSecret"],
            builder.Environment.EnvironmentName,
            builder.Configuration["Cloudinary:DefaultImageUris:Profile"],
            builder.Configuration["Cloudinary:DefaultImageUris:Recipe"])
    .AddSender();

builder.Services
    .AddPersistence(builder.Configuration["ConnectionString"])
    .AddToDoAssistantPersistence()
    .AddCookingAssistantPersistence()
    .AddAccountantPersistence();

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
    .AddFluentValidationClientsideAdapters()
    .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddHttpClient();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

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
