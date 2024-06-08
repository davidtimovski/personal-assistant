using Application.UnitTests.Builders;
using Core.Application.Contracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Contracts.Tasks;
using ToDoAssistant.Application.Contracts.Tasks.Models;
using ToDoAssistant.Application.Entities;
using ToDoAssistant.Application.Mappings;
using ToDoAssistant.Application.Services;
using Xunit;
using User = Core.Application.Entities.User;

namespace Application.UnitTests.ServiceTests.TaskServiceTests;

public class UpdateTests
{
    private readonly Mock<IValidator<UpdateTask>> _successfulValidatorMock;
    private readonly Mock<IListService> _listServiceMock = new();
    private readonly Mock<ITasksRepository> _tasksRepositoryMock = new();
    private readonly Mock<IListsRepository> _listsRepositoryMock = new();
    private readonly Mock<ISpan> _metricsSpanMock = new();
    private readonly ITaskService _sut;

    public UpdateTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<UpdateTask>();

        _listServiceMock.Setup(x => x.IsShared(It.IsAny<int>(), It.IsAny<int>())).Returns(new Result<bool?>(true));
        _listServiceMock.Setup(x => x.GetUsersToBeNotifiedOfChange(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>())).Returns(new Result<IReadOnlyList<User>>(new List<User>()));

        _tasksRepositoryMock.Setup(x => x.Get(It.IsAny<int>())).Returns(new ToDoTask());
        _listsRepositoryMock.Setup(x => x.GetWithShares(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>())).Returns(new ToDoList
        {
            Shares = new List<ListShare>()
        });

        _metricsSpanMock.Setup(x => x.StartChild(It.IsAny<string>())).Returns(new Mock<ISpan>().Object);

        _sut = new TaskService(
            new Mock<IUserService>().Object,
            _listServiceMock.Object,
            _tasksRepositoryMock.Object,
            _listsRepositoryMock.Object,
            MapperMocker.GetMapper<ToDoAssistantProfile>(),
            new Mock<ILogger<TaskService>>().Object);
    }

    [Fact]
    public async Task ValidatesModel()
    {
        UpdateTask model = new TaskBuilder().BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        _successfulValidatorMock.Verify(x => x.Validate(model));
    }

    [Fact]
    public async Task ReturnsInvalidStatus_IfValidationFails()
    {
        UpdateTask model = new TaskBuilder().BuildUpdateModel();
        var failedValidator = ValidatorMocker.GetFailed<UpdateTask>();

        var result = await _sut.UpdateAsync(model, failedValidator.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        Assert.Equal(result.Status, ResultStatus.Invalid);
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
