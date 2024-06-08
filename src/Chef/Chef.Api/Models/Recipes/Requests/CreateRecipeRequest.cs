namespace Chef.Api.Models.Recipes.Requests;

public record CreateRecipeRequest(string Name, string? Description, List<UpdateRecipeIngredient> Ingredients, string? Instructions, TimeSpan PrepDuration, TimeSpan CookDuration, byte Servings, string? ImageUri, string? VideoUrl);
