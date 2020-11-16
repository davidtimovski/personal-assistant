using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;
using PersonalAssistant.Application.Contracts.CookingAssistant.Common;
using PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles;
using PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles.Models;
using PersonalAssistant.Domain.Entities.CookingAssistant;
using Utility;

namespace PersonalAssistant.Application.Services.CookingAssistant
{
    public class DietaryProfileService : IDietaryProfileService
    {
        private readonly IConversion _conversion;
        private readonly IDailyIntakeHelper _dailyIntakeHelper;
        private readonly DailyIntakeReference _dailyIntakeRef;
        private readonly IDietaryProfilesRepository _dietaryProfilesRepository;
        private readonly IMapper _mapper;

        public DietaryProfileService(
            IConversion conversion,
            IDailyIntakeHelper dailyIntakeHelper,
            IOptions<DailyIntakeReference> dailyIntakeRef,
            IDietaryProfilesRepository dietaryProfilesRepository,
            IMapper mapper)
        {
            _conversion = conversion;
            _dailyIntakeHelper = dailyIntakeHelper;
            _dailyIntakeRef = dailyIntakeRef.Value;
            _dietaryProfilesRepository = dietaryProfilesRepository;
            _mapper = mapper;
        }

        public async Task<EditDietaryProfile> GetAsync(int userId)
        {
            DietaryProfile profile = await _dietaryProfilesRepository.GetAsync(userId);

            var result = _mapper.Map<EditDietaryProfile>(profile);

            return result;
        }

        public RecommendedDailyIntake GetRecommendedDailyIntake(GetRecommendedDailyIntake model, IValidator<GetRecommendedDailyIntake> validator)
        {
            ValidateAndThrow(model, validator);

            IEnumerable<DailyIntakeAgeGroup> intakeByGender = model.Gender == "Male" ? _dailyIntakeRef.Male : _dailyIntakeRef.Female;
            short age = model.GetAge();
            RecommendedIntake intake = intakeByGender.FirstOrDefault(x => age >= x.AgeFrom && age <= x.AgeTo).RecommendedIntake;

            // Find height
            float height = 0;
            if (model.HeightFeet.HasValue && !model.HeightCm.HasValue)
            {
                short heightInchesValue = model.HeightInches ?? 0;
                height = _conversion.FeetAndInchesToCentimeters(model.HeightFeet.Value, heightInchesValue);
            }
            else
            {
                height = (float)Math.Floor((double)model.HeightCm.Value);
            }

            // Find weight
            float weight = 0;
            if (model.WeightLbs.HasValue && !model.WeightKg.HasValue)
            {
                weight = _conversion.PoundsToKilos(model.WeightLbs.Value);
            }
            else
            {
                weight = (float)Math.Floor((double)model.WeightKg.Value);
            }

            var recommendedDailyIntake = new RecommendedDailyIntake
            {
                Calories = _dailyIntakeHelper.DeriveDailyCaloriesIntake(model.GetAge(), model.Gender,
                    height, weight, model.ActivityLevel, model.Goal),
                //Fat = intake.Fat,
                SaturatedFat = intake.SaturatedFatMax,
                Carbohydrate = intake.Carbohydrate,
                AddedSugars = intake.AddedSugarsMax,
                Fiber = intake.Fiber,
                Protein = intake.Protein,
                Sodium = intake.Sodium,
                Cholesterol = intake.CholesterolMax,
                VitaminA = intake.VitaminA,
                VitaminC = intake.VitaminC,
                VitaminD = intake.VitaminD,
                Calcium = intake.Calcium,
                Iron = intake.Iron,
                Potassium = intake.Potassium,
                Magnesium = intake.Magnesium
            };

            return recommendedDailyIntake;
        }

        public async Task UpdateAsync(UpdateDietaryProfile model, IValidator<UpdateDietaryProfile> validator)
        {
            ValidateAndThrow(model, validator);

            var dietaryProfile = _mapper.Map<DietaryProfile>(model);

            // Derive height
            if (model.HeightFeet.HasValue && !model.HeightCm.HasValue)
            {
                short heightInchesValue = model.HeightInches ?? 0;
                dietaryProfile.Height = _conversion.FeetAndInchesToCentimeters(model.HeightFeet.Value, heightInchesValue);
            }
            else
            {
                dietaryProfile.Height = (float)Math.Floor((double)model.HeightCm.Value);
            }

            // Derive weight
            if (model.WeightLbs.HasValue && !model.WeightKg.HasValue)
            {
                dietaryProfile.Weight = _conversion.PoundsToKilos(model.WeightLbs.Value);
            }
            else
            {
                dietaryProfile.Weight = (float)Math.Round(model.WeightKg.Value, 1);
            }

            await _dietaryProfilesRepository.UpdateAsync(dietaryProfile);
        }

        public async Task DeleteAsync(int userId)
        {
            await _dietaryProfilesRepository.DeleteAsync(userId);
        }

        private void ValidateAndThrow<T>(T model, IValidator<T> validator)
        {
            ValidationResult result = validator.Validate(model);
            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }
    }
}
