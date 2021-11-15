using Api.Config;
using Api.Controllers.CookingAssistant;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using PersonalAssistant.Api.UnitTests.Builders;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.Common.Models;
using PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using Xunit;

namespace PersonalAssistant.Api.UnitTests.Controllers.CookingAssistant
{
    public class RecipesControllerTests
    {
        private readonly Mock<IRecipeService> _recipeServiceMock = new Mock<IRecipeService>();
        private readonly RecipesController _sut;

        public RecipesControllerTests()
        {
            _sut = new RecipesController(
                 _recipeServiceMock.Object,
                new Mock<IIngredientService>().Object,
                new Mock<ITaskService>().Object,
                new Mock<IStringLocalizer<RecipesController>>().Object,
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
                new Mock<IOptions<Urls>>().Object)
            {
                ControllerContext = new ControllerContextBuilder().Build()
            };
        }

        [Fact]
        public void GetReturns404IfNotFound()
        {
            _recipeServiceMock.Setup(x => x.Get(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns((RecipeDto)null);

            var result = _sut.Get(It.IsAny<int>(), It.IsAny<string>());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetForUpdateReturns404IfNotFound()
        {
            _recipeServiceMock.Setup(x => x.GetForUpdate(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((RecipeForUpdate)null);

            var result = _sut.GetForUpdate(It.IsAny<int>());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetWithSharesReturns404IfNotFound()
        {
            _recipeServiceMock.Setup(x => x.GetWithShares(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((RecipeWithShares)null);

            var result = _sut.GetWithShares(It.IsAny<int>());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetForSendingReturns404IfNotFound()
        {
            _recipeServiceMock.Setup(x => x.GetForSending(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((RecipeForSending)null);

            var result = _sut.GetForSending(It.IsAny<int>());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetForReviewReturns404IfNotFound()
        {
            _recipeServiceMock.Setup(x => x.GetForReview(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((RecipeForReview)null);

            var result = _sut.GetForReview(It.IsAny<int>());

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
