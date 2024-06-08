namespace Chef.Api.Models.Ingredients.Requests;

public record UpdateIngredientNutritionData(bool IsSet, float ServingSize, bool ServingSizeIsOneUnit, float? Calories, float? Fat, float? SaturatedFat, float? Carbohydrate, float? Sugars, float? AddedSugars, float? Fiber, float? Protein, float? Sodium, float? Cholesterol, float? VitaminA, float? VitaminC, float? VitaminD, float? Calcium, float? Iron, float? Potassium, float? Magnesium);
