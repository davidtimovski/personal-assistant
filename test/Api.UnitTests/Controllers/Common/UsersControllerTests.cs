using System.Threading.Tasks;
using Api.Controllers.Common;
using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Api.UnitTests.Builders;
using Xunit;

namespace PersonalAssistant.Api.UnitTests.Controllers.Common
{
    public class UsersControllerTests
    {
        private readonly UsersController _sut;

        public UsersControllerTests()
        {
            _sut = new UsersController(null, null)
            {
                ControllerContext = new ControllerContextBuilder().Build()
            };
        }

        [Fact]
        public async Task UpdateToDoNotificationsEnabled_Returns400_IfBodyMissing()
        {
            var result = await _sut.UpdateToDoNotificationsEnabled(null);
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task UpdateCookingNotificationsEnabled_Returns400_IfBodyMissing()
        {
            var result = await _sut.UpdateCookingNotificationsEnabled(null);
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task UpdateImperialSystem_Returns400_IfBodyMissing()
        {
            var result = await _sut.UpdateImperialSystem(null);
            Assert.IsType<BadRequestResult>(result);
        }
    }
}
