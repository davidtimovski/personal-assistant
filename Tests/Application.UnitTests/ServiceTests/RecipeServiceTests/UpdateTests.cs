using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Moq;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Application.Services.CookingAssistant;
using PersonalAssistant.Application.UnitTests.Builders;
using PersonalAssistant.Domain.Entities.CookingAssistant;
using Xunit;

namespace PersonalAssistant.Application.UnitTests.ServiceTests.RecipeServiceTests
{
    public class UpdateTests
    {
        private readonly Mock<IValidator<UpdateRecipe>> _successfulValidatorMock;
        private readonly Mock<IRecipesRepository> _recipesRepositoryMock = new Mock<IRecipesRepository>();
        private readonly IRecipeService _sut;

        public UpdateTests()
        {
            _successfulValidatorMock = ValidatorMocker.GetSuccessful<UpdateRecipe>();

            _sut = new RecipeService(
                new Mock<ITaskService>().Object,
                new Mock<ICdnService>().Object,
                new Mock<IUserService>().Object,
                _recipesRepositoryMock.Object,
                MapperMocker.GetMapper<CookingAssistantProfile>());
        }

        [Fact]
        public async Task ValidatesModel()
        {
            UpdateRecipe model = new RecipeBuilder().BuildUpdateModel();

            await _sut.UpdateAsync(model, _successfulValidatorMock.Object);

            _successfulValidatorMock.Verify(x => x.Validate(model), Times.Once);
        }

        [Fact]
        public async Task ValidateThrowsIfInvalidModel()
        {
            UpdateRecipe model = new RecipeBuilder().BuildUpdateModel();
            var failedValidator = ValidatorMocker.GetFailed<UpdateRecipe>();

            await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateAsync(model, failedValidator.Object));
        }

