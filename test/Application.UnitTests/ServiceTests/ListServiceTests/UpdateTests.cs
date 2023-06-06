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

public class UpdateTests
{
    private readonly Mock<IValidator<UpdateList>> _successfulValidatorMock;
    private readonly Mock<IListsRepository> _listsRepositoryMock = new();
    private readonly Mock<ISpan> _metricsSpanMock = new();
    private readonly IListService _sut;

    public UpdateTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<UpdateList>();

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
        _listsRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ToDoList>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .ReturnsAsync(new ToDoList());

        UpdateList model = new ListBuilder().BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object);

        _successfulValidatorMock.Verify(x => x.Validate(model));
    }

    [Fact]
    public async Task Validate_Throws_IfInvalidModel()
    {
        UpdateList model = new ListBuilder().BuildUpdateModel();
        var failedValidator = ValidatorMocker.GetFailed<UpdateList>();

        await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateAsync(model, failedValidator.Object, _metricsSpanMock.Object));
    }

    [Fact]
    public async Task TrimsListName()
    {
        string? actualName = null;
        _listsRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ToDoList>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Callback<ToDoList, int, ISpan>((l, id, s) => actualName = l.Name)
            .ReturnsAsync(new ToDoList());

        UpdateList model = new ListBuilder().WithName(" List name ").BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object);
        const string expected = "List name";

        Assert.Equal(expected, actualName);
    }
}
