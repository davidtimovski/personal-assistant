using System.Globalization;
using System.Reflection;
using Auth0.AspNetCore.Authentication;
using Cdn;
using Chef.Application;
using Chef.Persistence;
using Core.Application;
using Core.Infrastructure;
using Core.Persistence;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using PersonalAssistant.Web.Models;
using PersonalAssistant.Web.Services;
using ToDoAssistant.Application;
using ToDoAssistant.Persistence;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.Host.ConfigureAppConfiguration((hostContext, configBuilder) => configBuilder.AddKeyVault())
                .ConfigureLogging(configureLogging => configureLogging.AddSentryLogging(builder.Configuration, "PersonalAssistant", new HashSet<string> { "GET /health" }));

    builder.Services.AddDataProtectionWithCertificate(builder.Configuration);
}

builder.Services.Configure<FormOptions>(options =>
{
    const int bodyLengthLimitInMegabytes = 10;
    options.MultipartBodyLengthLimit = bodyLengthLimitInMegabytes * 1024 * 1024;
});

builder.Services
    .AddApplication()
    .AddToDoAssistant()
    .AddChef(builder.Configuration);

builder.Services
    .AddUserIdMapper()
    .AddCdn(builder.Configuration, builder.Environment.EnvironmentName)
    .AddSender(builder.Configuration);

builder.Services
    .AddPersistence(builder.Configuration)
    .AddToDoAssistantPersistence(builder.Configuration)
    .AddChefPersistence(builder.Configuration);

builder.Services.AddHealthChecks();

// Cookie configuration for HTTPS
if (builder.Environment.EnvironmentName == Environments.Production)
{
    builder.Services.Configure<CookiePolicyOptions>(opt =>
    {
        opt.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
    });
}

builder.Services
    .AddAuth0WebAppAuthentication(opt =>
    {
        opt.Domain = builder.Configuration["Auth0:Domain"]!;
        opt.ClientId = builder.Configuration["Auth0:ClientId"]!;
    });

builder.Services
    .AddLocalization(opt => opt.ResourcesPath = "Resources")
    .AddCors();

builder.Services.AddMvc(opt =>
{
    opt.EnableEndpointRouting = false;
})
    .AddViewLocalization();

builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters()
    .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddHttpClient();
builder.Services.Configure<RouteOptions>(opt => opt.LowercaseUrls = true);

builder.Services.AddTransient<IEmailTemplateService, EmailTemplateService>();

if (builder.Environment.EnvironmentName == Environments.Development)
{
    // SameSite cookie workaround for Chrome
    builder.Services.ConfigureNonBreakingSameSiteCookies();
}

builder.Services.AddOptions<AppConfiguration>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotations();

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

var appSettings = app.Services.GetRequiredService<IOptions<AppConfiguration>>();
app.UseCors(builder =>
{
    builder.WithOrigins(
        appSettings.Value.Urls.ToDoAssistant,
        appSettings.Value.Urls.Chef,
        appSettings.Value.Urls.Accountant,
        appSettings.Value.Urls.Weatherman
    );
    builder.WithMethods("GET");
    builder.WithHeaders("Authorization");
});

app.MapHealthChecks("/health");

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
