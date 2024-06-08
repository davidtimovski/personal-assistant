using Application.UnitTests.Builders;
using Chef.Application.Contracts.Common;
using Chef.Application.Contracts.DietaryProfiles;
using Chef.Application.Contracts.DietaryProfiles.Models;
using Chef.Application.Mappings;
using Chef.Application.Services;
using Chef.Utility;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Application.UnitTests.ServiceTests.DietaryProfileServiceTests;

public class UpdateTests
{
    private readonly Mock<ISpan> _metricsSpanMock = new();
    private readonly IDietaryProfileService _sut;

    public UpdateTests()
    {
        _metricsSpanMock.Setup(x => x.StartChild(It.IsAny<string>())).Returns(new Mock<ISpan>().Object);

        _sut = new DietaryProfileService(
            new Mock<IConversion>().Object,
            new Mock<IDailyIntakeHelper>().Object,
            new Mock<IOptions<DailyIntakeReference>>().Object,
            new Mock<IDietaryProfilesRepository>().Object,
            MapperMocker.GetMapper<ChefProfile>(),
            new Mock<ILogger<DietaryProfileService>>().Object);
    }

    [Fact]
    public async Task ValidatesModel()
    {
        UpdateDietaryProfile model = new DietaryProfileBuilder().BuildUpdateModel();
        var validator = ValidatorMocker.GetSuccessful<UpdateDietaryProfile>();

        await _sut.CreateOrUpdateAsync(model, validator.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        validator.Verify(x => x.Validate(model));
    }

    [Fact]
    public async Task Validate_Throws_IfInvalidModel()
    {
        UpdateDietaryProfile model = new DietaryProfileBuilder().BuildUpdateModel();
        var failedValidator = ValidatorMocker.GetFailed<UpdateDietaryProfile>();

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateOrUpdateAsync(model, failedValidator.Object, It.IsAny<ISpan>(), It.IsAny<CancellationToken>()));
    }
}
