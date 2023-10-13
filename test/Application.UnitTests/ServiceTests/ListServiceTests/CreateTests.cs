using Application.UnitTests.Builders;
using FluentValidation;
using Moq;
using Sentry;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Contracts.Lists.Models;
using ToDoAssistant.Application.Entities;
using ToDoAssistant.Application.Mappings;
using ToDoAssistant.Application.Services;
using Xunit;

namespace Application.UnitTests.ServiceTests.ListServiceTests;

public class CreateTests
{
    private readonly Mock<IValidator<CreateList>> _successfulValidatorMock;
    private readonly Mock<IListsRepository> _listsRepositoryMock = new();
    private readonly Mock<ISpan> _metricsSpanMock = new();
    private readonly IListService _sut;

    public CreateTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<CreateList>();

        _metricsSpanMock.Setup(x => x.StartChild(It.IsAny<string>())).Returns(new Mock<ISpan>().Object);

        _sut = new ListService(
            null,
            _listsRepositoryMock.Object,
            null,
            null,
            MapperMocker.GetMapper<ToDoAssistantProfile>(),
            null);
    }

    [Fact]
    public async Task ValidatesModel()
    {
        CreateList model = new ListBuilder().BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        _successfulValidatorMock.Verify(x => x.Validate(model));
    }

    [Fact]
    public async Task Validate_Throws_IfInvalidModel()
    {
        CreateList model = new ListBuilder().BuildCreateModel();
        var failedValidator = ValidatorMocker.GetFailed<CreateList>();

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(model, failedValidator.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TrimsName()
    {
        string? actualName = null;
        _listsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoList>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<ToDoList, ISpan, CancellationToken>((l, _, _) => actualName = l.Name);

        CreateList model = new ListBuilder().WithName(" List name ").BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());
        const string expected = "List name";

        Assert.Equal(expected, actualName);
    }

    [Theory]
    [InlineData("Task 1\nTask 2", 2)]
    [InlineData("Task 1\n\nTask 2\nTask 3", 3)]
    public async Task SplitsTextIntoTasksByNewline(string tasksText, int expectedTaskCount)
    {
        var resultingTasks = new List<ToDoTask>();
        _listsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoList>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<ToDoList, ISpan, CancellationToken>((l, _, _) => resultingTasks = l.Tasks);

        CreateList model = new ListBuilder().WithTasksText(tasksText).BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        _listsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<ToDoList>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()));

        Assert.Equal(expectedTaskCount, resultingTasks.Count);
    }

    [Theory]
    [InlineData("Task 1 \nTask 2")]
    [InlineData("Task 1\n\n Task 2")]
    public async Task SplitsTextIntoTasksWithTrimmedNames(string tasksText)
    {
        CreateList model = new ListBuilder().WithTasksText(tasksText).BuildCreateModel();

        var resultingTasks = new List<ToDoTask>();
        _listsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoList>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<ToDoList, ISpan, CancellationToken>((l, _, _) => resultingTasks = l.Tasks);

        await _sut.CreateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());
        var expectedTasks = new List<ToDoTask>
        {
            new ToDoTask { Name = "Task 1" },
            new ToDoTask { Name = "Task 2" }
        };

        _listsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<ToDoList>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()));

        for (var i = 0; i < expectedTasks.Count; i++)
        {
            Assert.Equal(expectedTasks[i].Name, resultingTasks[i].Name);
        }
    }

    [Fact]
    public async Task ResultingTasksHaveIsOneTimeSetFromList()
    {
        var resultingTasks = new List<ToDoTask>();
        _listsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoList>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<ToDoList, ISpan, CancellationToken>((l, _, _) => resultingTasks = l.Tasks);

        CreateList model = new ListBuilder().WithTasksText("Task 1\nTask 2").BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        _listsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<ToDoList>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()));

        for (var i = 0; i < resultingTasks.Count; i++)
        {
            Assert.Equal(model.IsOneTimeToggleDefault, resultingTasks[i].IsOneTime);
        }
    }

    [Fact]
    public async Task SetsCreatedDate()
    {
        var actualCreatedDate = new DateTime();
        _listsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoList>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<ToDoList, ISpan, CancellationToken>((l, _, _) => actualCreatedDate = l.CreatedDate);

        CreateList model = new ListBuilder().BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        Assert.NotEqual(DateTime.MinValue, actualCreatedDate);
    }

    [Fact]
    public async Task SetsModifiedDate()
    {
        var actualModifiedDate = new DateTime();
        _listsRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ToDoList>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<ToDoList, ISpan, CancellationToken>((l, _, _) => actualModifiedDate = l.ModifiedDate);

        CreateList model = new ListBuilder().BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        Assert.NotEqual(DateTime.MinValue, actualModifiedDate);
    }
}
