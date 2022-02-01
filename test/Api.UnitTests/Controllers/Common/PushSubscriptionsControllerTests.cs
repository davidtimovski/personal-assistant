using System.Threading.Tasks;
using Api.Controllers.Common;
using Microsoft.AspNetCore.Mvc;
using Api.UnitTests.Builders;
using Xunit;

namespace Api.UnitTests.Controllers.Common
{
    public class PushSubscriptionsControllerTests
    {
        private readonly PushSubscriptionsController _sut;

        public PushSubscriptionsControllerTests()
        {
            _sut = new PushSubscriptionsController(null)
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
}
