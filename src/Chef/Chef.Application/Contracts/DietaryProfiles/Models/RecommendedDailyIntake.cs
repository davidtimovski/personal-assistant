namespace Chef.Application.Contracts.DietaryProfiles.Models;

public class RecommendedDailyIntake
{
    public short Calories { get; set; }
    //public short Fat { get; set; }
    public short SaturatedFat { get; set; }
    public short Carbohydrate { get; set; }
    public short AddedSugars { get; set; }
    public float Fiber { get; set; }
    public short Protein { get; set; }
    public short Sodium { get; set; }
    public short Cholesterol { get; set; }
    public short VitaminA { get; set; }
    public short VitaminC { get; set; }
    public short VitaminD { get; set; }
    public short Calcium { get; set; }
    public short Iron { get; set; }
    public short Potassium { get; set; }
    public short Magnesium { get; set; }
}