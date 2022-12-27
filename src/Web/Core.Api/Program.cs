using Core.Application;
using Core.Infrastructure;
using Core.Persistence;

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

builder.Services.AddApplication();

builder.Services
    .AddAuth0(
        authority: $"https://{builder.Configuration["Auth0:Domain"]}/",
        audience: builder.Configuration["Auth0:Audience"]
    );

builder.Services.AddPersistence(builder.Configuration["ConnectionString"]);

builder.Services.AddControllers();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsProduction())
{
    app.UseSentryTracing();
}

app.Run();
