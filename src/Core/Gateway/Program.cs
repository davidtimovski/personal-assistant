using Gateway.Models;
using Microsoft.AspNetCore.Http.Features;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((context, configBuilder) =>
{
    configBuilder.AddJsonFile("ocelot.json");

    if (context.HostingEnvironment.IsProduction())
    {
        configBuilder.AddJsonFile("ocelot.Production.json");
    }
});

builder.Services.AddCors(opt =>
{
    var config = builder.Configuration.Get<AppConfiguration>();
    if (config is null)
    {
        throw new ArgumentNullException("App configuration is missing");
    }

    opt.AddPolicy("AllowAllApps", corsBuilder =>
    {
        corsBuilder.WithOrigins(
            config.Urls.ToDoAssistant,
            config.Urls.Chef,
            config.Urls.Accountant,
            config.Urls.Weatherman,
            config.Urls.Trainer)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials() // For SignalR
            .SetPreflightMaxAge(TimeSpan.FromDays(20));
    });
});

builder.Services.Configure<FormOptions>(options =>
{
    const int bodyLengthLimitInMegabytes = 10;
    options.MultipartBodyLengthLimit = bodyLengthLimitInMegabytes * 1024 * 1024;
});

builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors("AllowAllApps");

app.UseWebSockets();
app.UseOcelot().Wait();

app.Run();
