using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((context, configBuilder) =>
{
    configBuilder.AddJsonFile("ocelot.json");

    if (context.HostingEnvironment.EnvironmentName == Environments.Production)
    {
        configBuilder.AddJsonFile("ocelot.Production.json");
    }
});

builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
            options.Audience = builder.Configuration["Auth0:Audience"];
            // If the access token does not have a `sub` claim, `User.Identity.Name` will be `null`. Map it to a different claim by setting the NameClaimType below.
            options.TokenValidationParameters = new TokenValidationParameters
            {
                NameClaimType = ClaimTypes.NameIdentifier
            };
        });

builder.Services.AddCors(options =>
{
    var toDoAssistantUrl = builder.Configuration["Urls:ToDoAssistant"];
    var toDoAssistant2Url = builder.Configuration["Urls:ToDoAssistant2"];
    var cookingAssistantUrl = builder.Configuration["Urls:CookingAssistant"];
    var accountantUrl = builder.Configuration["Urls:Accountant"];
    var accountant2Url = builder.Configuration["Urls:Accountant2"];

    options.AddPolicy("AllowAllApps", builder =>
    {
        builder.WithOrigins(toDoAssistantUrl, toDoAssistant2Url, cookingAssistantUrl, accountantUrl, accountant2Url)
               .AllowAnyMethod()
               .AllowAnyHeader()
               .SetPreflightMaxAge(TimeSpan.FromDays(20));
    });
});

builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors("AllowAllApps");

// Have user ID claim be named "sub"
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

app.UseAuthentication();
app.UseAuthorization();

app.UseOcelot().Wait();

app.Run();
