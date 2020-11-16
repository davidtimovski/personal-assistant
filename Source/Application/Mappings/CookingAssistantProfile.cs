using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Options;
using PersonalAssistant.Application.Contracts;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.Common.Models;
using PersonalAssistant.Application.Contracts.CookingAssistant;
using PersonalAssistant.Application.Contracts.CookingAssistant.Common;
using PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles.Models;
using PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients.Models;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models;
using PersonalAssistant.Domain.Entities;
using PersonalAssistant.Domain.Entities.Common;
using PersonalAssistant.Domain.Entities.CookingAssistant;
using Utility;

namespace PersonalAssistant.Application.Mappings
{
    public class CookingAssistantProfile : Profile
    {
        public CookingAssistantProfile()
        {
            CreateMap<CreateRecipe, Recipe>()
                .ForMember(x => x.Id, src => src.Ignore())
                .ForMember(x => x.LastOpenedDate, src => src.Ignore())
                .ForMember(x => x.CreatedDate, src => src.Ignore())
                .ForMember(x => x.ModifiedDate, src => src.Ignore())
                .ForMember(x => x.User, src => src.Ignore())
                .ForMember(x => x.RecipeIngredients, opt => opt.MapFrom(src => src.Ingredients))
                .ForMember(x => x.IngredientsMissing, src => src.Ignore());
            CreateMap<UpdateRecipe, Recipe>()
                .ForMember(x => x.LastOpenedDate, src => src.Ignore())
                .ForMember(x => x.CreatedDate, src => src.Ignore())
                .ForMember(x => x.ModifiedDate, src => src.Ignore())
                .ForMember(x => x.User, src => src.Ignore())
                .ForMember(x => x.RecipeIngredients, opt => opt.MapFrom(src => src.Ingredients))
                .ForMember(x => x.IngredientsMissing, src => src.Ignore());
            CreateMap<UpdateRecipeIngredient, RecipeIngredient>()
                .ForMember(x => x.RecipeId, src => src.Ignore())
                .ForMember(x => x.IngredientId, src => src.Ignore())
                .ForPath(x => x.Ingredient.TaskId, opt => opt.MapFrom(src => src.TaskId))
                .ForPath(x => x.Ingredient.Name, opt => opt.MapFrom(src => src.Name))
                .ForPath(x => x.Unit, opt => opt.MapFrom(src => src.Unit))
                .ForMember(x => x.CreatedDate, src => src.Ignore())
                .ForMember(x => x.ModifiedDate, src => src.Ignore())
                .ForMember(x => x.Recipe, src => src.Ignore());

            CreateMap<UpdateIngredient, Ingredient>()
                .ForMember(x => x.ServingSize, opt => opt.MapFrom(src => src.NutritionData.ServingSize))
                .ForMember(x => x.ServingSizeIsOneUnit, opt => opt.MapFrom(src => src.NutritionData.ServingSizeIsOneUnit))
                .ForMember(x => x.Calories, opt => opt.MapFrom(src => src.NutritionData.Calories))
                .ForMember(x => x.Fat, opt => opt.MapFrom(src => src.NutritionData.Fat))
                .ForMember(x => x.SaturatedFat, opt => opt.MapFrom(src => src.NutritionData.SaturatedFat))
                .ForMember(x => x.TransFat, opt => opt.MapFrom(src => src.NutritionData.TransFat))
                .ForMember(x => x.Carbohydrate, opt => opt.MapFrom(src => src.NutritionData.Carbohydrate))
                .ForMember(x => x.Sugars, opt => opt.MapFrom(src => src.NutritionData.Sugars))
                .ForMember(x => x.AddedSugars, opt => opt.MapFrom(src => src.NutritionData.AddedSugars))
                .ForMember(x => x.Fiber, opt => opt.MapFrom(src => src.NutritionData.Fiber))
                .ForMember(x => x.Protein, opt => opt.MapFrom(src => src.NutritionData.Protein))
                .ForMember(x => x.Sodium, opt => opt.MapFrom(src => src.NutritionData.Sodium))
                .ForMember(x => x.Cholesterol, opt => opt.MapFrom(src => src.NutritionData.Cholesterol))
                .ForMember(x => x.VitaminA, opt => opt.MapFrom(src => src.NutritionData.VitaminA))
                .ForMember(x => x.VitaminC, opt => opt.MapFrom(src => src.NutritionData.VitaminC))
                .ForMember(x => x.VitaminD, opt => opt.MapFrom(src => src.NutritionData.VitaminD))
                .ForMember(x => x.Calcium, opt => opt.MapFrom(src => src.NutritionData.Calcium))
                .ForMember(x => x.Iron, opt => opt.MapFrom(src => src.NutritionData.Iron))
                .ForMember(x => x.Potassium, opt => opt.MapFrom(src => src.NutritionData.Potassium))
                .ForMember(x => x.Magnesium, opt => opt.MapFrom(src => src.NutritionData.Magnesium))
                .ForMember(x => x.ProductSize, opt => opt.MapFrom(src => src.PriceData.ProductSize))
                .ForMember(x => x.ProductSizeIsOneUnit, opt => opt.MapFrom(src => src.PriceData.ProductSizeIsOneUnit))
                .ForMember(x => x.Price, opt => opt.MapFrom(src => src.PriceData.Price))
                .ForMember(x => x.Currency, opt => opt.MapFrom(src => src.PriceData.Currency))
                .ForMember(x => x.CreatedDate, src => src.Ignore())
                .ForMember(x => x.ModifiedDate, src => src.Ignore())
                .ForMember(x => x.Recipes, src => src.Ignore())
                .ForMember(x => x.Task, src => src.Ignore());

            CreateMap<UpdateDietaryProfile, DietaryProfile>()
                .ForMember(x => x.Height, src => src.Ignore())
                .ForMember(x => x.Weight, src => src.Ignore())
                .ForMember(x => x.CreatedDate, src => src.Ignore())
                .ForMember(x => x.ModifiedDate, src => src.Ignore())
                .ForMember(x => x.User, src => src.Ignore());

            CreateMap<User, CookingAssistantPreferences>()
                .ForMember(x => x.NotificationsEnabled, opt => opt.MapFrom(src => src.CookingNotificationsEnabled));
        }
    }

