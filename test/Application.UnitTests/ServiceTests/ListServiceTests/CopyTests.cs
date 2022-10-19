using System.Threading.Tasks;
using Application.Contracts.ToDoAssistant.Lists;
using Application.Contracts.ToDoAssistant.Lists.Models;
using Application.Mappings;
using Application.Services.ToDoAssistant;
using Application.UnitTests.Builders;
using Domain.Entities.ToDoAssistant;
using FluentValidation;
using Moq;
using Xunit;

namespace Application.UnitTests.ServiceTests.ListServiceTests;

public class CopyTests
{
    private readonly Mock<IValidator<CopyList>> _successfulValidatorMock;
    private readonly Mock<IListsRepository> _listsRepositoryMock = new();
    private readonly IListService _sut;

    public CopyTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<CopyList>();

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
        CopyList model = new ListBuilder().BuildCopyModel();

        await _sut.CopyAsync(model, _successfulValidatorMock.Object);

        _successfulValidatorMock.Verify(x => x.Validate(model));
    }

    [Fact]
    public async Task Validate_Throws_IfInvalidModel()
    {
        CopyList model = new ListBuilder().BuildCopyModel();
        var failedValidator = ValidatorMocker.GetFailed<CopyList>();

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CopyAsync(model, failedValidator.Object));
    }

    [Fact]
    public async Task TrimsListName()
    {
        string actualName = null;
        _listsRepositoryMock.Setup(x => x.CopyAsync(It.IsAny<ToDoList>()))
            .Callback<ToDoList>(l => actualName = l.Name);

        CopyList model = new ListBuilder().WithName(" List name ").BuildCopyModel();

        await _sut.CopyAsync(model, _successfulValidatorMock.Object);
        const string expected = "List name";

        Assert.Equal(expected, actualName);
    }
}
