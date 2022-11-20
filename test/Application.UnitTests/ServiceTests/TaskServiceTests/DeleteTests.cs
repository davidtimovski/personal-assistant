using Domain.ToDoAssistant;
using Moq;
using ToDoAssistant.Application.Contracts.Tasks;
using ToDoAssistant.Application.Mappings;
using ToDoAssistant.Application.Services;
using Xunit;

namespace Application.UnitTests.ServiceTests.TaskServiceTests;

public class DeleteTests
{
    private readonly Mock<ITasksRepository> _tasksRepositoryMock = new();
    private readonly ITaskService _sut;

    public DeleteTests()
    {
        _sut = new TaskService(
            null,
            null,
            _tasksRepositoryMock.Object,
            null,
            MapperMocker.GetMapper<ToDoAssistantProfile>(),
            null);
    }

    [Fact]
    public async Task DoesNothing_IfTaskAlreadyDeleted()
    {
        _tasksRepositoryMock.Setup(x => x.Get(It.IsAny<int>()))
            .Returns((ToDoTask)null);

        await _sut.DeleteAsync(It.IsAny<int>(), It.IsAny<int>());

        _tasksRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }
}
