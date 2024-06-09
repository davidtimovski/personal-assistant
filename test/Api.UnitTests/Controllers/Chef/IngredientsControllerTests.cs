using Api.UnitTests.Builders;
using Chef.Api.Controllers;
using Chef.Application.Contracts.Ingredients;
using Chef.Application.Contracts.Ingredients.Models;
using Core.Application.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace Api.UnitTests.Controllers.Chef;

public class IngredientsControllerTests
{
    private readonly Mock<IUserIdLookup> _userIdLookupMock = new();
    private readonly Mock<IIngredientService> _ingredientServiceMock = new();
    private readonly IngredientsController _sut;

    public IngredientsControllerTests()
    {
        _sut = new IngredientsController(
            _userIdLookupMock.Object,
            new Mock<IUsersRepository>().Object,
            _ingredientServiceMock.Object,
            new Mock<IStringLocalizer<IngredientsController>>().Object,
            new Mock<IValidator<UpdateIngredient>>().Object,
            new Mock<IValidator<UpdatePublicIngredient>>().Object)
        {
            ControllerContext = new ControllerContextBuilder().Build()
        };
    }

    [Fact]
    public void GetForUpdate_Returns404_IfNotFound()
    {
        _userIdLookupMock.Setup(x => x.Contains(It.IsAny<string>())).Returns(true);
        _userIdLookupMock.Setup(x => x.Get(It.IsAny<string>())).Returns(1);
        _ingredientServiceMock.Setup(x => x.GetForUpdate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns((EditIngredient?)null);

        var result = _sut.GetForUpdate(It.IsAny<int>());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_Returns400_IfBodyMissing()
    {
        var result = await _sut.Update(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task UpdatePublic_Returns400_IfBodyMissing()
    {
        var result = await _sut.UpdatePublic(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }
}
