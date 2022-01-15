﻿using System.Threading.Tasks;
using Api.Controllers.CookingAssistant;
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
            _sut = new IngredientsController(_ingredientServiceMock.Object, null)
            {
                ControllerContext = new ControllerContextBuilder().Build()
            };
        }

        [Fact]
        public void Get_Returns404_IfNotFound()
        {
            _ingredientServiceMock.Setup(x => x.Get(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((EditIngredient)null);

            var result = _sut.Get(It.IsAny<int>());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_Returns400_IfBodyMissing()
        {
            var result = await _sut.Update(null);
            Assert.IsType<BadRequestResult>(result);
        }
    }
}