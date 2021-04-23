using System;
using System.Threading.Tasks;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace PersonalAssistant.Sender
{
    class Program
    {
        static async Task Main()
        {
            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    if (hostContext.HostingEnvironment.EnvironmentName == Environments.Production)
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

            await hostBuilder.RunConsoleAsync();
        }
    }
}
