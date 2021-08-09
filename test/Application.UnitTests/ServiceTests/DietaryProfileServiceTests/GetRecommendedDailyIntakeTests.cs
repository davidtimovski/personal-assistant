using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Options;
using Moq;
using PersonalAssistant.Application.Contracts.CookingAssistant.Common;
using PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles;
using PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles.Models;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Application.Services.CookingAssistant;
using PersonalAssistant.Application.UnitTests.Builders;
using Utility;
using Xunit;

namespace PersonalAssistant.Application.UnitTests.ServiceTests.DietaryProfileServiceTests
{
    public class GetRecommendedDailyIntakeTests
    {
        private readonly Mock<IOptions<DailyIntakeReference>> _dailyIntakeRefOptionsMock = new Mock<IOptions<DailyIntakeReference>>();
        private readonly IDietaryProfileService _sut;

        public GetRecommendedDailyIntakeTests()
        {
            DailyIntakeReference intakeRefModel = new DietaryProfileBuilder().BuildDailyIntakeReference();
            _dailyIntakeRefOptionsMock.Setup(x => x.Value).Returns(intakeRefModel);

            _sut = new DietaryProfileService(
                new Mock<IConversion>().Object,
                new Mock<IDailyIntakeHelper>().Object,
                _dailyIntakeRefOptionsMock.Object,
                new Mock<IDietaryProfilesRepository>().Object,
                GetMapper());
        }

        [Fact]
        public void ValidatesModel()
        {
            GetRecommendedDailyIntake model = new DietaryProfileBuilder().BuildGetRecommendedModel();
            var validator = ValidatorMocker.GetSuccessful<GetRecommendedDailyIntake>();

            _sut.GetRecommendedDailyIntake(model, validator.Object);

            validator.Verify(x => x.Validate(model));
        }

        [Fact]
        public void ValidateThrowsIfInvalidModel()
        {
            GetRecommendedDailyIntake model = new DietaryProfileBuilder().BuildGetRecommendedModel();
            var failedValidator = ValidatorMocker.GetFailed<GetRecommendedDailyIntake>();

            Assert.Throws<ValidationException>(() => _sut.GetRecommendedDailyIntake(model, failedValidator.Object));
        }

        private IMapper GetMapper()
        {
            var configurationProvider = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
                cfg.AddProfile<CookingAssistantProfile>();
            });
            return configurationProvider.CreateMapper();
        }
    }
}
