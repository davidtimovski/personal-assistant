using FluentValidation;

namespace Chef.Application.Contracts.DietaryProfiles.Models;

public class UpdateDietaryProfile
{
    public required int UserId { get; init; }
    public required DateTime Birthday { get; init; }
    public required string Gender { get; init; }
    public required short? HeightCm { get; init; }
    public required short? HeightFeet { get; init; }
    public required short? HeightInches { get; init; }
    public required float? WeightKg { get; init; }
    public required short? WeightLbs { get; init; }
    public required string? ActivityLevel { get; init; }
    public required string? Goal { get; init; }
    public required short? CustomCalories { get; init; }
    public required bool TrackCalories { get; init; }
    public required short? CustomSaturatedFat { get; init; }
    public required bool TrackSaturatedFat { get; init; }
    public required short? CustomCarbohydrate { get; init; }
    public required bool TrackCarbohydrate { get; init; }
    public required short? CustomAddedSugars { get; init; }
    public required bool TrackAddedSugars { get; init; }
    public required float? CustomFiber { get; init; }
    public required bool TrackFiber { get; init; }
    public required short? CustomProtein { get; init; }
    public required bool TrackProtein { get; init; }
    public required short? CustomSodium { get; init; }
    public required bool TrackSodium { get; init; }
    public required short? CustomCholesterol { get; init; }
    public required bool TrackCholesterol { get; init; }
    public required short? CustomVitaminA { get; init; }
    public required bool TrackVitaminA { get; init; }
    public required short? CustomVitaminC { get; init; }
    public required bool TrackVitaminC { get; init; }
    public required short? CustomVitaminD { get; init; }
    public required bool TrackVitaminD { get; init; }
    public required short? CustomCalcium { get; init; }
    public required bool TrackCalcium { get; init; }
    public required short? CustomIron { get; init; }
    public required bool TrackIron { get; init; }
    public required short? CustomPotassium { get; init; }
    public required bool TrackPotassium { get; init; }
    public required short? CustomMagnesium { get; init; }
    public required bool TrackMagnesium { get; init; }
}

public class UpdateDietaryProfileValidator : AbstractValidator<UpdateDietaryProfile>
{
    private static readonly HashSet<string> Genders = new() { "Male", "Female" };
    private static readonly HashSet<string> ActivityLevels = new() { "Sedentary", "Light", "Moderate", "Active", "VeryActive" };
    private static readonly HashSet<string> Goals = new() { "None", "MildWeightLoss", "WeightLoss", "MildWeightGain", "WeightGain" };

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
            .Must(gender => Genders.Contains(gender)).WithMessage("DietaryProfiles.GenderIsInvalid");

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
            .Must(activityLevel => ActivityLevels.Contains(activityLevel)).WithMessage("ActivityLevelIsInvalid");

        RuleFor(dto => dto.Goal)
            .Must(goal => Goals.Contains(goal)).WithMessage("DietaryProfiles.GoalIsInvalid");

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