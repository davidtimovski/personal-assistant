using Application.UnitTests.Builders;
using Core.Application.Contracts;
using FluentValidation;
using Moq;
using Sentry;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Contracts.Tasks;
using ToDoAssistant.Application.Contracts.Tasks.Models;
using ToDoAssistant.Application.Entities;
using ToDoAssistant.Application.Mappings;
using ToDoAssistant.Application.Services;
using Xunit;
using User = Core.Application.Entities.User;

namespace Application.UnitTests.ServiceTests.TaskServiceTests;

public class CreateTests
{
    private readonly Mock<IValidator<CreateTask>> _successfulValidatorMock;
    private readonly Mock<IListService> _listServiceMock = new();
    private readonly Mock<ITasksRepository> _tasksRepositoryMock = new();
    private readonly Mock<IListsRepository> _listsRepositoryMock = new();
    private readonly Mock<ISpan> _metricsSpanMock = new();
    private readonly ITaskService _sut;

    public CreateTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<CreateTask>();

        _listServiceMock.Setup(x => x.IsShared(It.IsAny<int>(), It.IsAny<int>())).Returns(new Result<bool?>(true));
        _listServiceMock.Setup(x => x.GetUsersToBeNotifiedOfChange(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<ISpan>())).Returns(new Result<IReadOnlyList<User>>(new List<User>()));

        _listsRepositoryMock.Setup(x => x.GetWithShares(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>())).Returns(new ToDoList
        {
            Shares = new List<ListShare>()
        });

        _metricsSpanMock.Setup(x => x.StartChild(It.IsAny<string>())).Returns(new Mock<ISpan>().Object);

        _sut = new TaskService(
            null,
            _listServiceMock.Object,
            _tasksRepositoryMock.Object,
            _listsRepositoryMock.Object,
            MapperMocker.GetMapper<ToDoAssistantProfile>(),
            null);
    }

    [Fact]
    public async Task ValidatesModel()
    {
        CreateTask model = new TaskBuilder().BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        _successfulValidatorMock.Verify(x => x.Validate(model));
    }

    [Fact]
    public async Task ReturnsInvalidStatus_IfValidationFails()
    {
        CreateTask model = new TaskBuilder().BuildCreateModel();
        var failedValidator = ValidatorMocker.GetFailed<CreateTask>();

        var result = await _sut.CreateAsync(model, failedValidator.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        Assert.Equal(result.Status, ResultStatus.Invalid);
    }

    [Fact]
    public async Task TrimsName()
    {
        string? actualName = null;
        _tasksRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoTask>(), It.IsAny<int>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<ToDoTask, int, ISpan, CancellationToken>((t, _, _, _) => actualName = t.Name);

        CreateTask model = new TaskBuilder().WithName(" Task name ").BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());
        const string expected = "Task name";

        Assert.Equal(expected, actualName);
    }

    [Fact]
    public async Task SetsCreatedDate()
    {
        var actualCreatedDate = new DateTime();

        _tasksRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoTask>(), It.IsAny<int>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<ToDoTask, int, ISpan, CancellationToken>((t, _, _, _) => actualCreatedDate = t.CreatedDate);

        CreateTask model = new TaskBuilder().BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        Assert.NotEqual(DateTime.MinValue, actualCreatedDate);
    }

    [Fact]
    public async Task SetsModifiedDate()
    {
        var actualModifiedDate = new DateTime();
        _tasksRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoTask>(), It.IsAny<int>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<ToDoTask, int, ISpan, CancellationToken>((t, _, _, _) => actualModifiedDate = t.ModifiedDate);

        CreateTask model = new TaskBuilder().BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        Assert.NotEqual(DateTime.MinValue, actualModifiedDate);
    }
}
