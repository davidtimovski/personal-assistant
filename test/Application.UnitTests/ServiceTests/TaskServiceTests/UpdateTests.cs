using System.Threading.Tasks;
using FluentValidation;
using Moq;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Application.Services.ToDoAssistant;
using PersonalAssistant.Application.UnitTests.Builders;
using PersonalAssistant.Domain.Entities.ToDoAssistant;
using Xunit;

namespace PersonalAssistant.Application.UnitTests.ServiceTests.TaskServiceTests
{
    public class UpdateTests
    {
        private readonly Mock<IValidator<UpdateTask>> _successfulValidatorMock;
        private readonly Mock<ITasksRepository> _tasksRepositoryMock = new Mock<ITasksRepository>();
        private readonly ITaskService _sut;

        public UpdateTests()
        {
            _successfulValidatorMock = ValidatorMocker.GetSuccessful<UpdateTask>();

            _sut = new TaskService(
                _tasksRepositoryMock.Object,
                new Mock<IListService>().Object,
                MapperMocker.GetMapper<ToDoAssistantProfile>());
        }

        [Fact]
        public async Task ValidatesModel()
        {
            UpdateTask model = new TaskBuilder().BuildUpdateModel();

            await _sut.UpdateAsync(model, _successfulValidatorMock.Object);

            _successfulValidatorMock.Verify(x => x.Validate(model));
        }

        [Fact]
        public async Task ValidateThrowsIfInvalidModel()
        {
            UpdateTask model = new TaskBuilder().BuildUpdateModel();
            var failedValidator = ValidatorMocker.GetFailed<UpdateTask>();

            await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateAsync(model, failedValidator.Object));
        }

        [Fact]
        public async Task TrimsName()
        {
            string actualName = null;
            _tasksRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ToDoTask>(), It.IsAny<int>()))
                .Callback<ToDoTask, int>((t, i) => actualName = t.Name);

            UpdateTask model = new TaskBuilder().WithName(" Task name ").BuildUpdateModel();

            await _sut.UpdateAsync(model, _successfulValidatorMock.Object);
            const string expected = "Task name";

            Assert.Equal(expected, actualName);
        }
    }
}
