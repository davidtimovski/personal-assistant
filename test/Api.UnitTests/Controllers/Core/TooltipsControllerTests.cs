using Api.UnitTests.Builders;
using Core.Api.Controllers;
using Core.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Api.UnitTests.Controllers.Core;

public class TooltipsControllerTests
{
    private readonly Mock<IUserIdLookup> _userIdLookupMock = new();
    private readonly TooltipsController _sut;

    public TooltipsControllerTests()
    {
        _userIdLookupMock.Setup(x => x.Contains(It.IsAny<string>())).Returns(true);
        _userIdLookupMock.Setup(x => x.Get(It.IsAny<string>())).Returns(1);

        _sut = new TooltipsController(_userIdLookupMock.Object, null, null)
        {
            ControllerContext = new ControllerContextBuilder().Build()
        };
    }

    [Fact]
    public async Task ToggleDismissed_Returns400_IfBodyMissing()
    {
        var result = await _sut.ToggleDismissed(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }
}
