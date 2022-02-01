using System.Threading.Tasks;
using Api.Controllers.Accountant;
using Microsoft.AspNetCore.Mvc;
using Api.UnitTests.Builders;
using Xunit;

namespace Api.UnitTests.Controllers.Accountant
{
    public class AccountsControllerTests
    {
        private readonly AccountsController _sut;

        public AccountsControllerTests()
        {
            _sut = new AccountsController(null)
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
