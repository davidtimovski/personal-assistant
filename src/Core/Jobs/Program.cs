using Cdn;
using Jobs;
using Jobs.Models;
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
            Serilog.Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(hostContext.Configuration)
                .CreateLogger();

            logging.AddSerilog(Serilog.Log.Logger, dispose: true);
        }
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddOptions<AppConfiguration>()
            .Bind(hostContext.Configuration)
            .ValidateDataAnnotations();

        services.AddCdn(hostContext.Configuration, hostContext.HostingEnvironment.EnvironmentName);

        var fixerApiConfig = hostContext.Configuration.GetSection("FixerApi").Get<FixerApiConfig>();
        if (fixerApiConfig is null)
        {
            throw new ArgumentNullException("Fixer API configuration is missing");
        }

        services.AddHttpClient("fixer", c =>
        {
            c.BaseAddress = new Uri(fixerApiConfig.BaseUrl);
        });

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddTransient<DailyJob>();
    })
    .Build();

static async Task RunWorkerAsync(IServiceProvider services)
{
    using IServiceScope serviceScope = services.CreateScope();
    var job = serviceScope.ServiceProvider.GetRequiredService<DailyJob>();
    await job.RunAsync();
}

await host.StartAsync();
await RunWorkerAsync(host.Services);
await host.StopAsync();
