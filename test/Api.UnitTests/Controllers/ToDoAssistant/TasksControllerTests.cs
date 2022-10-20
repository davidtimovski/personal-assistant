using System.Threading.Tasks;
using Api.Config;
using Api.Controllers.ToDoAssistant;
using Api.UnitTests.Builders;
using Application.Contracts.Common;
using Application.Contracts.ToDoAssistant.Tasks;
using Application.Contracts.ToDoAssistant.Tasks.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Api.UnitTests.Controllers.ToDoAssistant;

public class TasksControllerTests
{
    private readonly Mock<IUserIdLookup> _userIdLookupMock = new();
    private readonly Mock<ITaskService> _taskServiceMock = new();
    private readonly TasksController _sut;

    public TasksControllerTests()
    {
        _sut = new TasksController(
            _userIdLookupMock.Object, null, null,
            _taskServiceMock.Object,
            null, null, null, null, null, null,
            new Mock<IOptions<Urls>>().Object, null)
        {
            ControllerContext = new ControllerContextBuilder().Build()
        };
    }

    [Fact]
    public void Get_Returns404_IfNotFound()
    {
        _userIdLookupMock.Setup(x => x.Contains(It.IsAny<string>())).Returns(true);
        _userIdLookupMock.Setup(x => x.Get(It.IsAny<string>())).Returns(1);
        _taskServiceMock.Setup(x => x.Get(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((TaskDto)null);

        var result = _sut.Get(It.IsAny<int>());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetForUpdate_Returns404_IfNotFound()
    {
        _userIdLookupMock.Setup(x => x.Contains(It.IsAny<string>())).Returns(true);
        _userIdLookupMock.Setup(x => x.Get(It.IsAny<string>())).Returns(1);
        _taskServiceMock.Setup(x => x.GetForUpdate(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((TaskForUpdate)null);

        var result = _sut.GetForUpdate(It.IsAny<int>());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_Returns400_IfBodyMissing()
    {
        var result = await _sut.Create(null);
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task BulkCreate_Returns400_IfBodyMissing()
    {
        var result = await _sut.BulkCreate(null);
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Update_Returns400_IfBodyMissing()
    {
        var result = await _sut.Update(null);
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Complete_Returns400_IfBodyMissing()
    {
        var result = await _sut.Complete(null);
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Uncomplete_Returns400_IfBodyMissing()
    {
        var result = await _sut.Uncomplete(null);
        Assert.IsType<BadRequestResult>(result);
    }

    //[Fact]
    //public async Task Reorder_Returns400_IfBodyMissing()
    //{
    //    var result = await _sut.Reorder(null);
    //    Assert.IsType<BadRequestResult>(result);
    //}
}
