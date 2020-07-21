using System;
using System.Collections.Generic;
using System.Linq;
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
    public class BulkCreateTests
    {
        private readonly Mock<IValidator<BulkCreate>> _successfulValidatorMock;
        private readonly Mock<ITasksRepository> _tasksRepositoryMock = new Mock<ITasksRepository>();
        private readonly Mock<IListService> _listServiceMock = new Mock<IListService>();
        private readonly ITaskService _sut;

        public BulkCreateTests()
        {
            _successfulValidatorMock = ValidatorMocker.GetSuccessful<BulkCreate>();

            _sut = new TaskService(
                _tasksRepositoryMock.Object,
                _listServiceMock.Object,
                MapperMocker.GetMapper<ToDoAssistantProfile>());
        }

        [Fact]
        public async Task ValidatesModel()
        {
            BulkCreate model = new TaskBuilder().BuildBulkCreateModel();

            await _sut.BulkCreateAsync(model, _successfulValidatorMock.Object);

            _successfulValidatorMock.Verify(x => x.Validate(model), Times.Once);
        }

        [Fact]
        public async Task ValidateThrowsIfInvalidModel()
        {
            BulkCreate model = new TaskBuilder().BuildBulkCreateModel();
            var failedValidator = ValidatorMocker.GetFailed<BulkCreate>();

            await Assert.ThrowsAsync<ValidationException>(() => _sut.BulkCreateAsync(model, failedValidator.Object));
        }

        [Theory]
        [InlineData("Task 1 \nTask 2", 2)]
        [InlineData("Task 1\n\nTask 2\nTask 3", 3)]
        public async Task SplitsTextIntoTasksByNewline(string tasksText, int expectedTaskCount)
        {
            var actualTasks = new List<ToDoTask>();
            _tasksRepositoryMock.Setup(x => x.BulkCreateAsync(It.IsAny<IEnumerable<ToDoTask>>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Callback<IEnumerable<ToDoTask>, bool, int>((t, p, u) => actualTasks = t.ToList());

            BulkCreate model = new TaskBuilder().WithTasksText(tasksText).BuildBulkCreateModel();

            await _sut.BulkCreateAsync(model, _successfulValidatorMock.Object);

            _tasksRepositoryMock.Verify(x => x.BulkCreateAsync(
                It.IsAny<List<ToDoTask>>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Once);

            Assert.Equal(expectedTaskCount, actualTasks.Count);
        }

        [Theory]
        [InlineData("Task 1 \nTask 2", 1)]
        [InlineData("Task 1\n\n Task 2", 2)]
        public async Task SplitsTextIntoTasksWithListIdSet(string tasksText, int listId)
        {
            var actualTasks = new List<ToDoTask>();
            _tasksRepositoryMock.Setup(x => x.BulkCreateAsync(It.IsAny<IEnumerable<ToDoTask>>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Callback<IEnumerable<ToDoTask>, bool, int>((t, p, u) => actualTasks = t.ToList());

            BulkCreate model = new TaskBuilder()
                .WithListId(listId)
                .WithTasksText(tasksText)
                .BuildBulkCreateModel();

            await _sut.BulkCreateAsync(model, _successfulValidatorMock.Object);

            _tasksRepositoryMock.Verify(x => x.BulkCreateAsync(
                It.IsAny<List<ToDoTask>>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Once);

            for (var i = 0; i < actualTasks.Count; i++)
            {
                Assert.Equal(listId, actualTasks[i].ListId);
            }
        }

        [Theory]
        [InlineData("Task 1 \nTask 2")]
        [InlineData("Task 1\n\n Task 2")]
        public async Task SplitsTextIntoTasksWithTrimmedNames(string tasksText)
        {
            var actualTasks = new List<ToDoTask>();
            _tasksRepositoryMock.Setup(x => x.BulkCreateAsync(It.IsAny<IEnumerable<ToDoTask>>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Callback<IEnumerable<ToDoTask>, bool, int>((t, p, u) => actualTasks = t.ToList());

            BulkCreate model = new TaskBuilder().WithTasksText(tasksText).BuildBulkCreateModel();

            await _sut.BulkCreateAsync(model, _successfulValidatorMock.Object);
            var expectedTasks = new List<ToDoTask>
            {
                new ToDoTask { Name = "Task 1" },
                new ToDoTask { Name = "Task 2" }
            };

            _tasksRepositoryMock.Verify(x => x.BulkCreateAsync(
                It.IsAny<List<ToDoTask>>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Once);

            for (var i = 0; i < expectedTasks.Count; i++)
            {
                Assert.Equal(expectedTasks[i].Name, actualTasks[i].Name);
            }
        }

        [Theory]
        [InlineData("Task 1 \nTask 2", true)]
        [InlineData("Task 1\n\n Task 2", false)]
        public async Task SplitsTextIntoTasksWithIsOneTimeSet(string tasksText, bool tasksAreOneTime)
        {
            var actualTasks = new List<ToDoTask>();
            _tasksRepositoryMock.Setup(x => x.BulkCreateAsync(It.IsAny<IEnumerable<ToDoTask>>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Callback<IEnumerable<ToDoTask>, bool, int>((t, p, u) => actualTasks = t.ToList());

            BulkCreate model = new TaskBuilder()
                .WithTasksText(tasksText)
                .WithTasksAreOneTime(tasksAreOneTime)
                .BuildBulkCreateModel();

            await _sut.BulkCreateAsync(model, _successfulValidatorMock.Object);

            _tasksRepositoryMock.Verify(x => x.BulkCreateAsync(
                It.IsAny<List<ToDoTask>>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Once);

            for (var i = 0; i < actualTasks.Count; i++)
            {
                Assert.Equal(tasksAreOneTime, actualTasks[i].IsOneTime);
            }
        }

        [Theory]
        [InlineData("Task 1 \nTask 2", true, 1)]
        [InlineData("Task 1\n\n Task 2", false, 2)]
        public async Task SplitsTextIntoTasksWithPrivateToUserIdSet(string tasksText, bool tasksArePrivate, int userId)
        {
            var actualTasks = new List<ToDoTask>();
            _tasksRepositoryMock.Setup(x => x.BulkCreateAsync(It.IsAny<IEnumerable<ToDoTask>>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Callback<IEnumerable<ToDoTask>, bool, int>((t, p, u) => actualTasks = t.ToList());

            BulkCreate model = new TaskBuilder()
                .WithUserId(userId)
                .WithTasksText(tasksText)
                .WithTasksArePrivate(tasksArePrivate)
                .BuildBulkCreateModel();

            await _sut.BulkCreateAsync(model, _successfulValidatorMock.Object);
            int? expectedPrivateToUserId = tasksArePrivate ? userId : (int?)null;

            _tasksRepositoryMock.Verify(x => x.BulkCreateAsync(
                It.IsAny<List<ToDoTask>>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Once);

            for (var i = 0; i < actualTasks.Count; i++)
            {
                Assert.Equal(expectedPrivateToUserId, actualTasks[i].PrivateToUserId);
            }
        }

        [Fact]
        public async Task ResultingTasksHaveCreatedDateSet()
        {
            var actualTasks = new List<ToDoTask>();
            _tasksRepositoryMock.Setup(x => x.BulkCreateAsync(It.IsAny<IEnumerable<ToDoTask>>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Callback<IEnumerable<ToDoTask>, bool, int>((t, p, u) => actualTasks = t.ToList());

            BulkCreate model = new TaskBuilder()
                .WithTasksText("Task 1\nTask 2")
                .BuildBulkCreateModel();

            await _sut.BulkCreateAsync(model, _successfulValidatorMock.Object);

            _tasksRepositoryMock.Verify(x => x.BulkCreateAsync(
                It.IsAny<List<ToDoTask>>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Once);

            for (var i = 0; i < actualTasks.Count; i++)
            {
                Assert.NotEqual(DateTime.MinValue, actualTasks[i].CreatedDate);
            }
        }

        [Fact]
        public async Task ResultingTasksHaveModifiedDateSet()
        {
            var actualTasks = new List<ToDoTask>();
            _tasksRepositoryMock.Setup(x => x.BulkCreateAsync(It.IsAny<IEnumerable<ToDoTask>>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Callback<IEnumerable<ToDoTask>, bool, int>((t, p, u) => actualTasks = t.ToList());

            BulkCreate model = new TaskBuilder()
                .WithTasksText("Task 1\nTask 2")
                .BuildBulkCreateModel();

            await _sut.BulkCreateAsync(model, _successfulValidatorMock.Object);

            _tasksRepositoryMock.Verify(x => x.BulkCreateAsync(
                It.IsAny<List<ToDoTask>>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Once);

            for (var i = 0; i < actualTasks.Count; i++)
            {
                Assert.NotEqual(DateTime.MinValue, actualTasks[i].ModifiedDate);
            }
        }
    }
}
