using System.Threading.Tasks;
using Api.Controllers.Accountant;
using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Api.UnitTests.Builders;
using Xunit;

namespace PersonalAssistant.Api.UnitTests.Controllers.Accountant
{
    public class DebtsControllerTests
    {
        private readonly DebtsController _sut;

        public DebtsControllerTests()
        {
            _sut = new DebtsController(null)
            {
                ControllerContext = new ControllerContextBuilder().Build()
            };
        }

        [Fact]
        public async Task Create_Returns400_IfBodyMissing()
        {
            var result = await _sut.Create(null);
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
