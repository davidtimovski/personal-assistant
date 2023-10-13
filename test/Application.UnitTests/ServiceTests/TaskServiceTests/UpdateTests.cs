using Application.UnitTests.Builders;
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

namespace Application.UnitTests.ServiceTests.TaskServiceTests;

public class UpdateTests
{
    private readonly Mock<IValidator<UpdateTask>> _successfulValidatorMock;
    private readonly Mock<ITasksRepository> _tasksRepositoryMock = new();
    private readonly Mock<IListsRepository> _listsRepositoryMock = new();
    private readonly Mock<ISpan> _metricsSpanMock = new();
    private readonly ITaskService _sut;

    public UpdateTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<UpdateTask>();

        _tasksRepositoryMock.Setup(x => x.Get(It.IsAny<int>())).Returns(new ToDoTask());
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
        UpdateTask model = new TaskBuilder().BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        _successfulValidatorMock.Verify(x => x.Validate(model));
    }

    [Fact]
    public async Task Validate_Throws_IfInvalidModel()
    {
        UpdateTask model = new TaskBuilder().BuildUpdateModel();
        var failedValidator = ValidatorMocker.GetFailed<UpdateTask>();

        await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateAsync(model, failedValidator.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TrimsName()
    {
        string? actualName = null;
        _tasksRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ToDoTask>(), It.IsAny<int>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<ToDoTask, int, ISpan, CancellationToken>((t, _, _, _) => actualName = t.Name);

        UpdateTask model = new TaskBuilder().WithName(" Task name ").BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());
        const string expected = "Task name";

        Assert.Equal(expected, actualName);
    }
}
