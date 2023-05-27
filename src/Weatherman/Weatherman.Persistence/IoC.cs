using Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Weatherman.Application.Contracts.Forecasts;
using Weatherman.Persistence.Repositories;

namespace Weatherman.Persistence;

public static class IoC
{
    public static IServiceCollection AddWeathermanPersistence(this IServiceCollection services, string connectionString)
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddDbContext<WeathermanContext>(opt =>
        {
            opt.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention();
        });

        services.AddTransient<IForecastsRepository, ForecastsRepository>();

        return services;
    }
}
