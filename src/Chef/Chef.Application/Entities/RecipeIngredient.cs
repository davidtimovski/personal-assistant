using Core.Application;

namespace Chef.Application.Entities;

public class RecipeIngredient : Entity
{
    public int RecipeId { get; set; }
    public int IngredientId { get; set; }
    public float? Amount { get; set; }
    public string? Unit { get; set; }

    public Recipe? Recipe { get; set; }
    public Ingredient? Ingredient { get; set; }
}
