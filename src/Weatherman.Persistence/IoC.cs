using Microsoft.Extensions.DependencyInjection;
using Weatherman.Application.Contracts.Forecasts;
using Weatherman.Persistence.Repositories;

namespace Weatherman.Persistence;

public static class IoC
{
    public static IServiceCollection AddWeathermanPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddTransient<IForecastsRepository, ForecastsRepository>();

        return services;
    }
}
