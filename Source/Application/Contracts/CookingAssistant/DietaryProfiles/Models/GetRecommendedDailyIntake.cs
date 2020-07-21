using System;
using System.Linq;
using FluentValidation;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles.Models
{
    public class GetRecommendedDailyIntake
    {
        public DateTime Birthday { get; set; }
        public string Gender { get; set; }
        public short? HeightCm { get; set; }
        public short? HeightFeet { get; set; }
        public short? HeightInches { get; set; }
        public short? WeightKg { get; set; }
        public short? WeightLbs { get; set; }
        public string ActivityLevel { get; set; }
        public string Goal { get; set; }

        public short GetAge()
        {
            var now = DateTime.Now;
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
        private readonly string[] genders = new string[] { "Male", "Female" };
        private readonly string[] activityLevels = new string[] { "Sedentary", "Light", "Moderate", "Active", "VeryActive" };
        private readonly string[] goals = new string[] { "None", "MildWeightLoss", "WeightLoss", "MildWeightGain", "WeightGain" };

        public GetRecommendedDailyIntakeValidator()
        {
            RuleFor(dto => dto.Birthday)
                .NotEmpty().WithMessage("DietaryProfiles.BirthdayIsRequired")
                .Must(birthday =>
                {
                    var now = DateTime.Now;
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

            RuleFor(dto => dto.HeightCm).Must((dto, heightCm, val) =>
            {
                if (!dto.HeightFeet.HasValue && !dto.HeightInches.HasValue)
                {
                    return heightCm.HasValue;
                }
                return true;
            }).WithMessage("DietaryProfiles.HeightIsRequired");

            RuleFor(dto => dto.HeightFeet).Must((dto, heightFeet, val) =>
            {
                if (!dto.HeightCm.HasValue)
                {
                    return heightFeet.HasValue;
                }
                return true;
            }).WithMessage("DietaryProfiles.HeightIsRequired");

            RuleFor(dto => dto.WeightKg).Must((dto, weightKg, val) =>
            {
                if (!dto.WeightLbs.HasValue)
                {
                    return weightKg.HasValue;
                }
                return true;
            }).WithMessage("DietaryProfiles.WeightIsRequired");

            RuleFor(dto => dto.WeightLbs).Must((dto, weightLbs, val) =>
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
        }
    }
}
