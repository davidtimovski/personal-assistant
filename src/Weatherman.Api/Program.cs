using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using Weatherman.Application;
using Weatherman.Infrastructure;
using Weatherman.Persistence;

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
    .AddInfrastructure(builder.Configuration, builder.Environment.EnvironmentName)
    .AddPersistence(builder.Configuration["ConnectionString"])
    .AddWeatherman(builder.Configuration)
    .AddWeathermanInfrastructure()
    .AddWeathermanPersistence(builder.Configuration["ConnectionString"]);

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
    });

builder.Services.AddMvc(options =>
{
    options.EnableEndpointRouting = false;
});
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

builder.Services.AddHttpClient("open-meteo", c =>
{
    c.BaseAddress = new Uri("https://api.open-meteo.com/v1/");
});

var app = builder.Build();

app.UseExceptionHandler("/error");

app.UseAuthentication();
app.UseAuthorization();
app.UseMvc();

app.Run();
