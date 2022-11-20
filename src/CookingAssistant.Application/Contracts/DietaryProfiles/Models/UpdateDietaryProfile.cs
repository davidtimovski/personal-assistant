using FluentValidation;

namespace CookingAssistant.Application.Contracts.DietaryProfiles.Models;

public class UpdateDietaryProfile
{
    public int UserId { get; set; }
    public DateTime Birthday { get; set; }
    public string Gender { get; set; }
    public short? HeightCm { get; set; }
    public short? HeightFeet { get; set; }
    public short? HeightInches { get; set; }
    public float? WeightKg { get; set; }
    public short? WeightLbs { get; set; }
    public string ActivityLevel { get; set; }
    public string Goal { get; set; }
    public short? CustomCalories { get; set; }
    public bool TrackCalories { get; set; }
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
}

public class UpdateDietaryProfileValidator : AbstractValidator<UpdateDietaryProfile>
{
    private readonly string[] genders = new string[] { "Male", "Female" };
    private readonly string[] activityLevels = new string[] { "Sedentary", "Light", "Moderate", "Active", "VeryActive" };
    private readonly string[] goals = new string[] { "None", "MildWeightLoss", "WeightLoss", "MildWeightGain", "WeightGain" };

    public UpdateDietaryProfileValidator()
    {
        RuleFor(dto => dto.UserId).NotEmpty().WithMessage("Unauthorized");

        RuleFor(dto => dto.Birthday)
            .NotEmpty().WithMessage("DietaryProfiles.BirthdayIsRequired")
            .Must(birthday =>
            {
                var now = DateTime.UtcNow;
                short age = (short)(now.Year - birthday.Year);
                if (birthday > now.AddYears(-age))
                {
                    age--;
                }
                return age > 3 && age < 101;
            }).WithMessage("DietaryProfiles.AgeIsTooHighOrLow");

        RuleFor(dto => dto.Gender)
            .NotEmpty().WithMessage("DietaryProfiles.GenderIsRequired")
            .Must(gender => genders.Contains(gender)).WithMessage("DietaryProfiles.GenderIsInvalid");

        RuleFor(dto => dto.HeightCm).Must((dto, heightCm) =>
        {
            if (!dto.HeightFeet.HasValue && !dto.HeightInches.HasValue)
            {
                return heightCm.HasValue;
            }
            return true;
        }).WithMessage("DietaryProfiles.HeightIsRequired");

        RuleFor(dto => dto.HeightFeet).Must((dto, heightFeet) =>
        {
            if (!dto.HeightCm.HasValue)
            {
                return heightFeet.HasValue;
            }
            return true;
        }).WithMessage("DietaryProfiles.HeightIsRequired");

        RuleFor(dto => dto.WeightKg).Must((dto, weightKg) =>
        {
            if (!dto.WeightLbs.HasValue)
            {
                return weightKg.HasValue;
            }
            return true;
        }).WithMessage("DietaryProfiles.WeightIsRequired");

        RuleFor(dto => dto.WeightLbs).Must((dto, weightLbs) =>
        {
            if (!dto.WeightKg.HasValue)
            {
                return weightLbs.HasValue;
            }
            return true;
        }).WithMessage("DietaryProfiles.WeightIsRequired");

        RuleFor(dto => dto.ActivityLevel)
            .Must(activityLevel => activityLevels.Contains(activityLevel)).WithMessage("ActivityLevelIsInvalid");

        RuleFor(dto => dto.Goal)
            .Must(goal => goals.Contains(goal)).WithMessage("DietaryProfiles.GoalIsInvalid");

        RuleFor(dto => dto.CustomCalories).ExclusiveBetween((short)299, (short)10000);
        //RuleFor(dto => dto.CustomFat).ExclusiveBetween((short)299, (short)10000);
        RuleFor(dto => dto.CustomSaturatedFat).ExclusiveBetween((short)0, (short)100);
        RuleFor(dto => dto.CustomCarbohydrate).ExclusiveBetween((short)0, (short)1300);
        RuleFor(dto => dto.CustomAddedSugars).ExclusiveBetween((short)0, (short)100);
        RuleFor(dto => dto.CustomFiber).ExclusiveBetween(0, 330);
        RuleFor(dto => dto.CustomProtein).ExclusiveBetween((short)0, (short)560);
        RuleFor(dto => dto.CustomSodium).ExclusiveBetween((short)0, (short)23000);
        RuleFor(dto => dto.CustomCholesterol).ExclusiveBetween((short)0, (short)3000);
        RuleFor(dto => dto.CustomVitaminA).ExclusiveBetween((short)0, (short)9000);
        RuleFor(dto => dto.CustomVitaminC).ExclusiveBetween((short)0, (short)900);
        RuleFor(dto => dto.CustomVitaminD).ExclusiveBetween((short)0, (short)6000);
        RuleFor(dto => dto.CustomCalcium).ExclusiveBetween((short)0, (short)10000);
        RuleFor(dto => dto.CustomIron).ExclusiveBetween((short)0, (short)80);
        RuleFor(dto => dto.CustomPotassium).ExclusiveBetween((short)0, (short)32000);
        RuleFor(dto => dto.CustomMagnesium).ExclusiveBetween((short)0, (short)4000);
    }
}