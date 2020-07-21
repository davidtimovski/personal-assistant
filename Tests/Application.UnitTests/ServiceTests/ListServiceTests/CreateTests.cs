using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Moq;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Application.Services.ToDoAssistant;
using PersonalAssistant.Application.UnitTests.Builders;
using PersonalAssistant.Domain.Entities.ToDoAssistant;
using Xunit;

namespace PersonalAssistant.Application.UnitTests.ServiceTests.ListServiceTests
{
    public class CreateTests
    {
        private readonly Mock<IValidator<CreateList>> _successfulValidatorMock;
        private readonly Mock<IListsRepository> _listsRepositoryMock = new Mock<IListsRepository>();
        private readonly IListService _sut;

        public CreateTests()
        {
            _successfulValidatorMock = ValidatorMocker.GetSuccessful<CreateList>();

            _sut = new ListService(
                new Mock<IUserService>().Object,
                _listsRepositoryMock.Object,
                new Mock<INotificationsRepository>().Object,
                MapperMocker.GetMapper<ToDoAssistantProfile>());
        }

        [Fact]
        public async Task ValidatesModel()
        {
            CreateList model = new ListBuilder().BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);

            _successfulValidatorMock.Verify(x => x.Validate(model), Times.Once);
        }

        [Fact]
        public async Task ValidateThrowsIfInvalidModel()
        {
            CreateList model = new ListBuilder().BuildCreateModel();
            var failedValidator = ValidatorMocker.GetFailed<CreateList>();

            await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(model, failedValidator.Object));
        }

        [Fact]
        public async Task TrimsName()
        {
            string actualName = null;
            _listsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoList>()))
                .Callback<ToDoList>((l) => actualName = l.Name);

            CreateList model = new ListBuilder().WithName(" List name ").BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);
            const string expected = "List name";

            Assert.Equal(expected, actualName);
        }

        [Theory]
        [InlineData("Task 1\nTask 2", 2)]
        [InlineData("Task 1\n\nTask 2\nTask 3", 3)]
        public async Task SplitsTextIntoTasksByNewline(string tasksText, int expectedTaskCount)
        {
            var resultingTasks = new List<ToDoTask>();
            _listsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoList>()))
                .Callback<ToDoList>((l) => resultingTasks = l.Tasks);

            CreateList model = new ListBuilder().WithTasksText(tasksText).BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);

            _listsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<ToDoList>()), Times.Once);

            Assert.Equal(expectedTaskCount, resultingTasks.Count);
        }

        [Theory]
        [InlineData("Task 1 \nTask 2")]
        [InlineData("Task 1\n\n Task 2")]
        public async Task SplitsTextIntoTasksWithTrimmedNames(string tasksText)
        {
            CreateList model = new ListBuilder().WithTasksText(tasksText).BuildCreateModel();

            var resultingTasks = new List<ToDoTask>();
            _listsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoList>()))
                .Callback<ToDoList>((l) => resultingTasks = l.Tasks);

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);
            var expectedTasks = new List<ToDoTask>
            {
                new ToDoTask { Name = "Task 1" },
                new ToDoTask { Name = "Task 2" }
            };

            _listsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<ToDoList>()), Times.Once);

            for (var i = 0; i < expectedTasks.Count; i++)
            {
                Assert.Equal(expectedTasks[i].Name, resultingTasks[i].Name);
            }
        }

        [Fact]
        public async Task ResultingTasksHaveIsOneTimeSetFromList()
        {
            var resultingTasks = new List<ToDoTask>();
            _listsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoList>()))
                .Callback<ToDoList>((l) => resultingTasks = l.Tasks);

            CreateList model = new ListBuilder().WithTasksText("Task 1\nTask 2").BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);

            _listsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<ToDoList>()), Times.Once);

            for (var i = 0; i < resultingTasks.Count; i++)
            {
                Assert.Equal(model.IsOneTimeToggleDefault, resultingTasks[i].IsOneTime);
            }
        }

        [Fact]
        public async Task SetsCreatedDate()
        {
            var actualCreatedDate = new DateTime();
            _listsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoList>()))
                .Callback<ToDoList>((l) => actualCreatedDate = l.CreatedDate);

            CreateList model = new ListBuilder().BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);

            Assert.NotEqual(DateTime.MinValue, actualCreatedDate);
        }

        [Fact]
        public async Task SetsModifiedDate()
        {
            var actualModifiedDate = new DateTime();
            _listsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoList>()))
                .Callback<ToDoList>((l) => actualModifiedDate = l.ModifiedDate);

            CreateList model = new ListBuilder().BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);

            Assert.NotEqual(DateTime.MinValue, actualModifiedDate);
        }
    }
}
