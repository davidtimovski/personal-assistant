using System.ComponentModel.DataAnnotations;

namespace Chef.Api.Models.Recipes.Requests;

public record UpdateRecipeRequest([Required] int Id, [Required] string Name, [Required] string? Description, [Required] List<UpdateRecipeIngredient> Ingredients, [Required] string? Instructions, [Required] TimeSpan? PrepDuration, [Required] TimeSpan? CookDuration, [Required] byte Servings, [Required] string? ImageUri, [Required] string? VideoUrl);

public record UpdateRecipeIngredient([Required] int? Id, [Required] string Name, [Required] float? Amount, [Required] string? Unit);
