using Microsoft.Extensions.DependencyInjection;
using Weatherman.Application.Contracts.Forecasts;

namespace Weatherman.Infrastructure;

public static class IoC
{
    public static IServiceCollection AddWeathermanInfrastructure(this IServiceCollection services)
    {
        services.AddTransient<IForecastService, ForecastService>();

        return services;
    }
}
