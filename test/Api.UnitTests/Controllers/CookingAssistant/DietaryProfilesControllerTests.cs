using System.Threading.Tasks;
using Api.Controllers.CookingAssistant;
using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Api.UnitTests.Builders;
using Xunit;

namespace PersonalAssistant.Api.UnitTests.Controllers.CookingAssistant
{
    public class DietaryProfilesControllerTests
    {
        private readonly DietaryProfilesController _sut;

        public DietaryProfilesControllerTests()
        {
            _sut = new DietaryProfilesController(null, null, null)
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
        public async Task Update_Returns400_IfBodyMissing()
        {
            var result = await _sut.Update(null);
            Assert.IsType<BadRequestResult>(result);
        }
    }
}
