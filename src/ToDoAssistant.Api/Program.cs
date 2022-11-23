using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Application;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using ToDoAssistant.Api.Hubs;
using ToDoAssistant.Application;
using ToDoAssistant.Persistence;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    var keyVaultUri = new Uri(builder.Configuration["KeyVault:Url"]);
    string tenantId = builder.Configuration["KeyVault:TenantId"];
    string clientId = builder.Configuration["KeyVault:ClientId"];
    string clientSecret = builder.Configuration["KeyVault:ClientSecret"];

    var tokenCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);

    builder.Host.ConfigureAppConfiguration((context, configBuilder) =>
    {
        var secretClient = new SecretClient(keyVaultUri, tokenCredential);
        configBuilder.AddAzureKeyVault(secretClient, new AzureKeyVaultConfigurationOptions());
    });

    var certClient = new CertificateClient(keyVaultUri, tokenCredential);
    X509Certificate2 certificate = certClient.DownloadCertificate("personal-assistant");

    builder.Services
        .AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(@"storage/dataprotection-keys/"))
        .ProtectKeysWithCertificate(certificate);

    builder.Host.ConfigureLogging((context, loggingBuilder) =>
    {
        loggingBuilder.AddConfiguration(context.Configuration);
        loggingBuilder.AddSentry();
    });
}

builder.Services
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration, builder.Environment.EnvironmentName)
    .AddPersistence(builder.Configuration["ConnectionString"])
    .AddToDoAssistant(builder.Configuration)
    .AddToDoAssistantPersistence(builder.Configuration["ConnectionString"]);

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
                if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/hub")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services
    .AddLocalization(options => options.ResourcesPath = "Resources")
    .AddSignalR();

builder.Services.AddMvc(options =>
{
    options.EnableEndpointRouting = false;
});
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

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

app.MapHub<ListActionsHub>("/hub");

app.Run();
