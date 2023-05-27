using CookingAssistant.Application.Contracts.DietaryProfiles;
using CookingAssistant.Application.Contracts.Ingredients;
using CookingAssistant.Application.Contracts.Recipes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repositories.CookingAssistant;

namespace CookingAssistant.Persistence;

public static class IoC
{
    public static IServiceCollection AddCookingAssistantPersistence(this IServiceCollection services, string connectionString)
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddDbContext<CookingAssistantContext>(opt =>
        {
            opt.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention();
        });

        services.AddTransient<IRecipesRepository, RecipesRepository>();
        services.AddTransient<IIngredientsRepository, IngredientsRepository>();
        services.AddTransient<IDietaryProfilesRepository, DietaryProfilesRepository>();

        return services;
    }
}
