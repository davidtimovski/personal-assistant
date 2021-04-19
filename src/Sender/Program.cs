using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PersonalAssistant.Persistence;
using Serilog;

namespace PersonalAssistant.Sender
{
    class Program
    {
        static async Task Main()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    if (context.HostingEnvironment.EnvironmentName == "Production")
                    {
                        var builtConfiguration = config.Build();

                        string url = builtConfiguration["KeyVault:Url"];
                        string tenantId = builtConfiguration["KeyVault:TenantId"];
                        string clientId = builtConfiguration["KeyVault:ClientId"];
                        string clientSecret = builtConfiguration["KeyVault:ClientSecret"];

                        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

                        var client = new SecretClient(new Uri(url), credential);
                        config.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions());
                    }

                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", false);
                    config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostContext, logging) =>
                {
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(hostContext.Configuration)
                        .CreateLogger();

                    logging.AddSerilog(Log.Logger, dispose: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddPersistence(hostContext.Configuration["ConnectionString"]);

                    services.AddOptions();

                    services.AddHostedService<HostedService>();
                });

            await hostBuilder.RunConsoleAsync();
        }
    }
}
