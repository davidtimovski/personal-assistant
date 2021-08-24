using System.Threading.Tasks;
using Moq;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Application.Services.ToDoAssistant;
using PersonalAssistant.Domain.Entities.ToDoAssistant;
using Xunit;

namespace PersonalAssistant.Application.UnitTests.ServiceTests.TaskServiceTests
{
    public class CompleteTests
    {
        private readonly Mock<ITasksRepository> _tasksRepositoryMock = new Mock<ITasksRepository>();
        private readonly ITaskService _sut;

        public CompleteTests()
        {
            _sut = new TaskService(
                new Mock<IUserService>().Object,
                new Mock<IListService>().Object,
                _tasksRepositoryMock.Object,
                new Mock<IListsRepository>().Object,
                MapperMocker.GetMapper<ToDoAssistantProfile>());
        }

        [Fact]
        public async Task DoesNothingIfTaskAlreadyCompleted()
        {
            _tasksRepositoryMock.Setup(x => x.Exists(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(true);

            _tasksRepositoryMock.Setup(x => x.Get(It.IsAny<int>()))
                .Returns(new ToDoTask { IsCompleted = true });

            await _sut.CompleteAsync(new CompleteUncomplete());

            _tasksRepositoryMock.Verify(x => x.CompleteAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
    }
}
