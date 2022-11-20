using Accountant.Application;
using Accountant.Persistence;
using Application;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Infrastructure;
using Persistence;
using Serilog;
using ToDoAssistant.Application;
using ToDoAssistant.Persistence;
using Weatherman.Application;
using Weatherman.Persistence;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.Host.ConfigureAppConfiguration((context, configBuilder) =>
    {
        var config = configBuilder.Build();

        string url = config["KeyVault:Url"];
        string tenantId = config["KeyVault:TenantId"];
        string clientId = config["KeyVault:ClientId"];
        string clientSecret = config["KeyVault:ClientSecret"];

        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

        var client = new SecretClient(new Uri(url), credential);
        configBuilder.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions());
    });
}

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration, builder.Environment.EnvironmentName)
    .AddPersistence(builder.Configuration["ConnectionString"])
    .AddToDoAssistant(builder.Configuration)
    .AddToDoAssistantPersistence(builder.Configuration["ConnectionString"])
    .AddAccountant(builder.Configuration)
    .AddAccountantPersistence(builder.Configuration["ConnectionString"])
    .AddWeatherman(builder.Configuration)
    .AddWeathermanPersistence(builder.Configuration["ConnectionString"]);

builder.Services.AddMvc(options =>
{
    options.EnableEndpointRouting = false;
});
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

var app = builder.Build();

app.UseExceptionHandler("/error");

app.UseMvc();

app.Run();
