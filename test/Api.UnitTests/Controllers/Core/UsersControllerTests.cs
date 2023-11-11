using Api.UnitTests.Builders;
using Core.Api.Controllers;
using Core.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Api.UnitTests.Controllers.Core;

public class UsersControllerTests
{
    private readonly Mock<IUserIdLookup> _userIdLookupMock = new();
    private readonly UsersController _sut;

    public UsersControllerTests()
    {
        _userIdLookupMock.Setup(x => x.Contains(It.IsAny<string>())).Returns(true);
        _userIdLookupMock.Setup(x => x.Get(It.IsAny<string>())).Returns(1);

        _sut = new UsersController(
            _userIdLookupMock.Object,
            new Mock<IUsersRepository>().Object,
            new Mock<IUserService>().Object)
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
