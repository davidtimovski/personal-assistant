using System;
using System.Collections.Generic;
using PersonalAssistant.Application.Contracts.CookingAssistant.Common;

namespace PersonalAssistant.Application.Services.CookingAssistant
{
    public class ConversionService : IConversionService
    {
        private const float gramsInOunce = 28.3495f;
        private const float gramsInCup = 220;
        private const float gramsInTablespoon = 14.15f;
        private const float gramsInTeaspoon = 4.2f;
        private const float poundsInKilo = 2.2046f;
        private const float centimetersInFoot = 30.48f;
        private const float centimetersInInch = 2.54f;
        private readonly Dictionary<string, float> _activityMultiplier;
        private readonly Dictionary<string, short> _dietaryGoalCalories;

        public ConversionService(Dictionary<string, float> activityMultiplier, Dictionary<string, short> dietaryGoalCalories)
        {
            _activityMultiplier = activityMultiplier;
            _dietaryGoalCalories = dietaryGoalCalories;
        }

        public float ConvertToGrams(string unit, float amount)
        {
            switch (unit)
            {
                case "g":
                case "ml": return amount;
                case "oz": return gramsInOunce * amount;
                case "cup": return gramsInCup * amount;
                case "tbsp": return gramsInTablespoon * amount;
                case "tsp": return gramsInTeaspoon * amount;
                default: throw new ArgumentException();
            }
        }

        public float ConvertToMilligrams(string unit, float amount)
        {
            switch (unit)
            {
                case "g":
                case "ml": return amount * 1000;
                case "oz": return gramsInOunce * amount * 1000;
                case "cup": return gramsInCup * amount * 1000;
                case "tbsp": return gramsInTablespoon * amount * 1000;
                case "tsp": return gramsInTeaspoon * amount * 1000;
                default: throw new ArgumentException();
            }
        }

        public float ConvertFeetAndInchesToCentimeters(short feet, short inches)
        {
            float feetInCm = feet * centimetersInFoot;
            float inchesInCm = inches * centimetersInInch;
            float cm = feetInCm + inchesInCm;
            return cm;
        }

        public (short feet, short inches) ConvertCentimetersToFeetAndInches(float centimeters)
        {
            double totalInches = Math.Floor(centimeters / centimetersInInch);
            short feet = (short)((totalInches - totalInches % 12) / 12);
            short inches = (short)(totalInches % 12);
            return (feet, inches);
        }

        public float ConvertPoundsToKilos(short pounds)
        {
            float kilos = pounds / poundsInKilo;
            return kilos;
        }

        public short ConvertKilosToPounds(float kilos)
        {
            float pounds = kilos * poundsInKilo;
            return (short)Math.Round(pounds, MidpointRounding.AwayFromZero);
        }

        public short DeriveDailyCaloriesIntake(short age, string gender, float height, float weight, string activityLevel, string goal)
        {
            // Find BMR (Mifflin-St Jeor equation)
            double bmr = Math.Round((10 * weight) + (6.25 * height) - (5 * age));
            if (gender == "Male")
            {
                bmr += 5;
            }
            else
            {
                bmr -= 161;
            }

            // Find recommended daily calories based on activity level
            bmr = Math.Round(bmr * _activityMultiplier[activityLevel]);

            // Adjust recommended daily calories based on dietary goal
            bmr = Math.Round(bmr + _dietaryGoalCalories[goal]);

            return (short)bmr;
        }
    }
}
