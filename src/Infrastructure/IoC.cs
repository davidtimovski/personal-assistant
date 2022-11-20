using Application.Contracts;
using CloudinaryDotNet;
using Infrastructure.Cdn;
using Infrastructure.Currency;
using Infrastructure.Identity;
using Infrastructure.Sender;
using Infrastructure.Weatherman;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Weatherman.Application.Contracts.Forecasts;

namespace Infrastructure;

public static class IoC
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string environmentName)
    {
        services.AddTransient<ICurrencyService, CurrencyService>();
        services.AddTransient<IForecastService, ForecastService>();

        services.AddSingleton<IUserIdLookup, UserIdLookup>();
        services.AddSingleton<ISenderService, SenderService>();

        services.AddSingleton<ICdnService>(new CloudinaryService(
            cloudinaryAccount: new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]),
            environmentName,
            configuration["DefaultImageUris:Profile"],
            configuration["DefaultImageUris:Recipe"],
            new HttpClient()));

        return services;
    }
}
