namespace Application.Contracts.CookingAssistant.Ingredients.Models;

public class IngredientNutritionData
{
    public bool IsSet { get; set; }
    public float ServingSize { get; set; }
    public bool ServingSizeIsOneUnit { get; set; }
    public float? Calories { get; set; }
    public float? Fat { get; set; }
    public float? SaturatedFat { get; set; }
    public float? Carbohydrate { get; set; }
    public float? Sugars { get; set; }
    public float? AddedSugars { get; set; }
    public float? Fiber { get; set; }
    public float? Protein { get; set; }
    public float? Sodium { get; set; }
    public float? Cholesterol { get; set; }
    public float? VitaminA { get; set; }
    public float? VitaminC { get; set; }
    public float? VitaminD { get; set; }
    public float? Calcium { get; set; }
    public float? Iron { get; set; }
    public float? Potassium { get; set; }
    public float? Magnesium { get; set; }
}