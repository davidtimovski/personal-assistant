using System;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Common
{
    public interface IConversionService
    {
        float ConvertToGrams(string unit, float amount);
        float ConvertToMilligrams(string unit, float amount);
        float ConvertFeetAndInchesToCentimeters(short feet, short inches);
        (short feet, short inches) ConvertCentimetersToFeetAndInches(float centimeters);
        float ConvertPoundsToKilos(short pounds);
        short ConvertKilosToPounds(float kilos);
        short DeriveDailyCaloriesIntake(short age, string gender, float height, float weight, string activityLevel, string goal);
    }
}
