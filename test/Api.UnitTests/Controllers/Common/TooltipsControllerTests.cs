using System.Threading.Tasks;
using Api.Controllers.Common;
using Microsoft.AspNetCore.Mvc;
using Api.UnitTests.Builders;
using Xunit;

namespace Api.UnitTests.Controllers.Common;

public class TooltipsControllerTests
{
    private readonly TooltipsController _sut;

    public TooltipsControllerTests()
    {
        _sut = new TooltipsController(null)
        {
            ControllerContext = new ControllerContextBuilder().Build()
        };
    }

    [Fact]
    public async Task ToggleDismissed_Returns400_IfBodyMissing()
    {
        var result = await _sut.ToggleDismissed(null);
        Assert.IsType<BadRequestResult>(result);
    }
}