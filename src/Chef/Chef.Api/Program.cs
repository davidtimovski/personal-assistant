using System.Globalization;
using Cdn;
using Chef.Api.Models;
using Chef.Application;
using Chef.Persistence;
using Core.Application;
using Core.Infrastructure;
using Core.Persistence;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.Host.ConfigureAppConfiguration((hostContext, configBuilder) => configBuilder.AddKeyVault())
                .ConfigureLogging(configureLogging => configureLogging.AddSentryLogging(builder.Configuration, "Chef", new HashSet<string> { "GET /health" }));

    builder.Services.AddDataProtectionWithCertificate(builder.Configuration);
}

builder.Services.Configure<FormOptions>(options =>
{
    const int bodyLengthLimitInMegabytes = 10;
    options.MultipartBodyLengthLimit = bodyLengthLimitInMegabytes * 1024 * 1024;
});

builder.Services
    .AddApplication()
    .AddChef(builder.Configuration);

builder.Services
    .AddAuth0(builder.Configuration)
    .AddCdn(builder.Configuration, builder.Environment.EnvironmentName)
    .AddSender(builder.Configuration);

builder.Services
    .AddPersistence(builder.Configuration)
    .AddChefPersistence(builder.Configuration);

builder.Services
    .AddLocalization(opt => opt.ResourcesPath = "Resources");

builder.Services.AddControllers();
builder.Services.Configure<RouteOptions>(opt => opt.LowercaseUrls = true);

builder.Services.AddHealthChecks();

builder.Services.AddOptions<AppConfiguration>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotations();

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

if (app.Environment.IsProduction())
{
    app.UseSentryTracing();
}

app.Run();
