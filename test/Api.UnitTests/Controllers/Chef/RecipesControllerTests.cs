using Api.UnitTests.Builders;
using Chef.Api.Controllers;
using Chef.Api.Models;
using Chef.Application.Contracts.Ingredients;
using Chef.Application.Contracts.Recipes;
using Chef.Application.Contracts.Recipes.Models;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Sentry;
using Xunit;

namespace Api.UnitTests.Controllers.Chef;

public class RecipesControllerTests
{
    private readonly Mock<IUserIdLookup> _userIdLookupMock = new();
    private readonly Mock<IRecipeService> _recipeServiceMock = new();
    private readonly RecipesController _sut;

    public RecipesControllerTests()
    {
        _sut = new RecipesController(
            _userIdLookupMock.Object,
            new Mock<IUsersRepository>().Object,
            _recipeServiceMock.Object,
            new Mock<IIngredientService>().Object,
            new Mock<IStringLocalizer<RecipesController>>().Object,
            new Mock<IStringLocalizer<IngredientsController>>().Object,
            new Mock<IWebHostEnvironment>().Object,
            new Mock<ICdnService>().Object,
            new Mock<IUserService>().Object,
            new Mock<ISenderService>().Object,
            new Mock<IValidator<CreateRecipe>>().Object,
            new Mock<IValidator<UpdateRecipe>>().Object,
            new Mock<IValidator<ShareRecipe>>().Object,
            new Mock<IValidator<CreateSendRequest>>().Object,
            new Mock<IValidator<ImportRecipe>>().Object,
            new Mock<IValidator<UploadTempImage>>().Object,
            new Mock<IOptions<AppConfiguration>>().Object,
            new Mock<ILogger<RecipesController>>().Object)
        {
            ControllerContext = new ControllerContextBuilder().Build()
        };
    }

    [Fact]
    public void Get_Returns404_IfNotFound()
    {
        _userIdLookupMock.Setup(x => x.Contains(It.IsAny<string>())).Returns(true);
        _userIdLookupMock.Setup(x => x.Get(It.IsAny<string>())).Returns(1);
        _recipeServiceMock.Setup(x => x.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ISpan>()))
            .Returns((RecipeDto?)null);

        var result = _sut.Get(It.IsAny<int>(), It.IsAny<string>());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetForUpdate_Returns404_IfNotFound()
    {
        _userIdLookupMock.Setup(x => x.Contains(It.IsAny<string>())).Returns(true);
        _userIdLookupMock.Setup(x => x.Get(It.IsAny<string>())).Returns(1);
        _recipeServiceMock.Setup(x => x.GetForUpdate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns((RecipeForUpdate?)null);

        var result = _sut.GetForUpdate(It.IsAny<int>());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetWithShares_Returns404_IfNotFound()
    {
        _userIdLookupMock.Setup(x => x.Contains(It.IsAny<string>())).Returns(true);
        _userIdLookupMock.Setup(x => x.Get(It.IsAny<string>())).Returns(1);
        _recipeServiceMock.Setup(x => x.GetWithShares(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns((RecipeWithShares?)null);

        var result = _sut.GetWithShares(It.IsAny<int>());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetForSending_Returns404_IfNotFound()
    {
        _userIdLookupMock.Setup(x => x.Contains(It.IsAny<string>())).Returns(true);
        _userIdLookupMock.Setup(x => x.Get(It.IsAny<string>())).Returns(1);
        _recipeServiceMock.Setup(x => x.GetForSending(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns((RecipeForSending?)null);

        var result = _sut.GetForSending(It.IsAny<int>());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetForReview_Returns404_IfNotFound()
    {
        _userIdLookupMock.Setup(x => x.Contains(It.IsAny<string>())).Returns(true);
        _userIdLookupMock.Setup(x => x.Get(It.IsAny<string>())).Returns(1);
        _recipeServiceMock.Setup(x => x.GetForReview(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns((RecipeForReview?)null);

        var result = _sut.GetForReview(It.IsAny<int>());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_Returns400_IfBodyMissing()
    {
        var result = await _sut.Create(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Update_Returns400_IfBodyMissing()
    {
        var result = await _sut.Update(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Share_Returns400_IfBodyMissing()
    {
        var result = await _sut.Share(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task SetShareIsAccepted_Returns400_IfBodyMissing()
    {
        var result = await _sut.SetShareIsAccepted(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Send_Returns400_IfBodyMissing()
    {
        var result = await _sut.Send(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task DeclineSendRequest_Returns400_IfBodyMissing()
    {
        var result = await _sut.DeclineSendRequest(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task TryImport_Returns400_IfBodyMissing()
    {
        var result = await _sut.TryImport(null, It.IsAny<CancellationToken>());
        Assert.IsType<BadRequestResult>(result);
    }
}
