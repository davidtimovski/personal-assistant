using Chef.Application.Contracts.Recipes.Models;

namespace Chef.Api.Models.Recipes;

public class ImportRecipeDto
{
    public int Id { get; set; }
    public List<IngredientReplacement> IngredientReplacements { get; set; } = new();
    public bool CheckIfReviewRequired { get; set; }
}
