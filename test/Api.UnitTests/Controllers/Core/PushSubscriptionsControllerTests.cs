using Api.UnitTests.Builders;
using Core.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Api.UnitTests.Controllers.Core;

public class PushSubscriptionsControllerTests
{
    private readonly PushSubscriptionsController _sut;

    public PushSubscriptionsControllerTests()
    {
        _sut = new PushSubscriptionsController(null, null, null)
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
