using Application.UnitTests.Builders;
using Core.Application.Contracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Contracts.Lists.Models;
using ToDoAssistant.Application.Contracts.Notifications;
using ToDoAssistant.Application.Contracts.Tasks;
using ToDoAssistant.Application.Mappings;
using ToDoAssistant.Application.Services;
using Xunit;

namespace Application.UnitTests.ServiceTests.ListServiceTests;

public class UpdateSharedTests
{
    private readonly Mock<IValidator<UpdateSharedList>> _successfulValidatorMock;
    private readonly Mock<IListsRepository> _listsRepositoryMock = new();
    private readonly Mock<ISpan> _metricsSpanMock = new();
    private readonly IListService _sut;

    public UpdateSharedTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<UpdateSharedList>();

        _metricsSpanMock.Setup(x => x.StartChild(It.IsAny<string>())).Returns(new Mock<ISpan>().Object);

        _sut = new ListService(
            new Mock<IUserService>().Object,
            _listsRepositoryMock.Object,
            new Mock<ITasksRepository>().Object,
            new Mock<INotificationsRepository>().Object,
            MapperMocker.GetMapper<ToDoAssistantProfile>(),
            new Mock<ILogger<ListService>>().Object);
    }

    [Fact]
    public async Task ValidatesModel()
    {
        UpdateSharedList model = new ListBuilder().BuildUpdateSharedModel();

        await _sut.UpdateSharedAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        _successfulValidatorMock.Verify(x => x.Validate(model));
    }

    [Fact]
    public async Task ReturnsInvalidStatus_IfValidationFails()
    {
        UpdateSharedList model = new ListBuilder().BuildUpdateSharedModel();
        var failedValidator = ValidatorMocker.GetFailed<UpdateSharedList>();

        var result = await _sut.UpdateSharedAsync(model, failedValidator.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        Assert.Equal(result.Status, ResultStatus.Invalid);
    }
}
