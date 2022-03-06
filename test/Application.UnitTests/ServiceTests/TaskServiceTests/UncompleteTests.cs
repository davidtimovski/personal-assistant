using System.Threading.Tasks;
using Moq;
using Application.Contracts.ToDoAssistant.Tasks;
using Application.Contracts.ToDoAssistant.Tasks.Models;
using Application.Mappings;
using Application.Services.ToDoAssistant;
using Domain.Entities.ToDoAssistant;
using Xunit;

namespace Application.UnitTests.ServiceTests.TaskServiceTests;

public class UncompleteTests
{
    private readonly Mock<ITasksRepository> _tasksRepositoryMock = new();
    private readonly ITaskService _sut;

    public UncompleteTests()
    {
        _sut = new TaskService(
            null,
            null,
            _tasksRepositoryMock.Object,
            null,
            MapperMocker.GetMapper<ToDoAssistantProfile>());
    }

    [Fact]
    public async Task DoesNothing_IfTaskAlreadyUncompleted()
    {
        _tasksRepositoryMock.Setup(x => x.Exists(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(true);

        _tasksRepositoryMock.Setup(x => x.Get(It.IsAny<int>()))
            .Returns(new ToDoTask { IsCompleted = false });

        await _sut.UncompleteAsync(new CompleteUncomplete());

        _tasksRepositoryMock.Verify(x => x.UncompleteAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }
}
