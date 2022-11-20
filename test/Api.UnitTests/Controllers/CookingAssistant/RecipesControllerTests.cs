using Api.Config;
using Api.Controllers.CookingAssistant;
using Api.UnitTests.Builders;
using Application.Contracts;
using CookingAssistant.Application.Contracts.Recipes;
using CookingAssistant.Application.Contracts.Recipes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Api.UnitTests.Controllers.CookingAssistant;

public class RecipesControllerTests
{
    private readonly Mock<IUserIdLookup> _userIdLookupMock = new();
    private readonly Mock<IRecipeService> _recipeServiceMock = new();
    private readonly RecipesController _sut;

    public RecipesControllerTests()
    {
        _sut = new RecipesController(_userIdLookupMock.Object, null, _recipeServiceMock.Object,
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, new Mock<IOptions<Urls>>().Object, null)
        {
            ControllerContext = new ControllerContextBuilder().Build()
        };
    }

    [Fact]
    public void Get_Returns404_IfNotFound()
    {
        _userIdLookupMock.Setup(x => x.Contains(It.IsAny<string>())).Returns(true);
        _userIdLookupMock.Setup(x => x.Get(It.IsAny<string>())).Returns(1);
        _recipeServiceMock.Setup(x => x.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns((RecipeDto)null);

        var result = _sut.Get(It.IsAny<int>(), It.IsAny<string>());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetForUpdate_Returns404_IfNotFound()
    {
        _userIdLookupMock.Setup(x => x.Contains(It.IsAny<string>())).Returns(true);
        _userIdLookupMock.Setup(x => x.Get(It.IsAny<string>())).Returns(1);
        _recipeServiceMock.Setup(x => x.GetForUpdate(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((RecipeForUpdate)null);

        var result = _sut.GetForUpdate(It.IsAny<int>());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetWithShares_Returns404_IfNotFound()
    {
        _userIdLookupMock.Setup(x => x.Contains(It.IsAny<string>())).Returns(true);
        _userIdLookupMock.Setup(x => x.Get(It.IsAny<string>())).Returns(1);
        _recipeServiceMock.Setup(x => x.GetWithShares(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((RecipeWithShares)null);

        var result = _sut.GetWithShares(It.IsAny<int>());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetForSending_Returns404_IfNotFound()
    {
        _userIdLookupMock.Setup(x => x.Contains(It.IsAny<string>())).Returns(true);
        _userIdLookupMock.Setup(x => x.Get(It.IsAny<string>())).Returns(1);
        _recipeServiceMock.Setup(x => x.GetForSending(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((RecipeForSending)null);

        var result = _sut.GetForSending(It.IsAny<int>());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetForReview_Returns404_IfNotFound()
    {
        _userIdLookupMock.Setup(x => x.Contains(It.IsAny<string>())).Returns(true);
        _userIdLookupMock.Setup(x => x.Get(It.IsAny<string>())).Returns(1);
        _recipeServiceMock.Setup(x => x.GetForReview(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((RecipeForReview)null);

        var result = _sut.GetForReview(It.IsAny<int>());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_Returns400_IfBodyMissing()
    {
        var result = await _sut.Create(null);
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Update_Returns400_IfBodyMissing()
    {
        var result = await _sut.Update(null);
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Share_Returns400_IfBodyMissing()
    {
        var result = await _sut.Share(null);
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task SetShareIsAccepted_Returns400_IfBodyMissing()
    {
        var result = await _sut.SetShareIsAccepted(null);
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Send_Returns400_IfBodyMissing()
    {
        var result = await _sut.Send(null);
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task DeclineSendRequest_Returns400_IfBodyMissing()
    {
        var result = await _sut.DeclineSendRequest(null);
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task TryImport_Returns400_IfBodyMissing()
    {
        var result = await _sut.TryImport(null);
        Assert.IsType<BadRequestResult>(result);
    }
}
