namespace Application.Contracts.CookingAssistant.DietaryProfiles.Models;

public class DailyIntake
{
    public short Calories { get; set; }
    public short? CustomCalories { get; set; }
    public bool TrackCalories { get; set; } = true;
    //public short Fat { get; set; }
    //public short? CustomFat { get; set; }
    //public bool TrackFat { get; set; } = true;
    public short SaturatedFat { get; set; }
    public short? CustomSaturatedFat { get; set; }
    public bool TrackSaturatedFat { get; set; } = true;
    public short Carbohydrate { get; set; }
    public short? CustomCarbohydrate { get; set; }
    public bool TrackCarbohydrate { get; set; } = true;
    public short AddedSugars { get; set; }
    public short? CustomAddedSugars { get; set; }
    public bool TrackAddedSugars { get; set; } = true;
    public float Fiber { get; set; }
    public float? CustomFiber { get; set; }
    public bool TrackFiber { get; set; } = true;
    public short Protein { get; set; }
    public short? CustomProtein { get; set; }
    public bool TrackProtein { get; set; } = true;
    public int Sodium { get; set; }
    public short? CustomSodium { get; set; }
    public bool TrackSodium { get; set; } = true;
    public short Cholesterol { get; set; }
    public short? CustomCholesterol { get; set; }
    public bool TrackCholesterol { get; set; } = true;
    public short VitaminA { get; set; }
    public short? CustomVitaminA { get; set; }
    public bool TrackVitaminA { get; set; } = true;
    public short VitaminC { get; set; }
    public short? CustomVitaminC { get; set; }
    public bool TrackVitaminC { get; set; } = true;
    public short VitaminD { get; set; }
    public short? CustomVitaminD { get; set; }
    public bool TrackVitaminD { get; set; } = true;
    public short Calcium { get; set; }
    public short? CustomCalcium { get; set; }
    public bool TrackCalcium { get; set; } = true;
    public short Iron { get; set; }
    public short? CustomIron { get; set; }
    public bool TrackIron { get; set; } = true;
    public short Potassium { get; set; }
    public short? CustomPotassium { get; set; }
    public bool TrackPotassium { get; set; } = true;
    public short Magnesium { get; set; }
    public short? CustomMagnesium { get; set; }
    public bool TrackMagnesium { get; set; } = true;
}