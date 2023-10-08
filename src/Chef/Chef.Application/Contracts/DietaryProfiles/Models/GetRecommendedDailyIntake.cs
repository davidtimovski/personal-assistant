using FluentValidation;

namespace Chef.Application.Contracts.DietaryProfiles.Models;

public class GetRecommendedDailyIntake
{
    public required DateTime Birthday { get; init; }
    public required string? Gender { get; init; }
    public required short? HeightCm { get; init; }
    public required short? HeightFeet { get; init; }
    public required short? HeightInches { get; init; }
    public required short? WeightKg { get; init; }
    public required short? WeightLbs { get; init; }
    public required string? ActivityLevel { get; init; }
    public required string? Goal { get; init; }

    public short GetAge()
    {
        var now = DateTime.UtcNow;
        var age = (short)(now.Year - Birthday.Year);
        if (Birthday > now.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}

public class GetRecommendedDailyIntakeValidator : AbstractValidator<GetRecommendedDailyIntake>
{
    private static readonly HashSet<string> Genders = new () { "Male", "Female" };
    private static readonly HashSet<string> ActivityLevels = new() { "Sedentary", "Light", "Moderate", "Active", "VeryActive" };
    private static readonly HashSet<string> Goals = new() { "None", "MildWeightLoss", "WeightLoss", "MildWeightGain", "WeightGain" };

    public GetRecommendedDailyIntakeValidator()
    {
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
            .Must(gender => gender is null || Genders.Contains(gender)).WithMessage("DietaryProfiles.GenderIsInvalid");

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
            .Must(activityLevel => activityLevel is null || ActivityLevels.Contains(activityLevel)).WithMessage("ActivityLevelIsInvalid");

        RuleFor(dto => dto.Goal)
            .Must(goal => goal is null || Goals.Contains(goal)).WithMessage("DietaryProfiles.GoalIsInvalid");
    }
}
