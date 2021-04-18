using System;
using System.IO;
using System.Threading.Tasks;
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
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", false);
                    config.AddJsonFile($"appsettings.{environmentName}.json", true, true);
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
