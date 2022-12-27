using Application.UnitTests.Builders;
using Application.Domain.ToDoAssistant;
using FluentValidation;
using Moq;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Contracts.Lists.Models;
using ToDoAssistant.Application.Mappings;
using ToDoAssistant.Application.Services;
using Xunit;

namespace Application.UnitTests.ServiceTests.ListServiceTests;

public class UpdateTests
{
    private readonly Mock<IValidator<UpdateList>> _successfulValidatorMock;
    private readonly Mock<IListsRepository> _listsRepositoryMock = new();
    private readonly IListService _sut;

    public UpdateTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<UpdateList>();

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
        _listsRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ToDoList>(), It.IsAny<int>()))
            .ReturnsAsync(new ToDoList());

        UpdateList model = new ListBuilder().BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object);

        _successfulValidatorMock.Verify(x => x.Validate(model));
    }

    [Fact]
    public async Task Validate_Throws_IfInvalidModel()
    {
        UpdateList model = new ListBuilder().BuildUpdateModel();
        var failedValidator = ValidatorMocker.GetFailed<UpdateList>();

        await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateAsync(model, failedValidator.Object));
    }

    [Fact]
    public async Task TrimsListName()
    {
        string actualName = null;
        _listsRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ToDoList>(), It.IsAny<int>()))
            .Callback<ToDoList, int>((l, id) => actualName = l.Name)
            .ReturnsAsync(new ToDoList());

        UpdateList model = new ListBuilder().WithName(" List name ").BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object);
        const string expected = "List name";

        Assert.Equal(expected, actualName);
    }
}
