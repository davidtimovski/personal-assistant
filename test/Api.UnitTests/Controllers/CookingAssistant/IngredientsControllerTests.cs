using Api.Controllers.CookingAssistant;
using Api.UnitTests.Builders;
using Application.Contracts;
using CookingAssistant.Application.Contracts.Ingredients;
using CookingAssistant.Application.Contracts.Ingredients.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Api.UnitTests.Controllers.CookingAssistant;

public class IngredientsControllerTests
{
    private readonly Mock<IUserIdLookup> _userIdLookupMock = new();
    private readonly Mock<IIngredientService> _ingredientServiceMock = new();
    private readonly IngredientsController _sut;

    public IngredientsControllerTests()
    {
        _sut = new IngredientsController(_userIdLookupMock.Object, null, _ingredientServiceMock.Object, null, null, null)
        {
            ControllerContext = new ControllerContextBuilder().Build()
        };
    }

    [Fact]
    public void GetForUpdate_Returns404_IfNotFound()
    {
        _userIdLookupMock.Setup(x => x.Contains(It.IsAny<string>())).Returns(true);
        _userIdLookupMock.Setup(x => x.Get(It.IsAny<string>())).Returns(1);
        _ingredientServiceMock.Setup(x => x.GetForUpdate(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((EditIngredient)null);

        var result = _sut.GetForUpdate(It.IsAny<int>());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_Returns400_IfBodyMissing()
    {
        var result = await _sut.Update(null);
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task UpdatePublic_Returns400_IfBodyMissing()
    {
        var result = await _sut.UpdatePublic(null);
        Assert.IsType<BadRequestResult>(result);
    }
}
