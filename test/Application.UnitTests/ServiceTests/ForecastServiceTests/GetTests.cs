using Application.UnitTests.Builders;
using FluentValidation;
using Moq;
using Weatherman.Application.Contracts.Forecasts;
using Weatherman.Infrastructure;
using Xunit;

namespace Application.UnitTests.ServiceTests.ForecastServiceTests;

public class GetTests
{
    private readonly Mock<IForecastsRepository> _forecastsRepositoryMock = new();
    private readonly IForecastService _sut;

    public GetTests()
    {
        _sut = new ForecastService(
            null,
            _forecastsRepositoryMock.Object,
            null);
    }

    [Fact]
    public async Task Throws_IfTemperatureUnitIsInvalid()
    {
        var parameters = new GetForecastBuilder().WithUnits().Build();
        parameters.TemperatureUnit = "invalid";

        await Assert.ThrowsAsync<ValidationException>(() => _sut.GetAsync(parameters));
    }

    [Fact]
    public async Task Throws_IfTPrecipitationUnitIsInvalid()
    {
        var parameters = new GetForecastBuilder().WithUnits().Build();
        parameters.PrecipitationUnit = "invalid";

        await Assert.ThrowsAsync<ValidationException>(() => _sut.GetAsync(parameters));
    }

    [Fact]
    public async Task Throws_IfWindSpeedUnitIsInvalid()
    {
        var parameters = new GetForecastBuilder().WithUnits().Build();
        parameters.WindSpeedUnit = "invalid";

        await Assert.ThrowsAsync<ValidationException>(() => _sut.GetAsync(parameters));
    }
}
