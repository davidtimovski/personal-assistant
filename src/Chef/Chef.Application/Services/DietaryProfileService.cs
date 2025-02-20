﻿using AutoMapper;
using Chef.Application.Contracts.Common;
using Chef.Application.Contracts.DietaryProfiles;
using Chef.Application.Contracts.DietaryProfiles.Models;
using Chef.Application.Contracts.Recipes.Models;
using Chef.Application.Entities;
using Chef.Utility;
using Core.Application.Utils;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chef.Application.Services;

public class DietaryProfileService : IDietaryProfileService
{
    private readonly IConversion _conversion;
    private readonly IDailyIntakeHelper _dailyIntakeHelper;
    private readonly DailyIntakeReference _dailyIntakeRef;
    private readonly IDietaryProfilesRepository _dietaryProfilesRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<DietaryProfileService> _logger;

    public DietaryProfileService(
        IConversion? conversion,
        IDailyIntakeHelper? dailyIntakeHelper,
        IOptions<DailyIntakeReference>? dailyIntakeRef,
        IDietaryProfilesRepository? dietaryProfilesRepository,
        IMapper? mapper,
        ILogger<DietaryProfileService>? logger)
    {
        _conversion = ArgValidator.NotNull(conversion);
        _dailyIntakeHelper = ArgValidator.NotNull(dailyIntakeHelper);
        _dailyIntakeRef = ArgValidator.NotNull(dailyIntakeRef).Value;
        _dietaryProfilesRepository = ArgValidator.NotNull(dietaryProfilesRepository);
        _mapper = ArgValidator.NotNull(mapper);
        _logger = ArgValidator.NotNull(logger);
    }

    public EditDietaryProfile? Get(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(DietaryProfileService)}.{nameof(Get)}");

        try
        {
            var profile = _dietaryProfilesRepository.Get(userId, metric);
            if (profile is null)
            {
                return null;
            }

            var result = _mapper.Map<EditDietaryProfile>(profile);
            result.DailyIntake = CalculateDailyIntake(profile);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Get)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public RecommendedDailyIntake GetRecommendedDailyIntake(GetRecommendedDailyIntake model, IValidator<GetRecommendedDailyIntake> validator, ISpan metricsSpan)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        var metric = metricsSpan.StartChild($"{nameof(DietaryProfileService)}.{nameof(GetRecommendedDailyIntake)}");

