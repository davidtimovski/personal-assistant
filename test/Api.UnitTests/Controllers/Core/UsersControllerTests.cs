using Api.UnitTests.Builders;
using Core.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Api.UnitTests.Controllers.Core;

public class UsersControllerTests
{
    private readonly UsersController _sut;

    public UsersControllerTests()
    {
        _sut = new UsersController(null, null, null)
        {
            ControllerContext = new ControllerContextBuilder().Build()
        };
    }

    [Fact]
    public async Task UpdateToDoNotificationsEnabled_Returns400_IfBodyMissing()
    {
        var result = await _sut.UpdateToDoNotificationsEnabled(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task UpdateChefNotificationsEnabled_Returns400_IfBodyMissing()
    {
        var result = await _sut.UpdateChefNotificationsEnabled(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task UpdateImperialSystem_Returns400_IfBodyMissing()
    {
        var result = await _sut.UpdateImperialSystem(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }
}
