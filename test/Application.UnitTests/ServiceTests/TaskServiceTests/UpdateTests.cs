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

public class UpdateTests
{
    private readonly Mock<IValidator<UpdateTask>> _successfulValidatorMock;
    private readonly Mock<ITasksRepository> _tasksRepositoryMock = new();
    private readonly Mock<IListsRepository> _listsRepositoryMock = new();
    private readonly Mock<ITransaction> _sentryTr = new();
    private readonly ITaskService _sut;

    public UpdateTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<UpdateTask>();

        _tasksRepositoryMock.Setup(x => x.Get(It.IsAny<int>())).Returns(new ToDoTask());
        _listsRepositoryMock.Setup(x => x.GetWithShares(It.IsAny<int>(), It.IsAny<int>())).Returns(new ToDoList
        {
            Shares = new List<ListShare>()
        });

        _sentryTr.Setup(x => x.StartChild(It.IsAny<string>())).Returns(new Mock<ISpan>().Object);

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

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _sentryTr.Object);

        _successfulValidatorMock.Verify(x => x.Validate(model));
    }

    [Fact]
    public async Task Validate_Throws_IfInvalidModel()
    {
        UpdateTask model = new TaskBuilder().BuildUpdateModel();
        var failedValidator = ValidatorMocker.GetFailed<UpdateTask>();

        await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateAsync(model, failedValidator.Object, _sentryTr.Object));
    }

    [Fact]
    public async Task TrimsName()
    {
        string actualName = null;
        _tasksRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ToDoTask>(), It.IsAny<int>(), It.IsAny<ITransaction>()))
            .Callback<ToDoTask, int, ITransaction>((t, i, tr) => actualName = t.Name);

        UpdateTask model = new TaskBuilder().WithName(" Task name ").BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _sentryTr.Object);
        const string expected = "Task name";

        Assert.Equal(expected, actualName);
    }
}
