using System.Collections.Generic;
using Application.Contracts.CookingAssistant.Recipes.Models;

namespace Api.Models.CookingAssistant.Recipes;

public class ImportRecipeDto
{
    public int Id { get; set; }
    public List<IngredientReplacement> IngredientReplacements { get; set; } = new List<IngredientReplacement>();
    public bool CheckIfReviewRequired { get; set; }
}
