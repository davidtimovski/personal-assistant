using System.Globalization;
using Application;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Infrastructure;
using Microsoft.AspNetCore.Localization;
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

    builder.Services.AddDataProtectionWithCertificate(keyVaultUri, tenantId, clientId, clientSecret);

    builder.Host.ConfigureLogging((context, loggingBuilder) =>
    {
        loggingBuilder.AddConfiguration(context.Configuration);
        loggingBuilder.AddSentry();
    });
}

builder.Services
    .AddApplication()
    .AddToDoAssistant();

builder.Services
    .AddAuth0(
        authority: $"https://{builder.Configuration["Auth0:Domain"]}/",
        audience: builder.Configuration["Auth0:Audience"],
        signalrHub: "/hub"
    )
    .AddCloudinary(builder.Configuration["Cloudinary:CloudName"],
                   builder.Configuration["Cloudinary:ApiKey"],
                   builder.Configuration["Cloudinary:ApiSecret"],
                   builder.Environment.EnvironmentName,
                   builder.Configuration["Cloudinary:DefaultImageUris:Profile"],
                   builder.Configuration["Cloudinary:DefaultImageUris:Recipe"]);

builder.Services
    .AddPersistence(builder.Configuration["ConnectionString"])
    .AddToDoAssistantPersistence();

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
