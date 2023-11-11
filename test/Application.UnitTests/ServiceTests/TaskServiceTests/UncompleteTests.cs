using Core.Application.Contracts;
using Microsoft.Extensions.Logging;
using Moq;
using Sentry;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Contracts.Tasks;
using ToDoAssistant.Application.Contracts.Tasks.Models;
using ToDoAssistant.Application.Entities;
using ToDoAssistant.Application.Mappings;
using ToDoAssistant.Application.Services;
using Xunit;

namespace Application.UnitTests.ServiceTests.TaskServiceTests;

public class UncompleteTests
{
    private readonly Mock<ITasksRepository> _tasksRepositoryMock = new();
    private readonly Mock<ISpan> _metricsSpanMock = new();
    private readonly ITaskService _sut;

    public UncompleteTests()
    {
        _metricsSpanMock.Setup(x => x.StartChild(It.IsAny<string>())).Returns(new Mock<ISpan>().Object);

        _sut = new TaskService(
            new Mock<IUserService>().Object,
            new Mock<IListService>().Object,
            _tasksRepositoryMock.Object,
            new Mock<IListsRepository>().Object,
            MapperMocker.GetMapper<ToDoAssistantProfile>(),
            new Mock<ILogger<TaskService>>().Object);
    }

    [Fact]
    public async Task DoesNothing_IfTaskAlreadyUncompleted()
    {
        _tasksRepositoryMock.Setup(x => x.Exists(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(true);

        _tasksRepositoryMock.Setup(x => x.Get(It.IsAny<int>()))
            .Returns(new ToDoTask { IsCompleted = false });

        await _sut.UncompleteAsync(new CompleteUncomplete { Id = 0, UserId = 0 }, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        _tasksRepositoryMock.Verify(x => x.UncompleteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
