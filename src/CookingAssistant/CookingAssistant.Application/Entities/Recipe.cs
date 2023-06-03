using Core.Application;
using Core.Application.Entities;

namespace CookingAssistant.Application.Entities;

public class Recipe : Entity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public TimeSpan? PrepDuration { get; set; }
    public TimeSpan? CookDuration { get; set; }
    public byte Servings { get; set; }
    public string ImageUri { get; set; } = null!;
    public string? VideoUrl { get; set; }
    public DateTime LastOpenedDate { get; set; }

    public User? User { get; set; }
    public List<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    public List<RecipeShare> Shares { get; set; } = new List<RecipeShare>();

    public short IngredientsMissing { get; set; }
}
