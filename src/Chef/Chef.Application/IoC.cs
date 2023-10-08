using System.Reflection;
using Chef.Application.Contracts.Common;
using Chef.Application.Contracts.DietaryProfiles;
using Chef.Application.Contracts.DietaryProfiles.Models;
using Chef.Application.Contracts.Ingredients;
using Chef.Application.Contracts.Ingredients.Models;
using Chef.Application.Contracts.Recipes;
using Chef.Application.Contracts.Recipes.Models;
using Chef.Application.Services;
using Chef.Utility;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chef.Application;

public static class IoC
{
    public static IServiceCollection AddChef(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddOptions<DailyIntakeReference>()
            .Bind(configuration.GetSection("DietaryProfile:ReferenceDailyIntake"));

        services.AddTransient<IConversion, Conversion>();

        services.AddTransient<IValidator<CreateRecipe>, CreateRecipeValidator>();
        services.AddTransient<IValidator<UpdateRecipe>, UpdateRecipeValidator>();
        services.AddTransient<IValidator<ShareRecipe>, ShareRecipeValidator>();
        services.AddTransient<IValidator<CreateSendRequest>, CreateSendRequestValidator>();
        services.AddTransient<IValidator<ImportRecipe>, ImportRecipeValidator>();
        services.AddTransient<IValidator<UpdateIngredient>, UpdateIngredientValidator>();
        services.AddTransient<IValidator<UpdatePublicIngredient>, UpdatePublicIngredientValidator>();
        services.AddTransient<IValidator<GetRecommendedDailyIntake>, GetRecommendedDailyIntakeValidator>();
        services.AddTransient<IValidator<UpdateDietaryProfile>, UpdateDietaryProfileValidator>();

        services.AddTransient<IRecipeService, RecipeService>();
        services.AddTransient<IIngredientService, IngredientService>();
        services.AddTransient<IDietaryProfileService, DietaryProfileService>();

        var activityMultiplier = new Dictionary<string, float>();
        configuration.Bind("DietaryProfile:ActivityMultiplier", activityMultiplier);
        var dietaryGoalCalories = new Dictionary<string, short>();
        configuration.Bind("DietaryProfile:DietaryGoalCalories", dietaryGoalCalories);
        services.AddSingleton<IDailyIntakeHelper>(new DailyIntakeHelper(activityMultiplier, dietaryGoalCalories));

        return services;
    }
}
