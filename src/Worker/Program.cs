using System;
using System.IO;
using System.Net.Http;
using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Application.Contracts.Common;
using Infrastructure.Cdn;
using Serilog;

namespace Worker;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", false);
                config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true, true);
                config.AddEnvironmentVariables();
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
                services.AddSingleton<ICdnService>(new CloudinaryService(
                    cloudinaryAccount: new Account(
                        hostContext.Configuration["Cloudinary:CloudName"],
                        hostContext.Configuration["Cloudinary:ApiKey"],
                        hostContext.Configuration["Cloudinary:ApiSecret"]),
                    hostContext.HostingEnvironment.EnvironmentName,
                    hostContext.Configuration["DefaultImageUris:Profile"],
                    hostContext.Configuration["DefaultImageUris:Recipe"],
                    new HttpClient()));

                services.AddHttpClient("fixer", c =>
                {
                    c.BaseAddress = new Uri("http://data.fixer.io/api/");
                });

                services.AddHostedService<MidnightWorker>();
            });
}