using System.Collections.Generic;

namespace Application.Contracts.CookingAssistant.Recipes.Models
{
    public class RecipeNutritionSummary
    {
        public bool IsSet { get; set; }
        public float ServingSize { get; set; }
        public bool ServingSizeIsOneUnit { get; set; }
        public float? Calories { get; set; }
        public float? CaloriesFromDaily { get; set; }
        public string CaloriesFromDailyGrade { get; set; }
        public float? Fat { get; set; }
        public float? FatFromDaily { get; set; }
        public string FatFromDailyGrade { get; set; }
        public float? SaturatedFat { get; set; }
        public float? SaturatedFatFromDaily { get; set; }
        public string SaturatedFatFromDailyGrade { get; set; }
        public float? Carbohydrate { get; set; }
        public float? CarbohydrateFromDaily { get; set; }
        public string CarbohydrateFromDailyGrade { get; set; }
        public float? Sugars { get; set; }
        public float? AddedSugars { get; set; }
        public float? AddedSugarsFromDaily { get; set; }
        public string AddedSugarsFromDailyGrade { get; set; }
        public float? Fiber { get; set; }
        public float? FiberFromDaily { get; set; }
        public string FiberFromDailyGrade { get; set; }
        public float? Protein { get; set; }
        public float? ProteinFromDaily { get; set; }
        public string ProteinFromDailyGrade { get; set; }
        public float? Sodium { get; set; }
        public float? SodiumFromDaily { get; set; }
        public string SodiumFromDailyGrade { get; set; }
        public float? Cholesterol { get; set; }
        public float? CholesterolFromDaily { get; set; }
        public string CholesterolFromDailyGrade { get; set; }
        public float? VitaminA { get; set; }
        public float? VitaminAFromDaily { get; set; }
        public string VitaminAFromDailyGrade { get; set; }
        public float? VitaminC { get; set; }
        public float? VitaminCFromDaily { get; set; }
        public string VitaminCFromDailyGrade { get; set; }
        public float? VitaminD { get; set; }
        public float? VitaminDFromDaily { get; set; }
        public string VitaminDFromDailyGrade { get; set; }
        public float? Calcium { get; set; }
        public float? CalciumFromDaily { get; set; }
        public string CalciumFromDailyGrade { get; set; }
        public float? Iron { get; set; }
        public float? IronFromDaily { get; set; }
        public string IronFromDailyGrade { get; set; }
        public float? Potassium { get; set; }
        public float? PotassiumFromDaily { get; set; }
        public string PotassiumFromDailyGrade { get; set; }
        public float? Magnesium { get; set; }
        public float? MagnesiumFromDaily { get; set; }
        public string MagnesiumFromDailyGrade { get; set; }
        public List<int> IngredientIds { get; set; } = new List<int>();
    }
}
