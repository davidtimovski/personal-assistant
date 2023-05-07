using Core.Application.Contracts;
using Core.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Persistence;

public static class IoC
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddTransient<IUsersRepository, UsersRepository>();
        services.AddTransient<IPushSubscriptionsRepository, PushSubscriptionsRepository>();
        services.AddTransient<ITooltipsRepository, TooltipsRepository>();
        services.AddTransient<ICurrenciesRepository, CurrenciesRepository>();
        services.AddTransient<ICsvRepository, CsvRepository>();

        services.AddDbContext<PersonalAssistantContext>(opt =>
        {
            opt.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention();
        });

        return services;
    }
}
