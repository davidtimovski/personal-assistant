using System;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sender;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureAppConfiguration((context, configBuilder) =>
{
    if (context.HostingEnvironment.EnvironmentName == Environments.Production)
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
})
    .ConfigureLogging((hostContext, logging) =>
    {
        if (hostContext.HostingEnvironment.EnvironmentName == Environments.Production)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(hostContext.Configuration)
                .CreateLogger();

            logging.AddSerilog(Log.Logger, dispose: true);
        }
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<HostedService>();
    });

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var app = builder.Build();
await app.RunAsync();
