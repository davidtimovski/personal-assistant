using Api.UnitTests.Builders;
using Chef.Api.Controllers;
using Chef.Application.Contracts.DietaryProfiles;
using Chef.Application.Contracts.DietaryProfiles.Models;
using Chef.Application.Services;
using Core.Application.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Api.UnitTests.Controllers.Chef;

public class DietaryProfilesControllerTests
{
    private readonly DietaryProfilesController _sut;

    public DietaryProfilesControllerTests()
    {
        _sut = new DietaryProfilesController(
            new Mock<IUserIdLookup>().Object,
            new Mock<IUsersRepository>().Object,
            new Mock<IDietaryProfileService>().Object,
            new Mock<IValidator<GetRecommendedDailyIntake>>().Object,
            new Mock<IValidator<UpdateDietaryProfile>>().Object)
        {
            ControllerContext = new ControllerContextBuilder().Build()
        };
    }

    [Fact]
    public void GetDailyIntake_Returns400_IfBodyMissing()
    {
        var result = _sut.GetDailyIntake(null);
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task CreateOrUpdate_Returns400_IfBodyMissing()
    {
        var result = await _sut.CreateOrUpdate(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }
}
