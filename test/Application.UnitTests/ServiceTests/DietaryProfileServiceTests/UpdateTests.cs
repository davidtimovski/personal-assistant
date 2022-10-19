using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Options;
using Moq;
using Application.Contracts.CookingAssistant.Common;
using Application.Contracts.CookingAssistant.DietaryProfiles;
using Application.Contracts.CookingAssistant.DietaryProfiles.Models;
using Application.Mappings;
using Application.Services.CookingAssistant;
using Application.UnitTests.Builders;
using Utility;
using Xunit;

namespace Application.UnitTests.ServiceTests.DietaryProfileServiceTests;

public class UpdateTests
{
    private readonly IDietaryProfileService _sut;

    public UpdateTests()
    {
        _sut = new DietaryProfileService(
            new Mock<IConversion>().Object,
            new Mock<IDailyIntakeHelper>().Object,
            new Mock<IOptions<DailyIntakeReference>>().Object,
            new Mock<IDietaryProfilesRepository>().Object,
            MapperMocker.GetMapper<CookingAssistantProfile>(),
            null);
    }

    [Fact]
    public async Task ValidatesModel()
    {
        UpdateDietaryProfile model = new DietaryProfileBuilder().BuildUpdateModel();
        var validator = ValidatorMocker.GetSuccessful<UpdateDietaryProfile>();

        await _sut.CreateOrUpdateAsync(model, validator.Object);

        validator.Verify(x => x.Validate(model));
    }

    [Fact]
    public async Task Validate_Throws_IfInvalidModel()
    {
        UpdateDietaryProfile model = new DietaryProfileBuilder().BuildUpdateModel();
        var failedValidator = ValidatorMocker.GetFailed<UpdateDietaryProfile>();

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateOrUpdateAsync(model, failedValidator.Object));
    }
}
