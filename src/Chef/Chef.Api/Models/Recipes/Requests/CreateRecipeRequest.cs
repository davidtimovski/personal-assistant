using System.ComponentModel.DataAnnotations;

namespace Chef.Api.Models.Recipes.Requests;

public record CreateRecipeRequest([Required] string Name, [Required] string? Description, [Required] List<UpdateRecipeIngredient> Ingredients, [Required] string? Instructions, [Required] TimeSpan? PrepDuration, [Required] TimeSpan? CookDuration, [Required] byte Servings, [Required] string? ImageUri, [Required] string? VideoUrl);
