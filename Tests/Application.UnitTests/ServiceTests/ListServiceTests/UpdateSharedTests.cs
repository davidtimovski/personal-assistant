using System.Threading.Tasks;
using FluentValidation;
using Moq;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Application.Services.ToDoAssistant;
using PersonalAssistant.Application.UnitTests.Builders;
using Xunit;

namespace PersonalAssistant.Application.UnitTests.ServiceTests.ListServiceTests
{
    public class UpdateSharedTests
    {
        private readonly Mock<IValidator<UpdateSharedList>> _successfulValidatorMock;
        private readonly Mock<IListsRepository> _listsRepositoryMock = new Mock<IListsRepository>();
        private readonly IListService _sut;

        public UpdateSharedTests()
        {
            _successfulValidatorMock = ValidatorMocker.GetSuccessful<UpdateSharedList>();

            _sut = new ListService(
                new Mock<IUserService>().Object,
                _listsRepositoryMock.Object,
                new Mock<INotificationsRepository>().Object,
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
        public async Task ValidateThrowsIfInvalidModel()
        {
            UpdateSharedList model = new ListBuilder().BuildUpdateSharedModel();
            var failedValidator = ValidatorMocker.GetFailed<UpdateSharedList>();

            await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateSharedAsync(model, failedValidator.Object));
        }
    }
}
