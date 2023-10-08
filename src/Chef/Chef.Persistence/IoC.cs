using Chef.Application.Contracts.DietaryProfiles;
using Chef.Application.Contracts.Ingredients;
using Chef.Application.Contracts.Recipes;
using Chef.Persistence.Models;
using Chef.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chef.Persistence;

public static class IoC
{
    public static IServiceCollection AddChefPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var config = configuration.Get<PersistenceConfiguration>();
        if (config is null)
        {
            throw new ArgumentNullException("Persistence configuration is missing");
        }

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddDbContext<ChefContext>(opt =>
        {
            opt.UseNpgsql(config.ConnectionString)
                   .UseSnakeCaseNamingConvention();
        });

        services.AddTransient<IRecipesRepository, RecipesRepository>();
        services.AddTransient<IIngredientsRepository, IngredientsRepository>();
        services.AddTransient<IDietaryProfilesRepository, DietaryProfilesRepository>();

        return services;
    }
}
