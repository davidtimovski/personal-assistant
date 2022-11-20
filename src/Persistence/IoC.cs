﻿using Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repositories;

namespace Persistence;

public static class IoC
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddTransient<IUsersRepository, UsersRepository>();
        services.AddTransient<IPushSubscriptionsRepository, PushSubscriptionsRepository>();
        services.AddTransient<ITooltipsRepository, TooltipsRepository>();

        services.AddDbContext<PersonalAssistantContext>(options =>
        {
            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention();
        });

        return services;
    }
}
