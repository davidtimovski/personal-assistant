using Api.Controllers.CookingAssistant;
using Api.UnitTests.Builders;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Api.UnitTests.Controllers.CookingAssistant;

public class DietaryProfilesControllerTests
{
    private readonly DietaryProfilesController _sut;

    public DietaryProfilesControllerTests()
    {
        _sut = new DietaryProfilesController(null, null, null, null, null)
        {
            ControllerContext = new ControllerContextBuilder().Build()
        };
    }

    [Fact]
    public void GetDailyIntake_Returns400_IfBodyMissing()
    {
        var result = _sut.GetDailyIntake(null);
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task CreateOrUpdate_Returns400_IfBodyMissing()
    {
        var result = await _sut.CreateOrUpdate(null);
        Assert.IsType<BadRequestResult>(result);
    }
}
