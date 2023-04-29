using Application.Domain.ToDoAssistant;
using Moq;
using Sentry;
using ToDoAssistant.Application.Contracts.Tasks;
using ToDoAssistant.Application.Mappings;
using ToDoAssistant.Application.Services;
using Xunit;

namespace Application.UnitTests.ServiceTests.TaskServiceTests;

public class DeleteTests
{
    private readonly Mock<ITasksRepository> _tasksRepositoryMock = new();
    private readonly Mock<ITransaction> _sentryTr = new();
    private readonly ITaskService _sut;

    public DeleteTests()
    {
        _sentryTr.Setup(x => x.StartChild(It.IsAny<string>())).Returns(new Mock<ISpan>().Object);

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

        await _sut.DeleteAsync(It.IsAny<int>(), It.IsAny<int>(), _sentryTr.Object);

        _tasksRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ITransaction>()), Times.Never);
    }
}
