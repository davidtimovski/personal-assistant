using Infrastructure;
using Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddJsonFile("appsettings.json", false);
        config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true, true);
        config.AddEnvironmentVariables();
    })
    .ConfigureLogging((hostContext, logging) =>
    {
        if (hostContext.HostingEnvironment.IsProduction())
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(hostContext.Configuration)
                .CreateLogger();

            logging.AddSerilog(Log.Logger, dispose: true);
        }
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddCloudinary(
            hostContext.Configuration["Cloudinary:CloudName"],
            hostContext.Configuration["Cloudinary:ApiKey"],
            hostContext.Configuration["Cloudinary:ApiSecret"],
            hostContext.HostingEnvironment.EnvironmentName,
            hostContext.Configuration["Cloudinary:DefaultImageUris:Profile"],
            hostContext.Configuration["Cloudinary:DefaultImageUris:Recipe"]);

        services.AddHttpClient("fixer", c =>
        {
            c.BaseAddress = new Uri("http://data.fixer.io/api/");
        });

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddTransient<MidnightJob>();
    })
    .Build();

static async Task RunWorkerAsync(IServiceProvider services)
{
    using IServiceScope serviceScope = services.CreateScope();
    var job = serviceScope.ServiceProvider.GetRequiredService<MidnightJob>();
    await job.RunAsync();
}

await host.StartAsync();
await RunWorkerAsync(host.Services);
await host.StopAsync();