        [Fact]
        public async Task TrimsName()
        {
            string actualName = null;
            _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<List<int>>()))
                .Callback<Recipe, List<int>>((r, ids) => actualName = r.Name);

            UpdateRecipe model = new RecipeBuilder().WithName(" Recipe name ").BuildUpdateModel();

            await _sut.UpdateAsync(model, _successfulValidatorMock.Object);
            const string expected = "Recipe name";

            Assert.Equal(expected, actualName);
        }

        [Fact]
        public async Task TrimsDescriptionIfPresent()
        {
            string actualDescription = null;
            _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<List<int>>()))
                .Callback<Recipe, List<int>>((r, ids) => actualDescription = r.Description);

            UpdateRecipe model = new RecipeBuilder().WithDescription(" Description ").BuildUpdateModel();

            await _sut.UpdateAsync(model, _successfulValidatorMock.Object);
            const string expected = "Description";

            Assert.Equal(expected, actualDescription);
        }

        [Fact]
        public async Task TrimsRecipeIngredientNamesIfTheyAreNotLinkedToTasks()
        {
            List<RecipeIngredient> actualRecipeIngredients = null;
            _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<List<int>>()))
                .Callback<Recipe, List<int>>((r, ids) => actualRecipeIngredients = r.RecipeIngredients);

            UpdateRecipe model = new RecipeBuilder()
                .WithRecipeIngredients(" Ingredient 1", "Ingredient 2 ", " Ingredient 3 ").BuildUpdateModel();

            await _sut.UpdateAsync(model, _successfulValidatorMock.Object);
            var expectedRecipeIngredients = new List<RecipeIngredient>
            {
                new RecipeIngredient { Ingredient = new Ingredient { Name = "Ingredient 1" } },
                new RecipeIngredient { Ingredient = new Ingredient { Name = "Ingredient 2" } },
                new RecipeIngredient { Ingredient = new Ingredient { Name = "Ingredient 3" } }
            };

            for (var i = 0; i < expectedRecipeIngredients.Count; i++)
            {
                Assert.Equal(expectedRecipeIngredients[i].Ingredient.Name, actualRecipeIngredients[i].Ingredient.Name);
            }
        }

        [Fact]
        public async Task NullsRecipeIngredientNamesIfTheyAreLinkedToTasks()
        {
            List<RecipeIngredient> actualRecipeIngredients = null;
            _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<List<int>>()))
                .Callback<Recipe, List<int>>((r, ids) => actualRecipeIngredients = r.RecipeIngredients);

            UpdateRecipe model = new RecipeBuilder().WithRecipeIngredientsLinkedToTasks().BuildUpdateModel();

            await _sut.UpdateAsync(model, _successfulValidatorMock.Object);

            foreach (RecipeIngredient recipeIngredient in actualRecipeIngredients)
            {
                Assert.Null(recipeIngredient.Ingredient.Name);
            }
        }

        [Fact]
        public async Task SetsAmountOfRecipeIngredientsToNullIfAmountIsZero()
        {
            List<RecipeIngredient> actualRecipeIngredients = null;
            _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<List<int>>()))
                .Callback<Recipe, List<int>>((r, ids) => actualRecipeIngredients = r.RecipeIngredients);

            UpdateRecipe model = new RecipeBuilder().WithRecipeIngredientsWithAmounts(0, 0).BuildUpdateModel();

            await _sut.UpdateAsync(model, _successfulValidatorMock.Object);

            foreach (RecipeIngredient recipeIngredient in actualRecipeIngredients)
            {
                Assert.Null(recipeIngredient.Amount);
            }
        }

        [Fact]
        public async Task SetsUnitOfRecipeIngredientsToNullIfAmountIsZero()
        {
            List<RecipeIngredient> actualRecipeIngredients = null;
            _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<List<int>>()))
                .Callback<Recipe, List<int>>((r, ids) => actualRecipeIngredients = r.RecipeIngredients);

            UpdateRecipe model = new RecipeBuilder().WithRecipeIngredientsWithAmounts(0, 0).BuildUpdateModel();

            await _sut.UpdateAsync(model, _successfulValidatorMock.Object);

            foreach (RecipeIngredient recipeIngredient in actualRecipeIngredients)
            {
                Assert.Null(recipeIngredient.Unit);
            }
        }

        [Fact]
        public async Task SetsUnitOfRecipeIngredientsToNullIfAmountIsNull()
        {
            List<RecipeIngredient> actualRecipeIngredients = null;
            _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<List<int>>()))
                .Callback<Recipe, List<int>>((r, ids) => actualRecipeIngredients = r.RecipeIngredients);

            UpdateRecipe model = new RecipeBuilder().WithRecipeIngredientsWithAmounts(null, null).BuildUpdateModel();

            await _sut.UpdateAsync(model, _successfulValidatorMock.Object);

            foreach (RecipeIngredient recipeIngredient in actualRecipeIngredients)
            {
                Assert.Null(recipeIngredient.Unit);
            }
        }

        [Theory]
        [InlineData("Instructions Row 1\r\nRow 2", "Instructions Row 1\r\nRow 2")]
        [InlineData("Instructions Row 1\r\n\r\n\r\nRow 2", "Instructions Row 1\r\n\r\nRow 2")]
        [InlineData("\r\nInstructions Row 1\r\nRow 2\r\n\r\n\r\n", "Instructions Row 1\r\nRow 2")]
        public async Task CollapsesNewlinesInInstructionsToAtMostTwo(string instructions, string expected)
        {
            string actualInstructions = null;
            _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<List<int>>()))
                .Callback<Recipe, List<int>>((r, ids) => actualInstructions = r.Instructions);

            UpdateRecipe model = new RecipeBuilder().WithInstructions(instructions).BuildUpdateModel();

            await _sut.UpdateAsync(model, _successfulValidatorMock.Object);

            Assert.Equal(expected, actualInstructions);
        }

        [Fact]
        public async Task TrimsInstructionsIfPresent()
        {
            string actualInstructions = null;
            _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<List<int>>()))
                .Callback<Recipe, List<int>>((r, ids) => actualInstructions = r.Instructions);

            UpdateRecipe model = new RecipeBuilder().WithInstructions(" Instructions ").BuildUpdateModel();

            await _sut.UpdateAsync(model, _successfulValidatorMock.Object);
            const string expected = "Instructions";

            Assert.Equal(expected, actualInstructions);
        }

        [Fact]
        public async Task SetsPrepDurationToNullIfLowerThanOneMinute()
        {
            TimeSpan? actualPrepDuration = null;
            _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<List<int>>()))
                .Callback<Recipe, List<int>>((r, ids) => actualPrepDuration = r.PrepDuration);

            UpdateRecipe model = new RecipeBuilder().WithPrepDuration(TimeSpan.FromSeconds(59)).BuildUpdateModel();

            await _sut.UpdateAsync(model, _successfulValidatorMock.Object);

            Assert.Null(actualPrepDuration);
        }

        [Fact]
        public async Task SetsCookDurationToNullIfLowerThanOneMinute()
        {
            TimeSpan? actualCookDuration = null;
            _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<List<int>>()))
                .Callback<Recipe, List<int>>((r, ids) => actualCookDuration = r.CookDuration);

            UpdateRecipe model = new RecipeBuilder().WithCookDuration(TimeSpan.FromSeconds(59)).BuildUpdateModel();

            await _sut.UpdateAsync(model, _successfulValidatorMock.Object);

            Assert.Null(actualCookDuration);
        }
    }
}
