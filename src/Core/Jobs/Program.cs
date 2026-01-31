using Cdn;
using Core.Infrastructure;
using Jobs;
using Jobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

var hostBuilder = Host.CreateDefaultBuilder(args);

using var host = hostBuilder
    .ConfigureAppConfiguration((hostContext, configBuilder) =>
    {
        configBuilder.SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile("appsettings.json", false)
                     .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true, true)
                     .AddEnvironmentVariables()
                     .AddKeyVault();
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
    .ConfigureHostOptions((hostBuilderContext, _) =>
    {
        if (hostBuilderContext.HostingEnvironment.IsProduction())
        {
            SentrySdk.Init(opt =>
            {
                opt.Dsn = hostBuilderContext.Configuration["Jobs:Sentry:Dsn"];
                opt.SampleRate = 1;
                opt.StackTraceMode = StackTraceMode.Enhanced;
            });
        }
    }).Build();

static async Task RunWorkerAsync(IServiceProvider services)
{
    using IServiceScope serviceScope = services.CreateScope();
    var job = serviceScope.ServiceProvider.GetRequiredService<DailyJob>();
    await job.RunAsync();
}

await host.StartAsync();
await RunWorkerAsync(host.Services);
await host.StopAsync();
