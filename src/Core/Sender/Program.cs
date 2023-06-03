using Core.Infrastructure;
using Sender;
using Sender.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.Host.AddKeyVault();
}

builder.Host
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
