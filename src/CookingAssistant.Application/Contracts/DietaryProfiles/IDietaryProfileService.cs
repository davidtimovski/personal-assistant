using CookingAssistant.Application.Contracts.DietaryProfiles.Models;
using CookingAssistant.Application.Contracts.Recipes.Models;
using Domain.CookingAssistant;
using FluentValidation;

namespace CookingAssistant.Application.Contracts.DietaryProfiles;

public interface IDietaryProfileService
{
    EditDietaryProfile Get(int userId);
    RecommendedDailyIntake GetRecommendedDailyIntake(GetRecommendedDailyIntake model, IValidator<GetRecommendedDailyIntake> validator);
    RecipeNutritionSummary CalculateNutritionSummary(Recipe recipe);
    Task CreateOrUpdateAsync(UpdateDietaryProfile model, IValidator<UpdateDietaryProfile> validator);
    Task DeleteAsync(int userId);
}
