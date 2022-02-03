using System.Threading.Tasks;
using FluentValidation;
using Moq;
using Application.Contracts.ToDoAssistant.Lists;
using Application.Contracts.ToDoAssistant.Lists.Models;
using Application.Mappings;
using Application.Services.ToDoAssistant;
using Application.UnitTests.Builders;
using Xunit;

namespace Application.UnitTests.ServiceTests.ListServiceTests;

public class UpdateSharedTests
{
    private readonly Mock<IValidator<UpdateSharedList>> _successfulValidatorMock;
    private readonly Mock<IListsRepository> _listsRepositoryMock = new();
    private readonly IListService _sut;

    public UpdateSharedTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<UpdateSharedList>();

        _sut = new ListService(
            null,
            _listsRepositoryMock.Object,
            null,
            null,
            MapperMocker.GetMapper<ToDoAssistantProfile>());
    }

    [Fact]
    public async Task ValidatesModel()
    {
        UpdateSharedList model = new ListBuilder().BuildUpdateSharedModel();

        await _sut.UpdateSharedAsync(model, _successfulValidatorMock.Object);

        _successfulValidatorMock.Verify(x => x.Validate(model));
    }

    [Fact]
    public async Task Validate_Throws_IfInvalidModel()
    {
        UpdateSharedList model = new ListBuilder().BuildUpdateSharedModel();
        var failedValidator = ValidatorMocker.GetFailed<UpdateSharedList>();

        await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateSharedAsync(model, failedValidator.Object));
    }
}