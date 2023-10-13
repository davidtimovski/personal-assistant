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

public class CopyTests
{
    private readonly Mock<IValidator<CopyList>> _successfulValidatorMock;
    private readonly Mock<IListsRepository> _listsRepositoryMock = new();
    private readonly Mock<ISpan> _metricsSpanMock = new();
    private readonly IListService _sut;

    public CopyTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<CopyList>();

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
        CopyList model = new ListBuilder().BuildCopyModel();

        await _sut.CopyAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        _successfulValidatorMock.Verify(x => x.Validate(model));
    }

    [Fact]
    public async Task Validate_Throws_IfInvalidModel()
    {
        CopyList model = new ListBuilder().BuildCopyModel();
        var failedValidator = ValidatorMocker.GetFailed<CopyList>();

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CopyAsync(model, failedValidator.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TrimsListName()
    {
        string? actualName = null;
        _listsRepositoryMock.Setup(x => x.CopyAsync(It.IsAny<ToDoList>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<ToDoList, ISpan, CancellationToken>((l, _, _) => actualName = l.Name);

        CopyList model = new ListBuilder().WithName(" List name ").BuildCopyModel();

        await _sut.CopyAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());
        const string expected = "List name";

        Assert.Equal(expected, actualName);
    }
}
