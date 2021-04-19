using System;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
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
            })
#if !DEBUG
                .UseSerilog()
#endif
                .UseStartup<Startup>();
    }
}
