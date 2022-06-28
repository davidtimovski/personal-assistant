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

namespace Application.UnitTests.ServiceTests.TaskServiceTests;

public class UpdateTests
{
    private readonly Mock<IValidator<UpdateTask>> _successfulValidatorMock;
    private readonly Mock<ITasksRepository> _tasksRepositoryMock = new();
    private readonly Mock<IListsRepository> _listsRepositoryMock = new();
    private readonly ITaskService _sut;

    public UpdateTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<UpdateTask>();

        _tasksRepositoryMock.Setup(x => x.Get(It.IsAny<int>())).Returns(new ToDoTask());
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
        UpdateTask model = new TaskBuilder().BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object);

        _successfulValidatorMock.Verify(x => x.Validate(model));
    }

    [Fact]
    public async Task Validate_Throws_IfInvalidModel()
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
