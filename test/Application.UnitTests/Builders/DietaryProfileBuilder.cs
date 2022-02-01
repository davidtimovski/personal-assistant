using System;
using System.Collections.Generic;
using Application.Contracts.CookingAssistant.DietaryProfiles.Models;

namespace Application.UnitTests.Builders
{
    public class DietaryProfileBuilder
    {
        public UpdateDietaryProfile BuildUpdateModel()
        {
            return new UpdateDietaryProfile
            {
                HeightCm = 178,
                WeightKg = 70
            };
        }

        public GetRecommendedDailyIntake BuildGetRecommendedModel()
        {
            return new GetRecommendedDailyIntake
            {
                Birthday = new DateTime(1991, 8, 10),
                Gender = "Male",
                HeightCm = 178,
                WeightKg = 70,
                ActivityLevel = "Light",
                Goal = "None"
            };
        }

        public DailyIntakeReference BuildDailyIntakeReference()
        {
            return new DailyIntakeReference
            {
                Male = new List<DailyIntakeAgeGroup>
                {
                    new DailyIntakeAgeGroup
                    {
                        AgeFrom = 19,
                        AgeTo = 31,
                        RecommendedIntake = new RecommendedIntake
                        {
                            TotalFatFrom = 25,
                            TotalFatTo = 35,
                            SaturatedFatMax = 10,
                            Carbohydrate = 130,
                            AddedSugarsMax = 10,
                            Fiber = 33.6f,
                            Protein = 56,
                            Sodium = 2300,
                            CholesterolMax = 300,
                            VitaminA = 900,
                            VitaminC = 90,
                            VitaminD = 600,
                            Calcium = 1000,
                            Iron = 8,
                            Potassium = 4700,
                            Magnesium = 400
                        }
                    }
                }
            };
        }
    }
}
