using Application.UnitTests.Builders;
using CookingAssistant.Application.Contracts.Common;
using CookingAssistant.Application.Contracts.DietaryProfiles;
using CookingAssistant.Application.Contracts.DietaryProfiles.Models;
using CookingAssistant.Application.Mappings;
using CookingAssistant.Application.Services;
using FluentValidation;
using Microsoft.Extensions.Options;
using Moq;
using Utility;
using Xunit;

namespace Application.UnitTests.ServiceTests.DietaryProfileServiceTests;

public class GetRecommendedDailyIntakeTests
{
    private readonly IDietaryProfileService _sut;

    public GetRecommendedDailyIntakeTests()
    {
        DailyIntakeReference intakeRefModel = new DietaryProfileBuilder().BuildDailyIntakeReference();

        var dailyIntakeRefOptionsMock = new Mock<IOptions<DailyIntakeReference>>();
        dailyIntakeRefOptionsMock.Setup(x => x.Value).Returns(intakeRefModel);

        _sut = new DietaryProfileService(
            new Mock<IConversion>().Object,
            new Mock<IDailyIntakeHelper>().Object,
            dailyIntakeRefOptionsMock.Object,
            null,
            MapperMocker.GetMapper<CookingAssistantProfile>(),
            null);
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
    public void Validate_Throws_IfInvalidModel()
    {
        GetRecommendedDailyIntake model = new DietaryProfileBuilder().BuildGetRecommendedModel();
        var failedValidator = ValidatorMocker.GetFailed<GetRecommendedDailyIntake>();

        Assert.Throws<ValidationException>(() => _sut.GetRecommendedDailyIntake(model, failedValidator.Object));
    }
}
