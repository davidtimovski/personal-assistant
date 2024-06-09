using Api.UnitTests.Builders;
using Core.Application.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ToDoAssistant.Api.Controllers;
using ToDoAssistant.Api.Models;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Contracts.Lists.Models;
using ToDoAssistant.Application.Contracts.Notifications;
using Xunit;

namespace Api.UnitTests.Controllers.ToDoAssistant;

public class ListsControllerTests
{
    private readonly Mock<IUserIdLookup> _userIdLookupMock = new();
    private readonly Mock<IListService> _listServiceMock = new();
    private readonly Mock<ISpan> _metricsSpanMock = new();
    private readonly ListsController _sut;

    public ListsControllerTests()
    {
        _userIdLookupMock.Setup(x => x.Contains(It.IsAny<string>())).Returns(true);
        _userIdLookupMock.Setup(x => x.Get(It.IsAny<string>())).Returns(1);

        _sut = new ListsController(
            _userIdLookupMock.Object,
            new Mock<IUsersRepository>().Object,
            _listServiceMock.Object,
            new Mock<INotificationService>().Object,
            new Mock<ISenderService>().Object,
            new Mock<IUserService>().Object,
            new Mock<IValidator<CreateList>>().Object,
            new Mock<IValidator<UpdateList>>().Object,
            new Mock<IValidator<UpdateSharedList>>().Object,
            new Mock<IValidator<ShareList>>().Object,
            new Mock<IValidator<CopyList>>().Object,
            new Mock<IStringLocalizer<ListsController>>().Object,
            new Mock<IOptions<AppConfiguration>>().Object,
            new Mock<ILogger<ListsController>>().Object,
            new Mock<IStringLocalizer<BaseController>>().Object)
        {
            ControllerContext = new ControllerContextBuilder().Build()
        };
    }

    [Fact]
    public void Get_Returns404_IfNotFound()
    {
        _listServiceMock.Setup(x => x.GetForEdit(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns(new Result<EditListDto?>(null));

        var result = _sut.Get(It.IsAny<int>());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetWithShares_Returns404_IfNotFound()
    {
        _listServiceMock.Setup(x => x.GetWithShares(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns(new Result<ListWithShares?>(null));

        var result = _sut.GetWithShares(It.IsAny<int>());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_Returns400_IfBodyMissing()
    {
        var result = await _sut.Create(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Update_Returns400_IfBodyMissing()
    {
        var result = await _sut.Update(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task UpdateShared_Returns400_IfBodyMissing()
    {
        var result = await _sut.UpdateShared(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Share_Returns400_IfBodyMissing()
    {
        var result = await _sut.Share(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Copy_Returns400_IfBodyMissing()
    {
        var result = await _sut.Copy(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task SetIsArchived_Returns400_IfBodyMissing()
    {
        var result = await _sut.SetIsArchived(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task UncompleteAll_Returns400_IfBodyMissing()
    {
        var result = await _sut.UncompleteAll(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task SetShareIsAccepted_Returns400_IfBodyMissing()
    {
        var result = await _sut.SetShareIsAccepted(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }

    //[Fact]
    //public async Task ReorderReturns_BadRequest_IfBodyMissing()
    //{
    //    var result = await _sut.Reorder(null);
    //    Assert.IsType<BadRequestResult>(result);
    //}
}
