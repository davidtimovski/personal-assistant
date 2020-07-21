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
using PersonalAssistant.Domain.Entities.ToDoAssistant;
using Xunit;

namespace PersonalAssistant.Application.UnitTests.ServiceTests.ListServiceTests
{
    public class UpdateTests
    {
        private readonly Mock<IValidator<UpdateList>> _successfulValidatorMock;
        private readonly Mock<IListsRepository> _listsRepositoryMock = new Mock<IListsRepository>();
        private readonly IListService _sut;

        public UpdateTests()
        {
            _successfulValidatorMock = ValidatorMocker.GetSuccessful<UpdateList>();

            _sut = new ListService(
                new Mock<IUserService>().Object,
                _listsRepositoryMock.Object,
                new Mock<INotificationsRepository>().Object,
                MapperMocker.GetMapper<ToDoAssistantProfile>());
        }

        [Fact]
        public async Task ValidatesModel()
        {
            UpdateList model = new ListBuilder().BuildUpdateModel();

            await _sut.UpdateAsync(model, _successfulValidatorMock.Object);

            _successfulValidatorMock.Verify(x => x.Validate(model), Times.Once);
        }

        [Fact]
        public async Task ValidateThrowsIfInvalidModel()
        {
            UpdateList model = new ListBuilder().BuildUpdateModel();
            var failedValidator = ValidatorMocker.GetFailed<UpdateList>();

            await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateAsync(model, failedValidator.Object));
        }

        [Fact]
        public async Task TrimsListName()
        {
            string actualName = null;
            _listsRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<ToDoList>()))
                .Callback<ToDoList>((l) => actualName = l.Name);

            UpdateList model = new ListBuilder().WithName(" List name ").BuildUpdateModel();

            await _sut.UpdateAsync(model, _successfulValidatorMock.Object);
            const string expected = "List name";

            Assert.Equal(expected, actualName);
        }
    }
}
