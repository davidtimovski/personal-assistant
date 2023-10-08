using Application.UnitTests.Builders;
using Chef.Application.Contracts.Common;
using Chef.Application.Contracts.DietaryProfiles;
using Chef.Application.Contracts.DietaryProfiles.Models;
using Chef.Application.Mappings;
using Chef.Application.Services;
using Chef.Utility;
using FluentValidation;
using Microsoft.Extensions.Options;
using Moq;
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
            MapperMocker.GetMapper<ChefProfile>(),
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
