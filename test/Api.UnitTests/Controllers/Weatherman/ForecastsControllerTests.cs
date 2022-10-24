using System;
using System.Threading.Tasks;
using Api.Controllers.Weatherman;
using Api.UnitTests.Builders;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Api.UnitTests.Controllers.Weatherman;

public class ForecastsControllerTests
{
    private readonly ForecastsController _sut;

    public ForecastsControllerTests()
    {
        _sut = new ForecastsController(null)
        {
            ControllerContext = new ControllerContextBuilder().Build()
        };
    }

    [Fact]
    public async Task Create_Returns400_IfTemperatureUnitParamMissing()
    {
        var result = await _sut.Get(1, 2, null, "mm", "kmh", DateTime.Now);
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Create_Returns400_IfPrecipitationUnitParamMissing()
    {
        var result = await _sut.Get(1, 2, "celsius", null, "kmh", DateTime.Now);
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Create_Returns400_IfWindSpeedUnitParamMissing()
    {
        var result = await _sut.Get(1, 2, "celsius", "mm", null, DateTime.Now);
        Assert.IsType<BadRequestResult>(result);
    }
}
