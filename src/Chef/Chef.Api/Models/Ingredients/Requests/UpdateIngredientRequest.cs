namespace Chef.Api.Models.Ingredients.Requests;

public record UpdateIngredientRequest(int Id, int? TaskId, string Name, UpdateIngredientNutritionData NutritionData, UpdateIngredientPriceData PriceData);
