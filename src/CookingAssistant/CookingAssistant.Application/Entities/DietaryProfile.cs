using Core.Application;

namespace CookingAssistant.Application.Entities;

public class DietaryProfile : Entity
{
    public int UserId { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Gender { get; set; }
    public float? Height { get; set; }
    public float? Weight { get; set; }
    public string? ActivityLevel { get; set; }
    public string? Goal { get; set; }
    public short? CustomCalories { get; set; }
    public bool TrackCalories { get; set; }
    //public short? CustomFat { get; set; }
    //public bool TrackFat { get; set; }
    public short? CustomSaturatedFat { get; set; }
    public bool TrackSaturatedFat { get; set; }
    public short? CustomCarbohydrate { get; set; }
    public bool TrackCarbohydrate { get; set; }
    public short? CustomAddedSugars { get; set; }
    public bool TrackAddedSugars { get; set; }
    public float? CustomFiber { get; set; }
    public bool TrackFiber { get; set; }
    public short? CustomProtein { get; set; }
    public bool TrackProtein { get; set; }
    public short? CustomSodium { get; set; }
    public bool TrackSodium { get; set; }
    public short? CustomCholesterol { get; set; }
    public bool TrackCholesterol { get; set; }
    public short? CustomVitaminA { get; set; }
    public bool TrackVitaminA { get; set; }
    public short? CustomVitaminC { get; set; }
    public bool TrackVitaminC { get; set; }
    public short? CustomVitaminD { get; set; }
    public bool TrackVitaminD { get; set; }
    public short? CustomCalcium { get; set; }
    public bool TrackCalcium { get; set; }
    public short? CustomIron { get; set; }
    public bool TrackIron { get; set; }
    public short? CustomPotassium { get; set; }
    public bool TrackPotassium { get; set; }
    public short? CustomMagnesium { get; set; }
    public bool TrackMagnesium { get; set; }

    public User? User { get; set; }

    public short PersonAge
    {
        get
        {
            var now = DateTime.UtcNow;
            var age = (short)(now.Year - Birthday.Value.Year);
            if (Birthday.Value > now.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}
