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
    var toDoAssistantUrl = builder.Configuration["Urls:ToDoAssistant"];
    var cookingAssistantUrl = builder.Configuration["Urls:CookingAssistant"];
    var accountantUrl = builder.Configuration["Urls:Accountant"];
    var weathermanUrl = builder.Configuration["Urls:Weatherman"];
    var trainerUrl = builder.Configuration["Urls:Trainer"];

    opt.AddPolicy("AllowAllApps", corsBuilder =>
    {
        corsBuilder.WithOrigins(toDoAssistantUrl, cookingAssistantUrl, accountantUrl, weathermanUrl, trainerUrl)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials() // For SignalR
            .SetPreflightMaxAge(TimeSpan.FromDays(20));
    });
});

builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors("AllowAllApps");

app.UseWebSockets();
app.UseOcelot().Wait();

app.Run();
