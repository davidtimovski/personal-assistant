using Core.Application.Contracts;
using Microsoft.Extensions.Logging;
using Moq;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Contracts.Notifications;
using ToDoAssistant.Application.Contracts.Tasks;
using ToDoAssistant.Application.Mappings;
using ToDoAssistant.Application.Services;
using Xunit;
using User = Core.Application.Entities.User;

namespace Application.UnitTests.ServiceTests.ListServiceTests;

public class NotificationTests
{
    private readonly Mock<IListsRepository> _listsRepositoryMock = new();
    private readonly Mock<ITasksRepository> _tasksRepositoryMock = new();
    private readonly Mock<ISpan> _metricsSpanMock = new();
    private readonly IListService _sut;

    public NotificationTests()
    {
        _metricsSpanMock.Setup(x => x.StartChild(It.IsAny<string>())).Returns(new Mock<ISpan>().Object);

        _sut = new ListService(
            new Mock<IUserService>().Object,
            _listsRepositoryMock.Object,
            _tasksRepositoryMock.Object,
            new Mock<INotificationsRepository>().Object,
            MapperMocker.GetMapper<ToDoAssistantProfile>(),
            new Mock<ILogger<ListService>>().Object);
    }

    [Fact]
    public void GetUsersToBeNotifiedOfChange_ReturnsEmptyList_IfIsPrivateIsTrue()
    {
        const bool isPrivate = true;

        _listsRepositoryMock.Setup(x => x.GetUsersToBeNotifiedOfChange(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns(new List<User> { new() });

        var result = _sut.GetUsersToBeNotifiedOfChange(It.IsAny<int>(), It.IsAny<int>(), isPrivate, _metricsSpanMock.Object);

        Assert.Empty(result.Data!);
    }

    [Fact]
    public void GetUsersToBeNotifiedOfChange_ReturnsEmptyList_IfIsPrivateCheckReturnsTrue()
    {
        const bool isPrivate = true;

        _tasksRepositoryMock.Setup(x => x.IsPrivate(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(isPrivate);
        _listsRepositoryMock.Setup(x => x.GetUsersToBeNotifiedOfChange(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns(new List<User> { new() });

        var result = _sut.GetUsersToBeNotifiedOfChange(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), _metricsSpanMock.Object);

        Assert.Empty(result.Data!);
    }
}
