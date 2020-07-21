using System.Collections.Generic;
using PersonalAssistant.Application.Contracts.CookingAssistant.Common;
using PersonalAssistant.Application.Services.CookingAssistant;
using Xunit;

namespace PersonalAssistant.Application.UnitTests.ServiceTests
{
    public class ConversionServiceTests
    {
        private readonly IConversionService _sut;

        public ConversionServiceTests()
        {
            var activityMultiplier = new Dictionary<string, float> {
                { "Sedentary", 1.2f },
                { "Light", 1.35f },
                { "Moderate", 1.465f },
                { "Active", 1.55f },
                { "VeryActive", 1.725f }
            };
            var dietaryGoalCalories = new Dictionary<string, short> {
                { "None", 0 },
                { "MildWeightLoss", -300 },
                { "WeightLoss", -600 },
                { "MildWeightGain", 300 },
                { "WeightGain", 600 }
            };

            _sut = new ConversionService(activityMultiplier, dietaryGoalCalories);
        }

        [Theory]
        [InlineData(5, 10)]
        [InlineData(6, 2)]
        public void ConvertFeetAndInchesToCentimetersAndBackIsCorrect(short feet, short inches)
        {
            float cm = _sut.ConvertFeetAndInchesToCentimeters(feet, inches);
            var result = _sut.ConvertCentimetersToFeetAndInches(cm);

            Assert.Equal(feet, result.feet);
            Assert.Equal(inches, result.inches);
        }

        [Theory]
        [InlineData(135)]
        [InlineData(191)]
        public void ConvertPoundsToKilosAndBackIsCorrect(short pounds)
        {
            float kilos = _sut.ConvertPoundsToKilos(pounds);
            float result = _sut.ConvertKilosToPounds(kilos);

            Assert.Equal(pounds, result);
        }
    }
}
