using System.Threading.Tasks;
using Moq;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Application.Services.ToDoAssistant;
using PersonalAssistant.Domain.Entities.ToDoAssistant;
using Xunit;

namespace PersonalAssistant.Application.UnitTests.ServiceTests.TaskServiceTests
{
    public class UncompleteTests
    {
        private readonly Mock<ITasksRepository> _tasksRepositoryMock = new Mock<ITasksRepository>();
        private readonly ITaskService _sut;

        public UncompleteTests()
        {
            _sut = new TaskService(
                _tasksRepositoryMock.Object,
                new Mock<IListService>().Object,
                MapperMocker.GetMapper<ToDoAssistantProfile>());
        }

        [Fact]
        public async Task DoesNothingIfTaskAlreadyUncompleted()
        {
            _tasksRepositoryMock.Setup(x => x.Exists(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(true);

            _tasksRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(new ToDoTask { IsCompleted = false });

            await _sut.UncompleteAsync(new CompleteUncomplete());

            _tasksRepositoryMock.Verify(x => x.UncompleteAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
    }
}
