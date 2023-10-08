using Chef.Application.Contracts.DietaryProfiles.Models;

namespace Application.UnitTests.Builders;

internal class DietaryProfileBuilder
{
    internal UpdateDietaryProfile BuildUpdateModel()
    {
        return new UpdateDietaryProfile
        {
            HeightCm = 178,
            WeightKg = 70,
            UserId = 0,
            Birthday = DateTime.UtcNow,
            Gender = "",
            HeightFeet = 5,
            HeightInches = 10,
            WeightLbs = 120,
            ActivityLevel = null,
            Goal = null,
            CustomCalories = null,
            TrackCalories = false,
            CustomSaturatedFat = null,
            TrackSaturatedFat = false,
            CustomCarbohydrate = null,
            TrackCarbohydrate = false,
            CustomAddedSugars = null,
            TrackAddedSugars = false,
            CustomFiber = null,
            TrackFiber = false,
            CustomProtein = null,
            TrackProtein = false,
            CustomSodium = null,
            TrackSodium = false,
            CustomCholesterol = null,
            TrackCholesterol = false,
            CustomVitaminA = null,
            TrackVitaminA = false,
            CustomVitaminC = null,
            TrackVitaminC = false,
            CustomVitaminD = null,
            TrackVitaminD = false,
            CustomCalcium = null,
            TrackCalcium = false,
            CustomIron = null,
            TrackIron = false,
            CustomPotassium = null,
            TrackPotassium = false,
            CustomMagnesium = null,
            TrackMagnesium = false
        };
    }

    internal GetRecommendedDailyIntake BuildGetRecommendedModel()
    {
        return new GetRecommendedDailyIntake
        {
            Birthday = new DateTime(1991, 8, 10),
            Gender = "Male",
            HeightCm = 178,
            WeightKg = 70,
            ActivityLevel = "Light",
            Goal = "None",
            HeightFeet = 5,
            HeightInches = 10,
            WeightLbs = 120
        };
    }

    internal DailyIntakeReference BuildDailyIntakeReference()
    {
        return new DailyIntakeReference
        {
            Male = new List<DailyIntakeAgeGroup>
            {
                new DailyIntakeAgeGroup
                {
                    AgeFrom = 31,
                    AgeTo = 50,
                    RecommendedIntake = new RecommendedIntake
                    {
                        TotalFatFrom = 25,
                        TotalFatTo = 35,
                        SaturatedFatMax = 10,
                        Carbohydrate = 130,
                        AddedSugarsMax = 10,
                        Fiber = 33.8f,
                        Protein = 56,
                        Sodium = 2300,
                        CholesterolMax = 300,
                        VitaminA = 900,
                        VitaminC = 90,
                        VitaminD = 600,
                        Calcium = 1000,
                        Iron = 8,
                        Potassium = 4700,
                        Magnesium = 420
                    }
                }
            }
        };
    }
}