        try
        {
            List<DailyIntakeAgeGroup> intakeByGender = model.Gender == "Male" ? _dailyIntakeRef.Male : _dailyIntakeRef.Female;
            short age = model.GetAge();
            RecommendedIntake intake = intakeByGender.First(x => age >= x.AgeFrom && age <= x.AgeTo).RecommendedIntake;

            // Find height
            float height = 0;
            if (model.HeightFeet.HasValue && !model.HeightCm.HasValue)
            {
                short heightInchesValue = model.HeightInches ?? 0;
                height = _conversion.FeetAndInchesToCentimeters(model.HeightFeet.Value, heightInchesValue);
            }
            else
            {
                height = (float)Math.Floor((double)model.HeightCm!.Value);
            }

            // Find weight
            float weight = 0;
            if (model.WeightLbs.HasValue && !model.WeightKg.HasValue)
            {
                weight = _conversion.PoundsToKilos(model.WeightLbs.Value);
            }
            else
            {
                weight = (float)Math.Floor((double)model.WeightKg!.Value);
            }

            var recommendedDailyIntake = new RecommendedDailyIntake
            {
                Calories = _dailyIntakeHelper.DeriveDailyCaloriesIntake(model.GetAge(), model.Gender!,
                    height, weight, model.ActivityLevel!, model.Goal!),
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
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetRecommendedDailyIntake)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public RecipeNutritionSummary CalculateNutritionSummary(Recipe recipe, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(DietaryProfileService)}.{nameof(CalculateNutritionSummary)}");

        try
        {
            RecipeIngredient[] validRecipeIngredients = recipe.RecipeIngredients
            .Where(x => x.Amount.HasValue
                        && (x.Ingredient!.ServingSizeIsOneUnit && x.Unit is null || !x.Ingredient.ServingSizeIsOneUnit && x.Unit is not null))
            .ToArray();

            var nutritionSummary = new RecipeNutritionSummary();

            foreach (var recipeIngredient in validRecipeIngredients)
            {
                short servingSize = recipeIngredient.Ingredient.ServingSize;
                bool servingSizeIsOneUnit = recipeIngredient.Ingredient.ServingSizeIsOneUnit;
                float amount = recipeIngredient.Amount.Value;
                string? unit = recipeIngredient.Unit;
                byte servings = recipe.Servings;

                if (recipeIngredient.Ingredient.Calories.HasValue)
                {
                    nutritionSummary.Calories = AddGramsValuePerAmountAndServing(nutritionSummary.Calories, recipeIngredient.Ingredient.Calories.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Fat.HasValue)
                {
                    nutritionSummary.Fat = AddGramsValuePerAmountAndServing(nutritionSummary.Fat, recipeIngredient.Ingredient.Fat.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.SaturatedFat.HasValue)
                {
                    nutritionSummary.SaturatedFat = AddGramsValuePerAmountAndServing(nutritionSummary.SaturatedFat, recipeIngredient.Ingredient.SaturatedFat.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Carbohydrate.HasValue)
                {
                    nutritionSummary.Carbohydrate = AddGramsValuePerAmountAndServing(nutritionSummary.Carbohydrate, recipeIngredient.Ingredient.Carbohydrate.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Sugars.HasValue)
                {
                    nutritionSummary.Sugars = AddGramsValuePerAmountAndServing(nutritionSummary.Sugars, recipeIngredient.Ingredient.Sugars.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.AddedSugars.HasValue)
                {
                    nutritionSummary.AddedSugars = AddGramsValuePerAmountAndServing(nutritionSummary.AddedSugars, recipeIngredient.Ingredient.AddedSugars.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Fiber.HasValue)
                {
                    nutritionSummary.Fiber = AddGramsValuePerAmountAndServing(nutritionSummary.Fiber, recipeIngredient.Ingredient.Fiber.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Protein.HasValue)
                {
                    nutritionSummary.Protein = AddGramsValuePerAmountAndServing(nutritionSummary.Protein, recipeIngredient.Ingredient.Protein.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Sodium.HasValue)
                {
                    nutritionSummary.Sodium = AddMilligramsValuePerAmountAndServing(nutritionSummary.Sodium, recipeIngredient.Ingredient.Sodium.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Cholesterol.HasValue)
                {
                    nutritionSummary.Cholesterol = AddMilligramsValuePerAmountAndServing(nutritionSummary.Cholesterol, recipeIngredient.Ingredient.Cholesterol.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.VitaminA.HasValue)
                {
                    nutritionSummary.VitaminA = AddMilligramsValuePerAmountAndServing(nutritionSummary.VitaminA, recipeIngredient.Ingredient.VitaminA.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.VitaminC.HasValue)
                {
                    nutritionSummary.VitaminC = AddMilligramsValuePerAmountAndServing(nutritionSummary.VitaminC, recipeIngredient.Ingredient.VitaminC.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.VitaminD.HasValue)
                {
                    nutritionSummary.VitaminD = AddMilligramsValuePerAmountAndServing(nutritionSummary.VitaminD, recipeIngredient.Ingredient.VitaminD.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Calcium.HasValue)
                {
                    nutritionSummary.Calcium = AddMilligramsValuePerAmountAndServing(nutritionSummary.Calcium, recipeIngredient.Ingredient.Calcium.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Iron.HasValue)
                {
                    nutritionSummary.Iron = AddMilligramsValuePerAmountAndServing(nutritionSummary.Iron, recipeIngredient.Ingredient.Iron.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Potassium.HasValue)
                {
                    nutritionSummary.Potassium = AddMilligramsValuePerAmountAndServing(nutritionSummary.Potassium, recipeIngredient.Ingredient.Potassium.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Magnesium.HasValue)
                {
                    nutritionSummary.Magnesium = AddMilligramsValuePerAmountAndServing(nutritionSummary.Magnesium, recipeIngredient.Ingredient.Magnesium.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
            }

            RecommendedIntake? intake = null;
            DietaryProfile? dietaryProfile = recipe.User?.DietaryProfile;

            if (dietaryProfile is not null)
            {
                List<DailyIntakeAgeGroup> intakeByGender = dietaryProfile.Gender == "Male" ? _dailyIntakeRef.Male : _dailyIntakeRef.Female;
                intake = intakeByGender.First(x => dietaryProfile.PersonAge >= x.AgeFrom && dietaryProfile.PersonAge <= x.AgeTo).RecommendedIntake;
            }

            if (nutritionSummary.Calories.HasValue)
            {
                if (dietaryProfile is not null && intake is not null && dietaryProfile.TrackCalories)
                {
                    short dailyCalories = dietaryProfile.CustomCalories ?? _dailyIntakeHelper.DeriveDailyCaloriesIntake(dietaryProfile.PersonAge, dietaryProfile.Gender!,
                        dietaryProfile.Height!.Value, dietaryProfile.Weight!.Value, dietaryProfile.ActivityLevel!, dietaryProfile.Goal!);
                    nutritionSummary.CaloriesFromDaily = GetPercentageFromRecommendedDailyIntake(dailyCalories, nutritionSummary.Calories.Value);
                    nutritionSummary.CaloriesFromDailyGrade = GetDailyGrade(nutritionSummary.CaloriesFromDaily.Value, false);
                }

                nutritionSummary.Calories = (float)Math.Ceiling((double)nutritionSummary.Calories);
            }
            if (nutritionSummary.Fat.HasValue)
            {
                nutritionSummary.Fat = (float)Math.Round((double)nutritionSummary.Fat, 2, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.SaturatedFat.HasValue)
            {
                if (dietaryProfile is not null && intake is not null && dietaryProfile.TrackSaturatedFat)
                {
                    short dailySaturatedFat = dietaryProfile.CustomSaturatedFat ?? intake.SaturatedFatMax;
                    nutritionSummary.SaturatedFatFromDaily = GetPercentageFromRecommendedDailyIntake(dailySaturatedFat, nutritionSummary.SaturatedFat.Value);
                    nutritionSummary.SaturatedFatFromDailyGrade = GetDailyGrade(nutritionSummary.SaturatedFatFromDaily.Value, false);
                }

                nutritionSummary.SaturatedFat = (float)Math.Round((double)nutritionSummary.SaturatedFat, 2, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.Carbohydrate.HasValue)
            {
                if (dietaryProfile is not null && intake is not null && dietaryProfile.TrackCarbohydrate)
                {
                    short dailyCarbohydrate = dietaryProfile.CustomCarbohydrate ?? intake.Carbohydrate;
                    nutritionSummary.CarbohydrateFromDaily = GetPercentageFromRecommendedDailyIntake(dailyCarbohydrate, nutritionSummary.Carbohydrate.Value);
                    nutritionSummary.CarbohydrateFromDailyGrade = GetDailyGrade(nutritionSummary.CarbohydrateFromDaily.Value, false);
                }

                nutritionSummary.Carbohydrate = (float)Math.Round((double)nutritionSummary.Carbohydrate, 2, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.Sugars.HasValue)
            {
                nutritionSummary.Sugars = (float)Math.Round((double)nutritionSummary.Sugars, 2, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.AddedSugars.HasValue)
            {
                if (dietaryProfile is not null && intake is not null && dietaryProfile.TrackAddedSugars)
                {
                    short dailyAddedSugars = dietaryProfile.CustomAddedSugars ?? intake.AddedSugarsMax;
                    nutritionSummary.AddedSugarsFromDaily = GetPercentageFromRecommendedDailyIntake(dailyAddedSugars, nutritionSummary.AddedSugars.Value);
                    nutritionSummary.AddedSugarsFromDailyGrade = GetDailyGrade(nutritionSummary.AddedSugarsFromDaily.Value, false);
                }

                nutritionSummary.AddedSugars = (float)Math.Round((double)nutritionSummary.AddedSugars, 2, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.Fiber.HasValue)
            {
                if (dietaryProfile is not null && intake is not null && dietaryProfile.TrackFiber)
                {
                    float dailyFiber = dietaryProfile.CustomFiber ?? intake.Fiber;
                    nutritionSummary.FiberFromDaily = GetPercentageFromRecommendedDailyIntake(dailyFiber, nutritionSummary.Fiber.Value);
                    nutritionSummary.FiberFromDailyGrade = GetDailyGrade(nutritionSummary.FiberFromDaily.Value, true);
                }

                nutritionSummary.Fiber = (float)Math.Round((double)nutritionSummary.Fiber, 2, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.Protein.HasValue)
            {
                if (dietaryProfile is not null && intake is not null && dietaryProfile.TrackProtein)
                {
                    short dailyProtein = dietaryProfile.CustomProtein ?? intake.Protein;
                    nutritionSummary.ProteinFromDaily = GetPercentageFromRecommendedDailyIntake(dailyProtein, nutritionSummary.Protein.Value);
                    nutritionSummary.ProteinFromDailyGrade = GetDailyGrade(nutritionSummary.ProteinFromDaily.Value, true);
                }

                nutritionSummary.Protein = (float)Math.Round((double)nutritionSummary.Protein, 2, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.Sodium.HasValue)
            {
                if (dietaryProfile is not null && intake is not null && dietaryProfile.TrackSodium)
                {
                    short dailySodium = dietaryProfile.CustomSodium ?? intake.Sodium;
                    nutritionSummary.SodiumFromDaily = GetPercentageFromRecommendedDailyIntake(dailySodium, nutritionSummary.Sodium.Value);
                    nutritionSummary.SodiumFromDailyGrade = GetDailyGrade(nutritionSummary.SodiumFromDaily.Value, false);
                }

                nutritionSummary.Sodium = (float)Math.Round((double)nutritionSummary.Sodium, 1, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.Cholesterol.HasValue)
            {
                if (dietaryProfile is not null && intake is not null && dietaryProfile.TrackCholesterol)
                {
                    short dailyCholesterol = dietaryProfile.CustomCholesterol ?? intake.CholesterolMax;
                    nutritionSummary.CholesterolFromDaily = GetPercentageFromRecommendedDailyIntake(dailyCholesterol, nutritionSummary.Cholesterol.Value);
                    nutritionSummary.CholesterolFromDailyGrade = GetDailyGrade(nutritionSummary.CholesterolFromDaily.Value, false);
                }

                nutritionSummary.Cholesterol = (float)Math.Round((double)nutritionSummary.Cholesterol, 1, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.VitaminA.HasValue)
            {
                if (dietaryProfile is not null && intake is not null && dietaryProfile.TrackVitaminA)
                {
                    short dailyVitaminA = dietaryProfile.CustomVitaminA ?? intake.VitaminA;
                    nutritionSummary.VitaminAFromDaily = GetPercentageFromRecommendedDailyIntake(dailyVitaminA, nutritionSummary.VitaminA.Value);
                    nutritionSummary.VitaminAFromDailyGrade = GetDailyGrade(nutritionSummary.VitaminAFromDaily.Value, true);
                }

                nutritionSummary.VitaminA = (float)Math.Round((double)nutritionSummary.VitaminA, 1, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.VitaminC.HasValue)
            {
                if (dietaryProfile is not null && intake is not null && dietaryProfile.TrackVitaminC)
                {
                    short dailyVitaminC = dietaryProfile.CustomVitaminC ?? intake.VitaminC;
                    nutritionSummary.VitaminCFromDaily = GetPercentageFromRecommendedDailyIntake(dailyVitaminC, nutritionSummary.VitaminC.Value);
                    nutritionSummary.VitaminCFromDailyGrade = GetDailyGrade(nutritionSummary.VitaminCFromDaily.Value, true);
                }

                nutritionSummary.VitaminC = (float)Math.Round((double)nutritionSummary.VitaminC, 1, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.VitaminD.HasValue)
            {
                if (dietaryProfile is not null && intake is not null && dietaryProfile.TrackVitaminD)
                {
                    short dailyVitaminD = dietaryProfile.CustomVitaminD ?? intake.VitaminD;
                    nutritionSummary.VitaminDFromDaily = GetPercentageFromRecommendedDailyIntake(dailyVitaminD, nutritionSummary.VitaminD.Value);
                    nutritionSummary.VitaminDFromDailyGrade = GetDailyGrade(nutritionSummary.VitaminDFromDaily.Value, true);
                }

                nutritionSummary.VitaminD = (float)Math.Round((double)nutritionSummary.VitaminD, 1, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.Calcium.HasValue)
            {
                if (dietaryProfile is not null && intake is not null && dietaryProfile.TrackCalcium)
                {
                    short dailyCalcium = dietaryProfile.CustomCalcium ?? intake.Calcium;
                    nutritionSummary.CalciumFromDaily = GetPercentageFromRecommendedDailyIntake(dailyCalcium, nutritionSummary.Calcium.Value);
                    nutritionSummary.CalciumFromDailyGrade = GetDailyGrade(nutritionSummary.CalciumFromDaily.Value, true);
                }

                nutritionSummary.Calcium = (float)Math.Round((double)nutritionSummary.Calcium, 1, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.Iron.HasValue)
            {
                if (dietaryProfile is not null && intake is not null && dietaryProfile.TrackIron)
                {
                    short dailyIron = dietaryProfile.CustomIron ?? intake.Iron;
                    nutritionSummary.IronFromDaily = GetPercentageFromRecommendedDailyIntake(dailyIron, nutritionSummary.Iron.Value);
                    nutritionSummary.IronFromDailyGrade = GetDailyGrade(nutritionSummary.IronFromDaily.Value, true);
                }

                nutritionSummary.Iron = (float)Math.Round((double)nutritionSummary.Iron, 1, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.Potassium.HasValue)
            {
                if (dietaryProfile is not null && intake is not null && dietaryProfile.TrackPotassium)
                {
                    short dailyPotassium = dietaryProfile.CustomPotassium ?? intake.Potassium;
                    nutritionSummary.PotassiumFromDaily = GetPercentageFromRecommendedDailyIntake(dailyPotassium, nutritionSummary.Potassium.Value);
                    nutritionSummary.PotassiumFromDailyGrade = GetDailyGrade(nutritionSummary.PotassiumFromDaily.Value, true);
                }

                nutritionSummary.Potassium = (float)Math.Round((double)nutritionSummary.Potassium, 1, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.Magnesium.HasValue)
            {
                if (dietaryProfile is not null && intake is not null && dietaryProfile.TrackMagnesium)
                {
                    short dailyMagnesium = dietaryProfile.CustomMagnesium ?? intake.Magnesium;
                    nutritionSummary.MagnesiumFromDaily = GetPercentageFromRecommendedDailyIntake(dailyMagnesium, nutritionSummary.Magnesium.Value);
                    nutritionSummary.MagnesiumFromDailyGrade = GetDailyGrade(nutritionSummary.MagnesiumFromDaily.Value, true);
                }

                nutritionSummary.Magnesium = (float)Math.Round((double)nutritionSummary.Magnesium, 1, MidpointRounding.AwayFromZero);
            }

            nutritionSummary.IsSet = nutritionSummary.Calories.HasValue
                                     || nutritionSummary.Fat.HasValue
                                     || nutritionSummary.SaturatedFat.HasValue
                                     || nutritionSummary.Carbohydrate.HasValue
                                     || nutritionSummary.Sugars.HasValue
                                     || nutritionSummary.AddedSugars.HasValue
                                     || nutritionSummary.Fiber.HasValue
                                     || nutritionSummary.Protein.HasValue
                                     || nutritionSummary.Sodium.HasValue
                                     || nutritionSummary.Cholesterol.HasValue
                                     || nutritionSummary.VitaminA.HasValue
                                     || nutritionSummary.VitaminC.HasValue
                                     || nutritionSummary.VitaminD.HasValue
                                     || nutritionSummary.Calcium.HasValue
                                     || nutritionSummary.Iron.HasValue
                                     || nutritionSummary.Potassium.HasValue
                                     || nutritionSummary.Magnesium.HasValue;

            return nutritionSummary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CalculateNutritionSummary)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task CreateOrUpdateAsync(UpdateDietaryProfile model, IValidator<UpdateDietaryProfile> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        var metric = metricsSpan.StartChild($"{nameof(DietaryProfileService)}.{nameof(CreateOrUpdateAsync)}");

        try
        {
            var dietaryProfile = _mapper.Map<DietaryProfile>(model);

            // Derive height
            if (model.HeightFeet.HasValue && !model.HeightCm.HasValue)
            {
                short heightInchesValue = model.HeightInches ?? 0;
                dietaryProfile.Height = _conversion.FeetAndInchesToCentimeters(model.HeightFeet.Value, heightInchesValue);
            }
            else
            {
                dietaryProfile.Height = (float)Math.Floor((double)model.HeightCm!.Value);
            }

            // Derive weight
            if (model.WeightLbs.HasValue && !model.WeightKg.HasValue)
            {
                dietaryProfile.Weight = _conversion.PoundsToKilos(model.WeightLbs.Value);
            }
            else
            {
                dietaryProfile.Weight = (float)Math.Round(model.WeightKg!.Value, 1);
            }

            await _dietaryProfilesRepository.CreateOrUpdateAsync(dietaryProfile, metric, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateOrUpdateAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task DeleteAsync(int userId, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(DietaryProfileService)}.{nameof(DeleteAsync)}");

        try
        {
            await _dietaryProfilesRepository.DeleteAsync(userId, metric, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    private DailyIntake CalculateDailyIntake(DietaryProfile profile)
    {
        try
        {
            List<DailyIntakeAgeGroup> intakeByGender = profile.Gender == "Male" ? _dailyIntakeRef.Male : _dailyIntakeRef.Female;
            RecommendedIntake intake = intakeByGender.First(x => profile.PersonAge >= x.AgeFrom && profile.PersonAge <= x.AgeTo).RecommendedIntake;

            var dailyIntake = new DailyIntake
            {
                Calories = _dailyIntakeHelper.DeriveDailyCaloriesIntake(profile.PersonAge, profile.Gender!, profile.Height!.Value,
                    profile.Weight!.Value, profile.ActivityLevel!, profile.Goal!),
                CustomCalories = profile.CustomCalories,
                TrackCalories = profile.TrackCalories,
                //Fat = intake.Fat,
                //CustomFat = source.CustomFat,
                //TrackFat = source.TrackFat,
                SaturatedFat = intake.SaturatedFatMax,
                CustomSaturatedFat = profile.CustomSaturatedFat,
                TrackSaturatedFat = profile.TrackSaturatedFat,
                Carbohydrate = intake.Carbohydrate,
                CustomCarbohydrate = profile.CustomCarbohydrate,
                TrackCarbohydrate = profile.TrackCarbohydrate,
                AddedSugars = intake.AddedSugarsMax,
                CustomAddedSugars = profile.CustomAddedSugars,
                TrackAddedSugars = profile.TrackAddedSugars,
                Fiber = intake.Fiber,
                CustomFiber = profile.CustomFiber,
                TrackFiber = profile.TrackFiber,
                Protein = intake.Protein,
                CustomProtein = profile.CustomProtein,
                TrackProtein = profile.TrackProtein,
                Sodium = intake.Sodium,
                CustomSodium = profile.CustomSodium,
                TrackSodium = profile.TrackSodium,
                Cholesterol = intake.CholesterolMax,
                CustomCholesterol = profile.CustomCholesterol,
                TrackCholesterol = profile.TrackCholesterol,
                VitaminA = intake.VitaminA,
                CustomVitaminA = profile.CustomVitaminA,
                TrackVitaminA = profile.TrackVitaminA,
                VitaminC = intake.VitaminC,
                CustomVitaminC = profile.CustomVitaminC,
                TrackVitaminC = profile.TrackVitaminC,
                VitaminD = intake.VitaminD,
                CustomVitaminD = profile.CustomVitaminD,
                TrackVitaminD = profile.TrackVitaminD,
                Calcium = intake.Calcium,
                CustomCalcium = profile.CustomCalcium,
                TrackCalcium = profile.TrackCalcium,
                Iron = intake.Iron,
                CustomIron = profile.CustomIron,
                TrackIron = profile.TrackIron,
                Potassium = intake.Potassium,
                CustomPotassium = profile.CustomPotassium,
                TrackPotassium = profile.TrackPotassium,
                Magnesium = intake.Magnesium,
                CustomMagnesium = profile.CustomMagnesium,
                TrackMagnesium = profile.TrackMagnesium
            };

            return dailyIntake;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CalculateDailyIntake)}");
            throw;
        }
    }

    private float? AddGramsValuePerAmountAndServing(float? currentValue, float valueInGrams, short servingSizeGrams, bool servingSizeIsOneUnit, float amount, string? unit, byte servings)
    {
        if (servingSizeIsOneUnit)
        {
            return valueInGrams * amount / servings + (currentValue ?? 0);
        }

        float amountInGrams = _conversion.ToGrams(unit, amount);
        float valuePerGram = valueInGrams / servingSizeGrams;
        return valuePerGram * amountInGrams / servings + (currentValue ?? 0);
    }

    private float? AddMilligramsValuePerAmountAndServing(float? currentValue, float valueInMilligrams, short servingSizeGrams, bool servingSizeIsOneUnit, float amount, string? unit, byte servings)
    {
        if (servingSizeIsOneUnit)
        {
            return valueInMilligrams * amount / servings + (currentValue ?? 0);
        }

        float amountInMilligrams = _conversion.ToMilligrams(unit, amount);
        float valuePerMilligram = valueInMilligrams / (servingSizeGrams * 1000);
        return valuePerMilligram * amountInMilligrams / servings + (currentValue ?? 0);
    }

    private static float GetPercentageFromRecommendedDailyIntake(float recommended, float actual)
    {
        float result = (float)Math.Round(actual / recommended * 100, MidpointRounding.AwayFromZero);
        return result < 100 ? result : 100f;
    }

    private static string GetDailyGrade(float percentageAmount, bool isBeneficial)
    {
        if (isBeneficial)
        {
            if (percentageAmount < 15)
            {
                return "bad";
            }
            if (percentageAmount < 30)
            {
                return "average";
            }

            return "good";
        }
        else
        {
            if (percentageAmount < 25)
            {
                return "good";
            }
            if (percentageAmount < 50)
            {
                return "average";
            }
            if (percentageAmount < 75)
            {
                return "bad";
            }

            return "terrible";
        }
    }
}
