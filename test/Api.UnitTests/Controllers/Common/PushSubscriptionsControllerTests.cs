using Api.Controllers.Common;
using Api.UnitTests.Builders;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Api.UnitTests.Controllers.Common;

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
        var result = await _sut.CreateSubscription(null);
        Assert.IsType<BadRequestResult>(result);
    }
}
