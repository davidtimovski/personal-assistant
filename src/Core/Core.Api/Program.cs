using Core.Api.Models;
using Core.Application;
using Core.Infrastructure;
using Core.Persistence;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.Host.ConfigureAppConfiguration((hostContext, configBuilder) => configBuilder.AddKeyVault())
                .ConfigureLogging(configureLogging => configureLogging.AddSentryLogging(builder.Configuration, "Core", new HashSet<string> { "GET /health" }));

    builder.Services.AddDataProtectionWithCertificate(builder.Configuration);
}

builder.Services.AddApplication();

builder.Services.AddAuth0(builder.Configuration);

builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddControllers();
builder.Services.Configure<RouteOptions>(opt => opt.LowercaseUrls = true);

builder.Services.AddHealthChecks();

builder.Services.AddOptions<AppConfiguration>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotations();

var app = builder.Build();

app.MapHealthChecks("/health");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsProduction())
{
    app.UseSentryTracing();
}

app.Run();
