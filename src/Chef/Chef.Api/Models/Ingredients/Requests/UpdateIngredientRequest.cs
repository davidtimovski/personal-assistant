using System.ComponentModel.DataAnnotations;

namespace Chef.Api.Models.Ingredients.Requests;

public record UpdateIngredientRequest([Required] int Id, [Required] int? TaskId, [Required] string Name, [Required] UpdateIngredientNutritionData NutritionData, [Required] UpdateIngredientPriceData PriceData);
