using Core.Application.Contracts;
using Core.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Persistence;

public static class IoC
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var config = configuration.Get<PersistenceConfiguration>();
        if (config is null)
        {
            throw new ArgumentNullException("Persistence configuration is missing");
        }

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddDbContext<CoreContext>(opt =>
        {
            opt.UseNpgsql(config.ConnectionString)
               .UseSnakeCaseNamingConvention();
        });

        services.AddTransient<IUsersRepository, UsersRepository>();
        services.AddTransient<IPushSubscriptionsRepository, PushSubscriptionsRepository>();
        services.AddTransient<ITooltipsRepository, TooltipsRepository>();
        services.AddTransient<ICurrenciesRepository, CurrenciesRepository>();
        services.AddTransient<ICsvRepository, CsvRepository>();

        return services;
    }
}
