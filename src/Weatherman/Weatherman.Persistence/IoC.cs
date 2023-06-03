using Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Weatherman.Application.Contracts.Forecasts;
using Weatherman.Persistence.Models;
using Weatherman.Persistence.Repositories;

namespace Weatherman.Persistence;

public static class IoC
{
    public static IServiceCollection AddWeathermanPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var config = configuration.Get<PersistenceConfiguration>();
        if (config is null)
        {
            throw new ArgumentNullException("Persistence configuration is missing");
        }

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddDbContext<WeathermanContext>(opt =>
        {
            opt.UseNpgsql(config.ConnectionString)
               .UseSnakeCaseNamingConvention();
        });

        services.AddTransient<IForecastsRepository, ForecastsRepository>();

        return services;
    }
}
