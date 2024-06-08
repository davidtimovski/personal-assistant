using System.Text.Json.Serialization;

namespace Chef.Api.Models.Recipes.Requests;

public record UpdateRecipeRequest(int Id, string Name, string? Description, List<UpdateRecipeIngredient> Ingredients, string? Instructions, TimeSpan PrepDuration, TimeSpan CookDuration, byte Servings, string? ImageUri, string? VideoUrl);

public record UpdateRecipeIngredient(int? Id, string Name, float? Amount, string? Unit);
