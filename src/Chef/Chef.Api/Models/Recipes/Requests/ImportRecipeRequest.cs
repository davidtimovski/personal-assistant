using Chef.Application.Contracts.Recipes.Models;

namespace Chef.Api.Models.Recipes.Requests;

public record ImportRecipeRequest(int Id, List<IngredientReplacement> IngredientReplacements, bool CheckIfReviewRequired);
