using Application.UnitTests.Builders;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Sentry;
using Weatherman.Application.Contracts.Forecasts;
using Weatherman.Infrastructure;
using Xunit;

namespace Application.UnitTests.ServiceTests.ForecastServiceTests;

public class GetTests
{
    private readonly Mock<IForecastsRepository> _forecastsRepositoryMock = new();
    private readonly Mock<ISpan> _metricsSpanMock = new();
    private readonly IForecastService _sut;

    public GetTests()
    {
        _metricsSpanMock.Setup(x => x.StartChild(It.IsAny<string>())).Returns(new Mock<ISpan>().Object);

        _sut = new ForecastService(
            new Mock<IHttpClientFactory>().Object,
            _forecastsRepositoryMock.Object,
            new Mock<ILogger<ForecastService>>().Object);
    }

    [Fact]
    public async Task Throws_IfTemperatureUnitIsInvalid()
    {
        var parameters = new GetForecastBuilder().WithUnits().Build();
        parameters.TemperatureUnit = "invalid";

        await Assert.ThrowsAsync<ValidationException>(() => _sut.GetAsync(parameters, _metricsSpanMock.Object));
    }

    [Fact]
    public async Task Throws_IfTPrecipitationUnitIsInvalid()
    {
        var parameters = new GetForecastBuilder().WithUnits().Build();
        parameters.PrecipitationUnit = "invalid";

        await Assert.ThrowsAsync<ValidationException>(() => _sut.GetAsync(parameters, _metricsSpanMock.Object));
    }

    [Fact]
    public async Task Throws_IfWindSpeedUnitIsInvalid()
    {
        var parameters = new GetForecastBuilder().WithUnits().Build();
        parameters.WindSpeedUnit = "invalid";

        await Assert.ThrowsAsync<ValidationException>(() => _sut.GetAsync(parameters, _metricsSpanMock.Object));
    }
}
