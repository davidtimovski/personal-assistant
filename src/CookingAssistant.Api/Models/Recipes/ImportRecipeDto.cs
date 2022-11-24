using CookingAssistant.Application.Contracts.Recipes.Models;

namespace CookingAssistant.Api.Models.Recipes;

public class ImportRecipeDto
{
    public int Id { get; set; }
    public List<IngredientReplacement> IngredientReplacements { get; set; } = new List<IngredientReplacement>();
    public bool CheckIfReviewRequired { get; set; }
}
