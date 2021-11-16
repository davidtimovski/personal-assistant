using System.Threading.Tasks;
using Api.Config;
using Api.Controllers.ToDoAssistant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using PersonalAssistant.Api.UnitTests.Builders;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models;
using Xunit;

namespace PersonalAssistant.Api.UnitTests.Controllers.ToDoAssistant
{
    public class ListsControllerTests
    {
        private readonly Mock<IListService> _listServiceMock = new Mock<IListService>();
        private readonly ListsController _sut;

        public ListsControllerTests()
        {
            _sut = new ListsController(
                _listServiceMock.Object,
                null, null, null, null, null, null, null, null, null,
                new Mock<IOptions<Urls>>().Object)
            {
                ControllerContext = new ControllerContextBuilder().Build()
            };
        }

        [Fact]
        public void Get_Returns404_IfNotFound()
        {
            _listServiceMock.Setup(x => x.Get(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((EditListDto)null);

            var result = _sut.Get(It.IsAny<int>());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetWithShares_Returns404_IfNotFound()
        {
            _listServiceMock.Setup(x => x.GetWithShares(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((ListWithShares)null);

            var result = _sut.GetWithShares(It.IsAny<int>());

            Assert.IsType<NotFoundResult>(result);
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

        [Fact]
        public async Task UpdateShared_Returns400_IfBodyMissing()
        {
            var result = await _sut.UpdateShared(null);
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Share_Returns400_IfBodyMissing()
        {
            var result = await _sut.Share(null);
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Copy_Returns400_IfBodyMissing()
        {
            var result = await _sut.Copy(null);
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task SetIsArchived_Returns400_IfBodyMissing()
        {
            var result = await _sut.SetIsArchived(null);
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task SetTasksAsNotCompleted_Returns400_IfBodyMissing()
        {
            var result = await _sut.SetTasksAsNotCompleted(null);
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task SetShareIsAccepted_Returns400_IfBodyMissing()
        {
            var result = await _sut.SetShareIsAccepted(null);
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task ReorderReturns_BadRequest_IfBodyMissing()
        {
            var result = await _sut.Reorder(null);
            Assert.IsType<BadRequestResult>(result);
        }
    }
}
