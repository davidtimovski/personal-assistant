using System.Threading.Tasks;
using Moq;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Application.Services.ToDoAssistant;
using PersonalAssistant.Domain.Entities.ToDoAssistant;
using Xunit;

namespace PersonalAssistant.Application.UnitTests.ServiceTests.TaskServiceTests
{
    public class DeleteTests
    {
        private readonly Mock<ITasksRepository> _tasksRepositoryMock = new Mock<ITasksRepository>();
        private readonly ITaskService _sut;

        public DeleteTests()
        {
            _sut = new TaskService(
                new Mock<IUserService>().Object,
                new Mock<IListService>().Object,
                _tasksRepositoryMock.Object,
                new Mock<IListsRepository>().Object,
                MapperMocker.GetMapper<ToDoAssistantProfile>());
        }

        [Fact]
        public async Task DoesNothingIfTaskAlreadyDeleted()
        {
            _tasksRepositoryMock.Setup(x => x.Get(It.IsAny<int>()))
                .Returns((ToDoTask)null);

            await _sut.DeleteAsync(It.IsAny<int>(), It.IsAny<int>());

            _tasksRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
    }
}
