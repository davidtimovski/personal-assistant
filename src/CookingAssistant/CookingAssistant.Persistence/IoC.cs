using CookingAssistant.Application.Contracts.DietaryProfiles;
using CookingAssistant.Application.Contracts.Ingredients;
using CookingAssistant.Application.Contracts.Recipes;
using CookingAssistant.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repositories.CookingAssistant;

namespace CookingAssistant.Persistence;

public static class IoC
{
    public static IServiceCollection AddCookingAssistantPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var config = configuration.Get<PersistenceConfiguration>();
        if (config is null)
        {
            throw new ArgumentNullException("Persistence configuration is missing");
        }

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddDbContext<CookingAssistantContext>(opt =>
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
