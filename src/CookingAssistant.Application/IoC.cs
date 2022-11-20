using CookingAssistant.Application.Contracts.Common;
using CookingAssistant.Application.Contracts.DietaryProfiles;
using CookingAssistant.Application.Contracts.DietaryProfiles.Models;
using CookingAssistant.Application.Contracts.Ingredients;
using CookingAssistant.Application.Contracts.Ingredients.Models;
using CookingAssistant.Application.Contracts.Recipes;
using CookingAssistant.Application.Contracts.Recipes.Models;
using CookingAssistant.Application.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CookingAssistant.Application;

public static class IoC
{
    public static IServiceCollection AddCookingAssistant(this IServiceCollection services, IConfiguration configuration)
    {
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
