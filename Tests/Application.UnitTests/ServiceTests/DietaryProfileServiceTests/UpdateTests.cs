using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Options;
using Moq;
using PersonalAssistant.Application.Contracts.CookingAssistant.Common;
using PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles;
using PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles.Models;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Application.Services.CookingAssistant;
using PersonalAssistant.Application.UnitTests.Builders;
using Xunit;

namespace PersonalAssistant.Application.UnitTests.ServiceTests.DietaryProfileServiceTests
{
    public class UpdateTests
    {
        private readonly Mock<IDietaryProfilesRepository> _dietaryProfilesRepositoryMock = new Mock<IDietaryProfilesRepository>();
        private readonly IDietaryProfileService _sut;

        public UpdateTests()
        {
            _sut = new DietaryProfileService(
                new Mock<IConversionService>().Object,
                new Mock<IOptions<DailyIntakeReference>>().Object,
                _dietaryProfilesRepositoryMock.Object,
                MapperMocker.GetMapper<CookingAssistantProfile>());
        }

        [Fact]
        public async Task ValidatesModel()
        {
            UpdateDietaryProfile model = new DietaryProfileBuilder().BuildUpdateModel();
            var validator = ValidatorMocker.GetSuccessful<UpdateDietaryProfile>();

            await _sut.UpdateAsync(model, validator.Object);

            validator.Verify(x => x.Validate(model), Times.Once);
        }

        [Fact]
        public async Task ValidateThrowsIfInvalidModel()
        {
            UpdateDietaryProfile model = new DietaryProfileBuilder().BuildUpdateModel();
            var failedValidator = ValidatorMocker.GetFailed<UpdateDietaryProfile>();

            await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateAsync(model, failedValidator.Object));
        }
    }
}
