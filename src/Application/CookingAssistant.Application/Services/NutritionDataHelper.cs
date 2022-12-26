using Application.Domain.CookingAssistant;

namespace CookingAssistant.Application.Services;

public static class NutritionDataHelper
{
    public static bool Has(Ingredient ingredient)
    {
        var hasNutritionData = ingredient.Calories.HasValue
                            || ingredient.Fat.HasValue
                            || ingredient.SaturatedFat.HasValue
                            || ingredient.Carbohydrate.HasValue
                            || ingredient.Sugars.HasValue
                            || ingredient.AddedSugars.HasValue
                            || ingredient.Fiber.HasValue
                            || ingredient.Protein.HasValue
                            || ingredient.Sodium.HasValue
                            || ingredient.Cholesterol.HasValue
                            || ingredient.VitaminA.HasValue
                            || ingredient.VitaminC.HasValue
                            || ingredient.VitaminD.HasValue
                            || ingredient.Calcium.HasValue
                            || ingredient.Iron.HasValue
                            || ingredient.Potassium.HasValue
                            || ingredient.Magnesium.HasValue;

        return hasNutritionData;
    }
}
