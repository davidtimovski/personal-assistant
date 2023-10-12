using Chef.Application.Contracts.DietaryProfiles.Models;
using Chef.Application.Contracts.Recipes.Models;
using Chef.Application.Entities;
using FluentValidation;
using Sentry;

namespace Chef.Application.Contracts.DietaryProfiles;

public interface IDietaryProfileService
{
    EditDietaryProfile? Get(int userId, ISpan metricsSpan);
    RecommendedDailyIntake GetRecommendedDailyIntake(GetRecommendedDailyIntake model, IValidator<GetRecommendedDailyIntake> validator, ISpan metricsSpan);
    RecipeNutritionSummary CalculateNutritionSummary(Recipe recipe, ISpan metricsSpan);
    Task CreateOrUpdateAsync(UpdateDietaryProfile model, IValidator<UpdateDietaryProfile> validator, ISpan metricsSpan);
    Task DeleteAsync(int userId, ISpan metricsSpan);
}
