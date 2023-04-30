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
    private readonly Mock<ISpan> _metricsSpanMock = new();
    private readonly ITaskService _sut;

    public DeleteTests()
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
    public async Task DoesNothing_IfTaskAlreadyDeleted()
    {
        _tasksRepositoryMock.Setup(x => x.Get(It.IsAny<int>()))
            .Returns((ToDoTask)null);

        await _sut.DeleteAsync(It.IsAny<int>(), It.IsAny<int>(), _metricsSpanMock.Object);

        _tasksRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()), Times.Never);
    }
}
