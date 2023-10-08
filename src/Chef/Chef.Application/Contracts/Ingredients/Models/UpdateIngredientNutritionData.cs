namespace Chef.Application.Contracts.Ingredients.Models;

public class UpdateIngredientNutritionData
{
    public required bool IsSet { get; init; }
    public required float ServingSize { get; init; }
    public required bool ServingSizeIsOneUnit { get; init; }
    public required float? Calories { get; init; }
    public required float? Fat { get; init; }
    public required float? SaturatedFat { get; init; }
    public required float? Carbohydrate { get; init; }
    public required float? Sugars { get; init; }
    public required float? AddedSugars { get; init; }
    public required float? Fiber { get; init; }
    public required float? Protein { get; init; }
    public required float? Sodium { get; init; }
    public required float? Cholesterol { get; init; }
    public required float? VitaminA { get; init; }
    public required float? VitaminC { get; init; }
    public required float? VitaminD { get; init; }
    public required float? Calcium { get; init; }
    public required float? Iron { get; init; }
    public required float? Potassium { get; init; }
    public required float? Magnesium { get; init; }
}
