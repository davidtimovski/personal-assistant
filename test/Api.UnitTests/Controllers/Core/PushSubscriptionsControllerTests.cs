using Api.UnitTests.Builders;
using Core.Api.Controllers;
using Core.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Api.UnitTests.Controllers.Core;

public class PushSubscriptionsControllerTests
{
    private readonly Mock<IUserIdLookup> _userIdLookupMock = new();
    private readonly PushSubscriptionsController _sut;

    public PushSubscriptionsControllerTests()
    {
        _userIdLookupMock.Setup(x => x.Contains(It.IsAny<string>())).Returns(true);
        _userIdLookupMock.Setup(x => x.Get(It.IsAny<string>())).Returns(1);

        _sut = new PushSubscriptionsController(
            _userIdLookupMock.Object,
            new Mock<IUsersRepository>().Object,
            new Mock<IPushSubscriptionService>().Object)
        {
            ControllerContext = new ControllerContextBuilder().Build()
        };
    }

    [Fact]
    public async Task CreateSubscription_Returns400_IfBodyMissing()
    {
        var result = await _sut.CreateSubscription(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }
}
