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

builder.Services.AddCors(options =>
{
    var toDoAssistantUrl = builder.Configuration["Urls:ToDoAssistant"];
    var cookingAssistantUrl = builder.Configuration["Urls:CookingAssistant"];
    var accountantUrl = builder.Configuration["Urls:Accountant"];
    var weathermanUrl = builder.Configuration["Urls:Weatherman"];

    options.AddPolicy("AllowAllApps", builder =>
    {
        builder.WithOrigins(toDoAssistantUrl, cookingAssistantUrl, accountantUrl, weathermanUrl)
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
