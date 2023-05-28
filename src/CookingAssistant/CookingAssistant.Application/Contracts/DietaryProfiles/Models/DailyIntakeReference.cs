namespace CookingAssistant.Application.Contracts.DietaryProfiles.Models;

public class DailyIntakeReference
{
    public List<DailyIntakeAgeGroup> Female { get; set; } = null!;
    public List<DailyIntakeAgeGroup> Male { get; set; } = null!;
}

public class DailyIntakeAgeGroup
{
    public short AgeFrom { get; set; }
    public short AgeTo { get; set; }
    public RecommendedIntake RecommendedIntake { get; set; } = null!;
}

public class RecommendedIntake
{
    public short TotalFatFrom { get; set; }
    public short TotalFatTo { get; set; }
    public short SaturatedFatMax { get; set; }
    public short Carbohydrate { get; set; }
    public short AddedSugarsMax { get; set; }
    public float Fiber { get; set; }
    public short Protein { get; set; }
    public short Sodium { get; set; }
    public short CholesterolMax { get; set; }
    public short VitaminA { get; set; }
    public short VitaminC { get; set; }
    public short VitaminD { get; set; }
    public short Calcium { get; set; }
    public short Iron { get; set; }
    public short Potassium { get; set; }
    public short Magnesium { get; set; }
}