    public class RecipeNutritionSummaryResolver : IValueResolver<Recipe, object, RecipeNutritionSummary>
    {
        private readonly IDailyIntakeHelper _dailyIntakeHelper;
        private readonly IConversion _conversion;
        private readonly DailyIntakeReference _dailyIntakeRef;

        public RecipeNutritionSummaryResolver(
            IDailyIntakeHelper dailyIntakeHelper,
            IConversion conversion,
            IOptions<DailyIntakeReference> dailyIntakeRef)
        {
            _dailyIntakeHelper = dailyIntakeHelper;
            _conversion = conversion;
            _dailyIntakeRef = dailyIntakeRef.Value;
        }

        public RecipeNutritionSummary Resolve(Recipe source, object dest, RecipeNutritionSummary destMember, ResolutionContext context)
        {
            RecipeIngredient[] validRecipeIngredients = source.RecipeIngredients
                .Where(x => x.Amount.HasValue 
                    && ((x.Ingredient.ServingSizeIsOneUnit && x.Unit == null) || (!x.Ingredient.ServingSizeIsOneUnit && x.Unit != null)))
                .ToArray();

            var nutritionSummary = new RecipeNutritionSummary();

            foreach (var recipeIngredient in validRecipeIngredients)
            {
                bool ingredientHasNutritionData = false;
                short servingSize = recipeIngredient.Ingredient.ServingSize;
                bool servingSizeIsOneUnit = recipeIngredient.Ingredient.ServingSizeIsOneUnit;
                float amount = recipeIngredient.Amount.Value;
                string unit = recipeIngredient.Unit;
                byte servings = source.Servings;

                if (recipeIngredient.Ingredient.Calories.HasValue)
                {
                    ingredientHasNutritionData = true;
                    nutritionSummary.Calories = AddGramsValuePerAmountAndServing(nutritionSummary.Calories, recipeIngredient.Ingredient.Calories.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Fat.HasValue)
                {
                    ingredientHasNutritionData = true;
                    nutritionSummary.Fat = AddGramsValuePerAmountAndServing(nutritionSummary.Fat, recipeIngredient.Ingredient.Fat.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.SaturatedFat.HasValue)
                {
                    ingredientHasNutritionData = true;
                    nutritionSummary.SaturatedFat = AddGramsValuePerAmountAndServing(nutritionSummary.SaturatedFat, recipeIngredient.Ingredient.SaturatedFat.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.TransFat.HasValue)
                {
                    ingredientHasNutritionData = true;
                    nutritionSummary.TransFat = AddGramsValuePerAmountAndServing(nutritionSummary.TransFat, recipeIngredient.Ingredient.TransFat.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Carbohydrate.HasValue)
                {
                    ingredientHasNutritionData = true;
                    nutritionSummary.Carbohydrate = AddGramsValuePerAmountAndServing(nutritionSummary.Carbohydrate, recipeIngredient.Ingredient.Carbohydrate.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Sugars.HasValue)
                {
                    ingredientHasNutritionData = true;
                    nutritionSummary.Sugars = AddGramsValuePerAmountAndServing(nutritionSummary.Sugars, recipeIngredient.Ingredient.Sugars.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.AddedSugars.HasValue)
                {
                    ingredientHasNutritionData = true;
                    nutritionSummary.AddedSugars = AddGramsValuePerAmountAndServing(nutritionSummary.AddedSugars, recipeIngredient.Ingredient.AddedSugars.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Fiber.HasValue)
                {
                    ingredientHasNutritionData = true;
                    nutritionSummary.Fiber = AddGramsValuePerAmountAndServing(nutritionSummary.Fiber, recipeIngredient.Ingredient.Fiber.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Protein.HasValue)
                {
                    ingredientHasNutritionData = true;
                    nutritionSummary.Protein = AddGramsValuePerAmountAndServing(nutritionSummary.Protein, recipeIngredient.Ingredient.Protein.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Sodium.HasValue)
                {
                    ingredientHasNutritionData = true;
                    nutritionSummary.Sodium = AddMilligramsValuePerAmountAndServing(nutritionSummary.Sodium, recipeIngredient.Ingredient.Sodium.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Cholesterol.HasValue)
                {
                    ingredientHasNutritionData = true;
                    nutritionSummary.Cholesterol = AddMilligramsValuePerAmountAndServing(nutritionSummary.Cholesterol, recipeIngredient.Ingredient.Cholesterol.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.VitaminA.HasValue)
                {
                    ingredientHasNutritionData = true;
                    nutritionSummary.VitaminA = AddMilligramsValuePerAmountAndServing(nutritionSummary.VitaminA, recipeIngredient.Ingredient.VitaminA.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.VitaminC.HasValue)
                {
                    ingredientHasNutritionData = true;
                    nutritionSummary.VitaminC = AddMilligramsValuePerAmountAndServing(nutritionSummary.VitaminC, recipeIngredient.Ingredient.VitaminC.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.VitaminD.HasValue)
                {
                    ingredientHasNutritionData = true;
                    nutritionSummary.VitaminD = AddMilligramsValuePerAmountAndServing(nutritionSummary.VitaminD, recipeIngredient.Ingredient.VitaminD.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Calcium.HasValue)
                {
                    ingredientHasNutritionData = true;
                    nutritionSummary.Calcium = AddMilligramsValuePerAmountAndServing(nutritionSummary.Calcium, recipeIngredient.Ingredient.Calcium.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Iron.HasValue)
                {
                    ingredientHasNutritionData = true;
                    nutritionSummary.Iron = AddMilligramsValuePerAmountAndServing(nutritionSummary.Iron, recipeIngredient.Ingredient.Iron.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Potassium.HasValue)
                {
                    ingredientHasNutritionData = true;
                    nutritionSummary.Potassium = AddMilligramsValuePerAmountAndServing(nutritionSummary.Potassium, recipeIngredient.Ingredient.Potassium.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }
                if (recipeIngredient.Ingredient.Magnesium.HasValue)
                {
                    ingredientHasNutritionData = true;
                    nutritionSummary.Magnesium = AddMilligramsValuePerAmountAndServing(nutritionSummary.Magnesium, recipeIngredient.Ingredient.Magnesium.Value, servingSize, servingSizeIsOneUnit, amount, unit, servings);
                }

                if (ingredientHasNutritionData)
                {
                    nutritionSummary.IngredientIds.Add(recipeIngredient.Ingredient.Id);
                }
            }

            RecommendedIntake intake = null;
            DietaryProfile dietaryProfile = source.User.DietaryProfile;

            if (dietaryProfile != null)
            {
                IEnumerable<DailyIntakeAgeGroup> intakeByGender = dietaryProfile.Gender == "Male" ? _dailyIntakeRef.Male : _dailyIntakeRef.Female;
                short age = dietaryProfile.GetAge();
                intake = intakeByGender.FirstOrDefault(x => age >= x.AgeFrom && age <= x.AgeTo).RecommendedIntake;
            }

            if (nutritionSummary.Calories.HasValue)
            {
                if (intake != null && dietaryProfile.TrackCalories)
                {
                    short dailyCalories = dietaryProfile.CustomCalories ?? _dailyIntakeHelper.DeriveDailyCaloriesIntake(dietaryProfile.GetAge(), dietaryProfile.Gender,
                            dietaryProfile.Height.Value, dietaryProfile.Weight.Value, dietaryProfile.ActivityLevel, dietaryProfile.Goal);
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
                if (intake != null && dietaryProfile.TrackSaturatedFat)
                {
                    short dailySaturatedFat = dietaryProfile.CustomSaturatedFat ?? intake.SaturatedFatMax;
                    nutritionSummary.SaturatedFatFromDaily = GetPercentageFromRecommendedDailyIntake(dailySaturatedFat, nutritionSummary.SaturatedFat.Value);
                    nutritionSummary.SaturatedFatFromDailyGrade = GetDailyGrade(nutritionSummary.SaturatedFatFromDaily.Value, false);
                }

                nutritionSummary.SaturatedFat = (float)Math.Round((double)nutritionSummary.SaturatedFat, 2, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.TransFat.HasValue)
            {
                nutritionSummary.TransFat = (float)Math.Round((double)nutritionSummary.TransFat, 2, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.Carbohydrate.HasValue)
            {
                if (intake != null && dietaryProfile.TrackCarbohydrate)
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
                if (intake != null && dietaryProfile.TrackAddedSugars)
                {
                    short dailyAddedSugars = dietaryProfile.CustomAddedSugars ?? intake.AddedSugarsMax;
                    nutritionSummary.AddedSugarsFromDaily = GetPercentageFromRecommendedDailyIntake(dailyAddedSugars, nutritionSummary.AddedSugars.Value);
                    nutritionSummary.AddedSugarsFromDailyGrade = GetDailyGrade(nutritionSummary.AddedSugarsFromDaily.Value, false);
                }

                nutritionSummary.AddedSugars = (float)Math.Round((double)nutritionSummary.AddedSugars, 2, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.Fiber.HasValue)
            {
                if (intake != null && dietaryProfile.TrackFiber)
                {
                    float dailyFiber = dietaryProfile.CustomFiber ?? intake.Fiber;
                    nutritionSummary.FiberFromDaily = GetPercentageFromRecommendedDailyIntake(dailyFiber, nutritionSummary.Fiber.Value);
                    nutritionSummary.FiberFromDailyGrade = GetDailyGrade(nutritionSummary.FiberFromDaily.Value, true);
                }

                nutritionSummary.Fiber = (float)Math.Round((double)nutritionSummary.Fiber, 2, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.Protein.HasValue)
            {
                if (intake != null && dietaryProfile.TrackProtein)
                {
                    short dailyProtein = dietaryProfile.CustomProtein ?? intake.Protein;
                    nutritionSummary.ProteinFromDaily = GetPercentageFromRecommendedDailyIntake(dailyProtein, nutritionSummary.Protein.Value);
                    nutritionSummary.ProteinFromDailyGrade = GetDailyGrade(nutritionSummary.ProteinFromDaily.Value, true);
                }

                nutritionSummary.Protein = (float)Math.Round((double)nutritionSummary.Protein, 2, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.Sodium.HasValue)
            {
                if (intake != null && dietaryProfile.TrackSodium)
                {
                    short dailySodium = dietaryProfile.CustomSodium ?? intake.Sodium;
                    nutritionSummary.SodiumFromDaily = GetPercentageFromRecommendedDailyIntake(dailySodium, nutritionSummary.Sodium.Value);
                    nutritionSummary.SodiumFromDailyGrade = GetDailyGrade(nutritionSummary.SodiumFromDaily.Value, false);
                }

                nutritionSummary.Sodium = (float)Math.Round((double)nutritionSummary.Sodium, 1, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.Cholesterol.HasValue)
            {
                if (intake != null && dietaryProfile.TrackCholesterol)
                {
                    short dailyCholesterol = dietaryProfile.CustomCholesterol ?? intake.CholesterolMax;
                    nutritionSummary.CholesterolFromDaily = GetPercentageFromRecommendedDailyIntake(dailyCholesterol, nutritionSummary.Cholesterol.Value);
                    nutritionSummary.CholesterolFromDailyGrade = GetDailyGrade(nutritionSummary.CholesterolFromDaily.Value, false);
                }

                nutritionSummary.Cholesterol = (float)Math.Round((double)nutritionSummary.Cholesterol, 1, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.VitaminA.HasValue)
            {
                if (intake != null && dietaryProfile.TrackVitaminA)
                {
                    short dailyVitaminA = dietaryProfile.CustomVitaminA ?? intake.VitaminA;
                    nutritionSummary.VitaminAFromDaily = GetPercentageFromRecommendedDailyIntake(dailyVitaminA, nutritionSummary.VitaminA.Value);
                    nutritionSummary.VitaminAFromDailyGrade = GetDailyGrade(nutritionSummary.VitaminAFromDaily.Value, true);
                }

                nutritionSummary.VitaminA = (float)Math.Round((double)nutritionSummary.VitaminA, 1, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.VitaminC.HasValue)
            {
                if (intake != null && dietaryProfile.TrackVitaminC)
                {
                    short dailyVitaminC = dietaryProfile.CustomVitaminC ?? intake.VitaminC;
                    nutritionSummary.VitaminCFromDaily = GetPercentageFromRecommendedDailyIntake(dailyVitaminC, nutritionSummary.VitaminC.Value);
                    nutritionSummary.VitaminCFromDailyGrade = GetDailyGrade(nutritionSummary.VitaminCFromDaily.Value, true);
                }

                nutritionSummary.VitaminC = (float)Math.Round((double)nutritionSummary.VitaminC, 1, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.VitaminD.HasValue)
            {
                if (intake != null && dietaryProfile.TrackVitaminD)
                {
                    short dailyVitaminD = dietaryProfile.CustomVitaminD ?? intake.VitaminD;
                    nutritionSummary.VitaminDFromDaily = GetPercentageFromRecommendedDailyIntake(dailyVitaminD, nutritionSummary.VitaminD.Value);
                    nutritionSummary.VitaminDFromDailyGrade = GetDailyGrade(nutritionSummary.VitaminDFromDaily.Value, true);
                }

                nutritionSummary.VitaminD = (float)Math.Round((double)nutritionSummary.VitaminD, 1, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.Calcium.HasValue)
            {
                if (intake != null && dietaryProfile.TrackCalcium)
                {
                    short dailyCalcium = dietaryProfile.CustomCalcium ?? intake.Calcium;
                    nutritionSummary.CalciumFromDaily = GetPercentageFromRecommendedDailyIntake(dailyCalcium, nutritionSummary.Calcium.Value);
                    nutritionSummary.CalciumFromDailyGrade = GetDailyGrade(nutritionSummary.CalciumFromDaily.Value, true);
                }

                nutritionSummary.Calcium = (float)Math.Round((double)nutritionSummary.Calcium, 1, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.Iron.HasValue)
            {
                if (intake != null && dietaryProfile.TrackIron)
                {
                    short dailyIron = dietaryProfile.CustomIron ?? intake.Iron;
                    nutritionSummary.IronFromDaily = GetPercentageFromRecommendedDailyIntake(dailyIron, nutritionSummary.Iron.Value);
                    nutritionSummary.IronFromDailyGrade = GetDailyGrade(nutritionSummary.IronFromDaily.Value, true);
                }

                nutritionSummary.Iron = (float)Math.Round((double)nutritionSummary.Iron, 1, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.Potassium.HasValue)
            {
                if (intake != null && dietaryProfile.TrackPotassium)
                {
                    short dailyPotassium = dietaryProfile.CustomPotassium ?? intake.Potassium;
                    nutritionSummary.PotassiumFromDaily = GetPercentageFromRecommendedDailyIntake(dailyPotassium, nutritionSummary.Potassium.Value);
                    nutritionSummary.PotassiumFromDailyGrade = GetDailyGrade(nutritionSummary.PotassiumFromDaily.Value, true);
                }

                nutritionSummary.Potassium = (float)Math.Round((double)nutritionSummary.Potassium, 1, MidpointRounding.AwayFromZero);
            }
            if (nutritionSummary.Magnesium.HasValue)
            {
                if (intake != null && dietaryProfile.TrackMagnesium)
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
                || nutritionSummary.TransFat.HasValue
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

        private float? AddGramsValuePerAmountAndServing(float? currentValue, float valueInGrams, short servingSizeGrams, bool servingSizeIsOneUnit, float amount, string unit, byte servings)
        {
            if (servingSizeIsOneUnit)
            {
                return (valueInGrams * amount / servings) + (currentValue ?? 0);
            }

            float amountInGrams = _conversion.ToGrams(unit, amount);
            float valuePerGram = valueInGrams / servingSizeGrams;
            return (valuePerGram * amountInGrams / servings) + (currentValue ?? 0);
        }

        private float? AddMilligramsValuePerAmountAndServing(float? currentValue, float valueInMilligrams, short servingSizeGrams, bool servingSizeIsOneUnit, float amount, string unit, byte servings)
        {
            if (servingSizeIsOneUnit)
            {
                return (valueInMilligrams * amount / servings) + (currentValue ?? 0);
            }

            float amountInMilligrams = _conversion.ToMilligrams(unit, amount);
            float valuePerMilligram = valueInMilligrams / (servingSizeGrams * 1000);
            return (valuePerMilligram * amountInMilligrams / servings) + (currentValue ?? 0);
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

    public class RecipeCostSummaryResolver : IValueResolver<Recipe, object, RecipeCostSummary>
    {
        private readonly IConversion _conversion;
        private readonly ICurrencyService _currencyService;

        public RecipeCostSummaryResolver(IConversion conversion, ICurrencyService currencyService)
        {
            _conversion = conversion;
            _currencyService = currencyService;
        }

        public RecipeCostSummary Resolve(Recipe source, object dest, RecipeCostSummary destMember, ResolutionContext context)
        {
            RecipeIngredient[] validRecipeIngredients = source.RecipeIngredients
                .Where(x => x.Amount.HasValue
                    && x.Ingredient.Price.HasValue
                    && ((x.Ingredient.ProductSizeIsOneUnit && x.Unit == null) || (!x.Ingredient.ProductSizeIsOneUnit && x.Unit != null)))
                .ToArray();

            var costSummary = new RecipeCostSummary();
            var currency = (string)context.Items["Currency"];

            foreach (var recipeIngredient in validRecipeIngredients)
            {
                costSummary.IngredientIds.Add(recipeIngredient.Ingredient.Id);

                short productSize = recipeIngredient.Ingredient.ProductSize;
                bool productSizeIsOneUnit = recipeIngredient.Ingredient.ProductSizeIsOneUnit;
                float amount = recipeIngredient.Amount.Value;
                string unit = recipeIngredient.Unit;

                decimal price = _currencyService.Convert(recipeIngredient.Ingredient.Price.Value, recipeIngredient.Ingredient.Currency, currency);

                costSummary.Cost = AddPricePerAmount(costSummary.Cost, price, productSize, productSizeIsOneUnit, amount, unit);
            }

            if (costSummary.Cost.HasValue)
            {
                costSummary.IsSet = true;

                if (source.Servings > 1)
                {
                    costSummary.CostPerServing = costSummary.Cost.Value / source.Servings;
                }
            }

            return costSummary;
        }

        private decimal? AddPricePerAmount(decimal? currentValue, decimal priceInGrams, short productSizeGrams, bool productSizeIsOneUnit, float amount, string unit)
        {
            if (productSizeIsOneUnit)
            {
                return priceInGrams * (decimal)amount + (currentValue ?? 0);
            }

            float amountInGrams = _conversion.ToGrams(unit, amount);
            decimal valuePerGram = priceInGrams / productSizeGrams;
            return valuePerGram * (decimal)amountInGrams + (currentValue ?? 0);
        }
    }

    public class DurationResolver : IMemberValueResolver<object, object, TimeSpan?, string>
    {
        public string Resolve(object source, object destination, TimeSpan? sourceMember, string destinationMember, ResolutionContext context)
        {
            return sourceMember.HasValue ? sourceMember.Value.ToString(@"hh\:mm") : string.Empty;
        }
    }

    public class RecipeNameResolver : IMemberValueResolver<object, object, List<Recipe>, List<string>>
    {
        public List<string> Resolve(object source, object destination, List<Recipe> sourceMember, List<string> destinationMember, ResolutionContext context)
        {
            return sourceMember.Select(x => x.Name).ToList();
        }
    }

    public class RecipeIngredientHasNutritionDataResolver : IValueResolver<RecipeIngredient, object, bool>
    {
        public bool Resolve(RecipeIngredient source, object dest, bool destMember, ResolutionContext context)
        {
            var hasNutritionData = source.Ingredient.Calories.HasValue
                || source.Ingredient.Fat.HasValue
                || source.Ingredient.SaturatedFat.HasValue
                || source.Ingredient.TransFat.HasValue
                || source.Ingredient.Carbohydrate.HasValue
                || source.Ingredient.Sugars.HasValue
                || source.Ingredient.AddedSugars.HasValue
                || source.Ingredient.Fiber.HasValue
                || source.Ingredient.Protein.HasValue
                || source.Ingredient.Sodium.HasValue
                || source.Ingredient.Cholesterol.HasValue
                || source.Ingredient.VitaminA.HasValue
                || source.Ingredient.VitaminC.HasValue
                || source.Ingredient.VitaminD.HasValue
                || source.Ingredient.Calcium.HasValue
                || source.Ingredient.Iron.HasValue
                || source.Ingredient.Potassium.HasValue
                || source.Ingredient.Magnesium.HasValue;

            return hasNutritionData;
        }
    }

    public class RecipeIngredientHasPriceDataResolver : IValueResolver<RecipeIngredient, object, bool>
    {
        public bool Resolve(RecipeIngredient source, object dest, bool destMember, ResolutionContext context)
        {
            return source.Ingredient.Price.HasValue;
        }
    }

    public class IngredientHasNutritionDataResolver : IValueResolver<Ingredient, object, bool>
    {
        public bool Resolve(Ingredient source, object dest, bool destMember, ResolutionContext context)
        {
            var hasNutritionData = source.Calories.HasValue
                || source.Fat.HasValue
                || source.SaturatedFat.HasValue
                || source.TransFat.HasValue
                || source.Carbohydrate.HasValue
                || source.Sugars.HasValue
                || source.AddedSugars.HasValue
                || source.Fiber.HasValue
                || source.Protein.HasValue
                || source.Sodium.HasValue
                || source.Cholesterol.HasValue
                || source.VitaminA.HasValue
                || source.VitaminC.HasValue
                || source.VitaminD.HasValue
                || source.Calcium.HasValue
                || source.Iron.HasValue
                || source.Potassium.HasValue
                || source.Magnesium.HasValue;

            return hasNutritionData;
        }
    }

    public class IngredientHasPriceDataResolver : IValueResolver<Ingredient, object, bool>
    {
        public bool Resolve(Ingredient source, object dest, bool destMember, ResolutionContext context)
        {
            return source.Price.HasValue;
        }
    }

    public class NutritionDataResolver : IValueResolver<Ingredient, object, IngredientNutritionData>
    {
        public IngredientNutritionData Resolve(Ingredient source, object dest, IngredientNutritionData destMember, ResolutionContext context)
        {
            var nutritionData = new IngredientNutritionData
            {
                ServingSize = source.ServingSize,
                ServingSizeIsOneUnit = source.ServingSizeIsOneUnit,
                Calories = source.Calories,
                Fat = source.Fat,
                SaturatedFat = source.SaturatedFat,
                TransFat = source.TransFat,
                Carbohydrate = source.Carbohydrate,
                Sugars = source.Sugars,
                AddedSugars = source.AddedSugars,
                Fiber = source.Fiber,
                Protein = source.Protein,
                Sodium = source.Sodium,
                Cholesterol = source.Cholesterol,
                VitaminA = source.VitaminA,
                VitaminC = source.VitaminC,
                VitaminD = source.VitaminD,
                Calcium = source.Calcium,
                Iron = source.Iron,
                Potassium = source.Potassium,
                Magnesium = source.Magnesium
            };

            nutritionData.IsSet = source.Calories.HasValue
                || source.Fat.HasValue
                || source.SaturatedFat.HasValue
                || source.TransFat.HasValue
                || source.Carbohydrate.HasValue
                || source.Sugars.HasValue
                || source.AddedSugars.HasValue
                || source.Fiber.HasValue
                || source.Protein.HasValue
                || source.Sodium.HasValue
                || source.Cholesterol.HasValue
                || source.VitaminA.HasValue
                || source.VitaminC.HasValue
                || source.VitaminD.HasValue
                || source.Calcium.HasValue
                || source.Iron.HasValue
                || source.Potassium.HasValue
                || source.Magnesium.HasValue;

            return nutritionData;
        }
    }

    public class PriceDataResolver : IValueResolver<Ingredient, object, IngredientPriceData>
    {
        public IngredientPriceData Resolve(Ingredient source, object dest, IngredientPriceData destMember, ResolutionContext context)
        {
            var priceData = new IngredientPriceData
            {
                IsSet = source.Price.HasValue,
                ProductSize = source.ProductSize,
                ProductSizeIsOneUnit = source.ProductSizeIsOneUnit,
                Price = source.Price,
                Currency = source.Currency
            };

            return priceData;
        }
    }

    public class HeightCmResolver : IValueResolver<DietaryProfile, object, short?>
    {
        public short? Resolve(DietaryProfile source, object dest, short? destMember, ResolutionContext context)
        {
            if (source.Height.HasValue && !source.User.ImperialSystem)
            {
                return (short)Math.Floor((double)source.Height);
            }

            return null;
        }
    }

    public class HeightFeetResolver : IValueResolver<DietaryProfile, object, short?>
    {
        private readonly IConversion _conversion;

        public HeightFeetResolver(IConversion conversion)
        {
            _conversion = conversion;
        }

        public short? Resolve(DietaryProfile source, object dest, short? destMember, ResolutionContext context)
        {
            if (source.Height.HasValue && source.User.ImperialSystem)
            {
                var (feet, _) = _conversion.CentimetersToFeetAndInches(source.Height.Value);
                return (short?)feet;
            }

            return null;
        }
    }

    public class HeightInchesResolver : IValueResolver<DietaryProfile, object, short?>
    {
        private readonly IConversion _conversion;

        public HeightInchesResolver(IConversion conversion)
        {
            _conversion = conversion;
        }

        public short? Resolve(DietaryProfile source, object dest, short? destMember, ResolutionContext context)
        {
            if (source.Height.HasValue && source.User.ImperialSystem)
            {
                var (_, inches) = _conversion.CentimetersToFeetAndInches(source.Height.Value);
                return (short?)inches;
            }

            return null;
        }
    }

    public class WeightKgResolver : IValueResolver<DietaryProfile, object, float?>
    {
        public float? Resolve(DietaryProfile source, object dest, float? destMember, ResolutionContext context)
        {
            if (source.Weight.HasValue && !source.User.ImperialSystem)
            {
                return (float)Math.Round(source.Weight.Value, 1);
            }

            return null;
        }
    }

    public class WeightLbsResolver : IValueResolver<DietaryProfile, object, short?>
    {
        private readonly IConversion _conversion;

        public WeightLbsResolver(IConversion conversion)
        {
            _conversion = conversion;
        }

        public short? Resolve(DietaryProfile source, object dest, short? destMember, ResolutionContext context)
        {
            if (source.Weight.HasValue && source.User.ImperialSystem)
            {
                var pounds = _conversion.KilosToPounds(source.Weight.Value);
                return (short?)pounds;
            }

            return null;
        }
    }

    public class DailyIntakeResolver : IValueResolver<DietaryProfile, object, DailyIntake>
    {
        private readonly IDailyIntakeHelper _dailyIntakeHelper;
        private readonly DailyIntakeReference _dailyIntakeRef;

        public DailyIntakeResolver(IDailyIntakeHelper dailyIntakeHelper, IOptions<DailyIntakeReference> dailyIntakeRef)
        {
            _dailyIntakeHelper = dailyIntakeHelper;
            _dailyIntakeRef = dailyIntakeRef.Value;
        }

        public DailyIntake Resolve(DietaryProfile source, object dest, DailyIntake destMember, ResolutionContext context)
        {
            IEnumerable<DailyIntakeAgeGroup> intakeByGender = source.Gender == "Male" ? _dailyIntakeRef.Male : _dailyIntakeRef.Female;
            short age = source.GetAge();
            RecommendedIntake intake = intakeByGender.FirstOrDefault(x => age >= x.AgeFrom && age <= x.AgeTo).RecommendedIntake;

            var dailyIntake = new DailyIntake
            {
                Calories = _dailyIntakeHelper.DeriveDailyCaloriesIntake(age, source.Gender, source.Height.Value,
                    source.Weight.Value, source.ActivityLevel, source.Goal),
                CustomCalories = source.CustomCalories,
                TrackCalories = source.TrackCalories,
                //Fat = intake.Fat,
                //CustomFat = source.CustomFat,
                //TrackFat = source.TrackFat,
                SaturatedFat = intake.SaturatedFatMax,
                CustomSaturatedFat = source.CustomSaturatedFat,
                TrackSaturatedFat = source.TrackSaturatedFat,
                Carbohydrate = intake.Carbohydrate,
                CustomCarbohydrate = source.CustomCarbohydrate,
                TrackCarbohydrate = source.TrackCarbohydrate,
                AddedSugars = intake.AddedSugarsMax,
                CustomAddedSugars = source.CustomAddedSugars,
                TrackAddedSugars = source.TrackAddedSugars,
                Fiber = intake.Fiber,
                CustomFiber = source.CustomFiber,
                TrackFiber = source.TrackFiber,
                Protein = intake.Protein,
                CustomProtein = source.CustomProtein,
                TrackProtein = source.TrackProtein,
                Sodium = intake.Sodium,
                CustomSodium = source.CustomSodium,
                TrackSodium = source.TrackSodium,
                Cholesterol = intake.CholesterolMax,
                CustomCholesterol = source.CustomCholesterol,
                TrackCholesterol = source.TrackCholesterol,
                VitaminA = intake.VitaminA,
                CustomVitaminA = source.CustomVitaminA,
                TrackVitaminA = source.TrackVitaminA,
                VitaminC = intake.VitaminC,
                CustomVitaminC = source.CustomVitaminC,
                TrackVitaminC = source.TrackVitaminC,
                VitaminD = intake.VitaminD,
                CustomVitaminD = source.CustomVitaminD,
                TrackVitaminD = source.TrackVitaminD,
                Calcium = intake.Calcium,
                CustomCalcium = source.CustomCalcium,
                TrackCalcium = source.TrackCalcium,
                Iron = intake.Iron,
                CustomIron = source.CustomIron,
                TrackIron = source.TrackIron,
                Potassium = intake.Potassium,
                CustomPotassium = source.CustomPotassium,
                TrackPotassium = source.TrackPotassium,
                Magnesium = intake.Magnesium,
                CustomMagnesium = source.CustomMagnesium,
                TrackMagnesium = source.TrackMagnesium
            };

            return dailyIntake;
        }
    }
}
