using Core.Infrastructure;
using Sender;
using Sender.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .ConfigureAppConfiguration((hostContext, configBuilder) =>
    {
        if (builder.Environment.IsProduction())
        {
            configBuilder.AddKeyVault();
        }
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
        services.AddOptions<AppConfiguration>()
            .Bind(builder.Configuration)
            .ValidateDataAnnotations();

        services.AddHostedService<HostedService>();
    });

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var app = builder.Build();
await app.RunAsync();
