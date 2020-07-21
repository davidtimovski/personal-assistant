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
    public class CopyTests
    {
        private readonly Mock<IValidator<CopyList>> _successfulValidatorMock;
        private readonly Mock<IListsRepository> _listsRepositoryMock = new Mock<IListsRepository>();
        private readonly IListService _sut;

        public CopyTests()
        {
            _successfulValidatorMock = ValidatorMocker.GetSuccessful<CopyList>();

            _sut = new ListService(
                new Mock<IUserService>().Object,
                _listsRepositoryMock.Object,
                new Mock<INotificationsRepository>().Object,
                MapperMocker.GetMapper<ToDoAssistantProfile>());
        }

        [Fact]
        public async Task ValidatesModel()
        {
            CopyList model = new ListBuilder().BuildCopyModel();

            await _sut.CopyAsync(model, _successfulValidatorMock.Object);

            _successfulValidatorMock.Verify(x => x.Validate(model), Times.Once);
        }

        [Fact]
        public async Task ValidateThrowsIfInvalidModel()
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
                .Callback<ToDoList>((l) => actualName = l.Name);

            CopyList model = new ListBuilder().WithName(" List name ").BuildCopyModel();

            await _sut.CopyAsync(model, _successfulValidatorMock.Object);
            const string expected = "List name";

            Assert.Equal(expected, actualName);
        }
    }
}
