using Application.Domain.ToDoAssistant;
using Application.UnitTests.Builders;
using FluentValidation;
using Moq;
using Sentry;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Contracts.Tasks;
using ToDoAssistant.Application.Contracts.Tasks.Models;
using ToDoAssistant.Application.Mappings;
using ToDoAssistant.Application.Services;
using Xunit;

namespace Application.UnitTests.ServiceTests.TaskServiceTests;

public class CreateTests
{
    private readonly Mock<IValidator<CreateTask>> _successfulValidatorMock;
    private readonly Mock<ITasksRepository> _tasksRepositoryMock = new();
    private readonly Mock<IListsRepository> _listsRepositoryMock = new();
    private readonly Mock<ISpan> _metricsSpanMock = new();
    private readonly ITaskService _sut;

    public CreateTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<CreateTask>();

        _listsRepositoryMock.Setup(x => x.GetWithShares(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>())).Returns(new ToDoList
        {
            Shares = new List<ListShare>()
        });

        _metricsSpanMock.Setup(x => x.StartChild(It.IsAny<string>())).Returns(new Mock<ISpan>().Object);

        _sut = new TaskService(
            null,
            new Mock<IListService>().Object,
            _tasksRepositoryMock.Object,
            _listsRepositoryMock.Object,
            MapperMocker.GetMapper<ToDoAssistantProfile>(),
            null);
    }

    [Fact]
    public async Task ValidatesModel()
    {
        CreateTask model = new TaskBuilder().BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object);

        _successfulValidatorMock.Verify(x => x.Validate(model));
    }

    [Fact]
    public async Task Validate_Throws_IfInvalidModel()
    {
        CreateTask model = new TaskBuilder().BuildCreateModel();
        var failedValidator = ValidatorMocker.GetFailed<CreateTask>();

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(model, failedValidator.Object, _metricsSpanMock.Object));
    }

    [Fact]
    public async Task TrimsName()
    {
        string actualName = null;
        _tasksRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoTask>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Callback<ToDoTask, int, ISpan>((t, i, s) => actualName = t.Name);

        CreateTask model = new TaskBuilder().WithName(" Task name ").BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object);
        const string expected = "Task name";

        Assert.Equal(expected, actualName);
    }

    [Fact]
    public async Task SetsCreatedDate()
    {
        var actualCreatedDate = new DateTime();
        _tasksRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoTask>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Callback<ToDoTask, int, ISpan>((t, u, s) => actualCreatedDate = t.CreatedDate);

        CreateTask model = new TaskBuilder().BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object);

        Assert.NotEqual(DateTime.MinValue, actualCreatedDate);
    }

    [Fact]
    public async Task SetsModifiedDate()
    {
        var actualModifiedDate = new DateTime();
        _tasksRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoTask>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Callback<ToDoTask, int, ISpan>((t, u, s) => actualModifiedDate = t.ModifiedDate);

        CreateTask model = new TaskBuilder().BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object);

        Assert.NotEqual(DateTime.MinValue, actualModifiedDate);
    }
}
