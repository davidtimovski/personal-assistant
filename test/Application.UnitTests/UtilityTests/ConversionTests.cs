using Chef.Utility;
using Xunit;

namespace Application.UnitTests.UtilityTests;

public class ConversionTests
{
    private readonly IConversion _sut;

    public ConversionTests()
    {
        _sut = new Conversion();
    }

    [Theory]
    [InlineData(5, 10)]
    [InlineData(6, 2)]
    public void ConvertFeetAndInchesToCentimetersAndBackIsCorrect(short feet, short inches)
    {
        float cm = _sut.FeetAndInchesToCentimeters(feet, inches);
        var result = _sut.CentimetersToFeetAndInches(cm);

        Assert.Equal(feet, result.Item1);
        Assert.Equal(inches, result.Item2);
    }

    [Theory]
    [InlineData(135)]
    [InlineData(191)]
    public void ConvertPoundsToKilosAndBackIsCorrect(short pounds)
    {
        float kilos = _sut.PoundsToKilos(pounds);
        float result = _sut.KilosToPounds(kilos);

        Assert.Equal(pounds, result);
    }
}
