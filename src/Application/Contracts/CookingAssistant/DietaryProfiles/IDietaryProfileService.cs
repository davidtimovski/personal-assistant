using System.Threading.Tasks;
using FluentValidation;
using Application.Contracts.CookingAssistant.DietaryProfiles.Models;
using Application.Contracts.CookingAssistant.Recipes.Models;
using Domain.Entities.CookingAssistant;

namespace Application.Contracts.CookingAssistant.DietaryProfiles;

public interface IDietaryProfileService
{
    EditDietaryProfile Get(int userId);
    RecommendedDailyIntake GetRecommendedDailyIntake(GetRecommendedDailyIntake model, IValidator<GetRecommendedDailyIntake> validator);
    RecipeNutritionSummary CalculateNutritionSummary(Recipe recipe);
    Task CreateOrUpdateAsync(UpdateDietaryProfile model, IValidator<UpdateDietaryProfile> validator);
    Task DeleteAsync(int userId);
}
