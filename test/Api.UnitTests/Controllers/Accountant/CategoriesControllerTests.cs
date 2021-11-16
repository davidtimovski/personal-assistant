using System.Threading.Tasks;
using Api.Controllers.Accountant;
using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Api.UnitTests.Builders;
using Xunit;

namespace PersonalAssistant.Api.UnitTests.Controllers.Accountant
{
    public class CategoriesControllerTests
    {
        private readonly CategoriesController _sut;

        public CategoriesControllerTests()
        {
            _sut = new CategoriesController(null)
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
