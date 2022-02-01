using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Moq;
using Application.Contracts.ToDoAssistant.Lists;
using Application.Contracts.ToDoAssistant.Tasks;
using Application.Contracts.ToDoAssistant.Tasks.Models;
using Application.Mappings;
using Application.Services.ToDoAssistant;
using Application.UnitTests.Builders;
using Domain.Entities.ToDoAssistant;
using Xunit;

namespace Application.UnitTests.ServiceTests.TaskServiceTests
{
    public class CreateTests
    {
        private readonly Mock<IValidator<CreateTask>> _successfulValidatorMock;
        private readonly Mock<ITasksRepository> _tasksRepositoryMock = new();
        private readonly Mock<IListsRepository> _listsRepositoryMock = new();
        private readonly ITaskService _sut;

        public CreateTests()
        {
            _successfulValidatorMock = ValidatorMocker.GetSuccessful<CreateTask>();

            _listsRepositoryMock.Setup(x => x.GetWithShares(It.IsAny<int>(), It.IsAny<int>())).Returns(new ToDoList
            {
                Shares = new List<ListShare>()
            });

            _sut = new TaskService(
                null,
                new Mock<IListService>().Object,
                _tasksRepositoryMock.Object,
                _listsRepositoryMock.Object,
                MapperMocker.GetMapper<ToDoAssistantProfile>());
        }

        [Fact]
        public async Task ValidatesModel()
        {
            CreateTask model = new TaskBuilder().BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);

            _successfulValidatorMock.Verify(x => x.Validate(model));
        }

        [Fact]
        public async Task Validate_Throws_IfInvalidModel()
        {
            CreateTask model = new TaskBuilder().BuildCreateModel();
            var failedValidator = ValidatorMocker.GetFailed<CreateTask>();

            await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(model, failedValidator.Object));
        }

        [Fact]
        public async Task TrimsName()
        {
            string actualName = null;
            _tasksRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoTask>(), It.IsAny<int>()))
                .Callback<ToDoTask, int>((t, i) => actualName = t.Name);

            CreateTask model = new TaskBuilder().WithName(" Task name ").BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);
            const string expected = "Task name";

            Assert.Equal(expected, actualName);
        }

        [Fact]
        public async Task SetsCreatedDate()
        {
            var actualCreatedDate = new DateTime();
            _tasksRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoTask>(), It.IsAny<int>()))
                .Callback<ToDoTask, int>((l, u) => actualCreatedDate = l.CreatedDate);

            CreateTask model = new TaskBuilder().BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);

            Assert.NotEqual(DateTime.MinValue, actualCreatedDate);
        }

        [Fact]
        public async Task SetsModifiedDate()
        {
            var actualModifiedDate = new DateTime();
            _tasksRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoTask>(), It.IsAny<int>()))
                .Callback<ToDoTask, int>((l, u) => actualModifiedDate = l.ModifiedDate);

            CreateTask model = new TaskBuilder().BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);

            Assert.NotEqual(DateTime.MinValue, actualModifiedDate);
        }
    }
}
