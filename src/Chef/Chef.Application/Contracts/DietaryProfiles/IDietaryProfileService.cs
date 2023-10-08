using Chef.Application.Contracts.DietaryProfiles.Models;
using Chef.Application.Contracts.Recipes.Models;
using Chef.Application.Entities;
using FluentValidation;

namespace Chef.Application.Contracts.DietaryProfiles;

public interface IDietaryProfileService
{
    EditDietaryProfile? Get(int userId);
    RecommendedDailyIntake GetRecommendedDailyIntake(GetRecommendedDailyIntake model, IValidator<GetRecommendedDailyIntake> validator);
    RecipeNutritionSummary CalculateNutritionSummary(Recipe recipe);
    Task CreateOrUpdateAsync(UpdateDietaryProfile model, IValidator<UpdateDietaryProfile> validator);
    Task DeleteAsync(int userId);
}
