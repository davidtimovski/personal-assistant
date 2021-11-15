using Api.Controllers.CookingAssistant;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PersonalAssistant.Api.UnitTests.Builders;
using PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients;
using PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients.Models;
using Xunit;

namespace PersonalAssistant.Api.UnitTests.Controllers.CookingAssistant
{
    public class IngredientsControllerTests
    {
        private readonly Mock<IIngredientService> _ingredientServiceMock = new Mock<IIngredientService>();
        private readonly IngredientsController _sut;

        public IngredientsControllerTests()
        {
            _sut = new IngredientsController(
                _ingredientServiceMock.Object,
                new Mock<IValidator<UpdateIngredient>>().Object)
            {
                ControllerContext = new ControllerContextBuilder().Build()
            };
        }

        [Fact]
        public void GetReturns404IfNotFound()
        {
            _ingredientServiceMock.Setup(x => x.Get(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((EditIngredient)null);

            var result = _sut.Get(It.IsAny<int>());

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
