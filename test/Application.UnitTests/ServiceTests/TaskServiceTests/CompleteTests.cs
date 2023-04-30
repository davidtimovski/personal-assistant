using Application.Domain.ToDoAssistant;
using Moq;
using Sentry;
using ToDoAssistant.Application.Contracts.Tasks;
using ToDoAssistant.Application.Contracts.Tasks.Models;
using ToDoAssistant.Application.Mappings;
using ToDoAssistant.Application.Services;
using Xunit;

namespace Application.UnitTests.ServiceTests.TaskServiceTests;

public class CompleteTests
{
    private readonly Mock<ITasksRepository> _tasksRepositoryMock = new();
    private readonly Mock<ISpan> _metricsSpanMock = new();
    private readonly ITaskService _sut;

    public CompleteTests()
    {
        _metricsSpanMock.Setup(x => x.StartChild(It.IsAny<string>())).Returns(new Mock<ISpan>().Object);

        _sut = new TaskService(
            null,
            null,
            _tasksRepositoryMock.Object,
            null,
            MapperMocker.GetMapper<ToDoAssistantProfile>(),
            null);
    }

    [Fact]
    public async Task DoesNothing_IfTaskAlreadyCompleted()
    {
        _tasksRepositoryMock.Setup(x => x.Exists(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(true);

        _tasksRepositoryMock.Setup(x => x.Get(It.IsAny<int>()))
            .Returns(new ToDoTask { IsCompleted = true });

        await _sut.CompleteAsync(new CompleteUncomplete(), _metricsSpanMock.Object);

        _tasksRepositoryMock.Verify(x => x.CompleteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()), Times.Never);
    }
}
