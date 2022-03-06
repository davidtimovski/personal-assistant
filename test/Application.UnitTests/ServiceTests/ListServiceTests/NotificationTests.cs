using System.Collections.Generic;
using Moq;
using Application.Contracts.ToDoAssistant.Lists;
using Application.Contracts.ToDoAssistant.Tasks;
using Application.Mappings;
using Application.Services.ToDoAssistant;
using Domain.Entities.Common;
using Xunit;

namespace Application.UnitTests.ServiceTests.ListServiceTests;

public class NotificationTests
{
    private readonly Mock<IListsRepository> _listsRepositoryMock = new();
    private readonly Mock<ITasksRepository> _tasksRepositoryMock = new();
    private readonly IListService _sut;

    public NotificationTests()
    {
        _sut = new ListService(
            null,
            _listsRepositoryMock.Object,
            _tasksRepositoryMock.Object,
            null,
            MapperMocker.GetMapper<ToDoAssistantProfile>());
    }

    [Fact]
    public void GetUsersToBeNotifiedOfChange_ReturnsEmptyList_IfIsPrivateIsTrue()
    {
        const bool isPrivate = true;

        _listsRepositoryMock.Setup(x => x.GetUsersToBeNotifiedOfChange(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new List<User> { new() });

        IEnumerable<User> result = _sut.GetUsersToBeNotifiedOfChange(It.IsAny<int>(), It.IsAny<int>(), isPrivate);

        Assert.Empty(result);
    }

    [Fact]
    public void GetUsersToBeNotifiedOfChange_ReturnsEmptyList_IfIsPrivateCheckReturnsTrue()
    {
        const bool isPrivate = true;

        _tasksRepositoryMock.Setup(x => x.IsPrivate(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(isPrivate);
        _listsRepositoryMock.Setup(x => x.GetUsersToBeNotifiedOfChange(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new List<User> { new() });

        IEnumerable<User> result = _sut.GetUsersToBeNotifiedOfChange(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>());

        Assert.Empty(result);
    }
}
