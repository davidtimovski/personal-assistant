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
    public class CreateTests
    {
        private readonly Mock<IValidator<CreateRecipe>> _successfulValidatorMock;
        private readonly Mock<IRecipesRepository> _recipesRepositoryMock = new Mock<IRecipesRepository>();
        private readonly IRecipeService _sut;

        public CreateTests()
        {
            _successfulValidatorMock = ValidatorMocker.GetSuccessful<CreateRecipe>();

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
            CreateRecipe model = new RecipeBuilder().BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);

            _successfulValidatorMock.Verify(x => x.Validate(model));
        }

        [Fact]
        public async Task ValidateThrowsIfInvalidModel()
        {
            CreateRecipe model = new RecipeBuilder().BuildCreateModel();
            var failedValidator = ValidatorMocker.GetFailed<CreateRecipe>();

            await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(model, failedValidator.Object));
        }

        [Fact]
        public async Task TrimsName()
        {
            string actualName = null;
            _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>((r) => actualName = r.Name);

            CreateRecipe model = new RecipeBuilder().WithName(" Recipe name ").BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);
            const string expected = "Recipe name";

            Assert.Equal(expected, actualName);
        }

        [Fact]
        public async Task TrimsDescriptionIfPresent()
        {
            string actualDescription = null;
            _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>((r) => actualDescription = r.Description);

            CreateRecipe model = new RecipeBuilder().WithDescription(" Description ").BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);
            const string expected = "Description";

            Assert.Equal(expected, actualDescription);
        }

        [Fact]
        public async Task TrimsRecipeIngredientNamesIfTheyAreNotLinkedToTasks()
        {
            List<RecipeIngredient> actualRecipeIngredients = null;
            _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>((r) => actualRecipeIngredients = r.RecipeIngredients);

            CreateRecipe model = new RecipeBuilder()
                .WithRecipeIngredients(" Ingredient 1", "Ingredient 2 ", " Ingredient 3 ").BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);
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
            _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>((r) => actualRecipeIngredients = r.RecipeIngredients);

            CreateRecipe model = new RecipeBuilder().WithRecipeIngredientsLinkedToTasks().BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);

            foreach (RecipeIngredient recipeIngredient in actualRecipeIngredients)
            {
                Assert.Null(recipeIngredient.Ingredient.Name);
            }
        }

        [Fact]
        public async Task SetsAmountOfRecipeIngredientsToNullIfAmountIsZero()
        {
            List<RecipeIngredient> actualRecipeIngredients = null;
            _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>((r) => actualRecipeIngredients = r.RecipeIngredients);

            CreateRecipe model = new RecipeBuilder().WithRecipeIngredientsWithAmounts(0, 0).BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);

            foreach (RecipeIngredient recipeIngredient in actualRecipeIngredients)
            {
                Assert.Null(recipeIngredient.Amount);
            }
        }

        [Fact]
        public async Task SetsUnitOfRecipeIngredientsToNullIfAmountIsZero()
        {
            List<RecipeIngredient> actualRecipeIngredients = null;
            _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>((r) => actualRecipeIngredients = r.RecipeIngredients);

            CreateRecipe model = new RecipeBuilder().WithRecipeIngredientsWithAmounts(0, 0).BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);

            foreach (RecipeIngredient recipeIngredient in actualRecipeIngredients)
            {
                Assert.Null(recipeIngredient.Unit);
            }
        }

        [Fact]
        public async Task SetsUnitOfRecipeIngredientsToNullIfAmountIsNull()
        {
            List<RecipeIngredient> actualRecipeIngredients = null;
            _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>((r) => actualRecipeIngredients = r.RecipeIngredients);

            CreateRecipe model = new RecipeBuilder().WithRecipeIngredientsWithAmounts(null, null).BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);

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
            _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>((r) => actualInstructions = r.Instructions);

            CreateRecipe model = new RecipeBuilder().WithInstructions(instructions).BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);

            Assert.Equal(expected, actualInstructions);
        }

        [Fact]
        public async Task TrimsInstructionsIfPresent()
        {
            string actualInstructions = null;
            _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>((r) => actualInstructions = r.Instructions);

            CreateRecipe model = new RecipeBuilder().WithInstructions(" Instructions ").BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);
            const string expected = "Instructions";

            Assert.Equal(expected, actualInstructions);
        }

        [Fact]
        public async Task SetsPrepDurationToNullIfLowerThanOneMinute()
        {
            TimeSpan? actualPrepDuration = null;
            _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>((r) => actualPrepDuration = r.PrepDuration);

            CreateRecipe model = new RecipeBuilder().WithPrepDuration(TimeSpan.FromSeconds(59)).BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);

            Assert.Null(actualPrepDuration);
        }

        [Fact]
        public async Task SetsCookDurationToNullIfLowerThanOneMinute()
        {
            TimeSpan? actualCookDuration = null;
            _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>((r) => actualCookDuration = r.CookDuration);

            CreateRecipe model = new RecipeBuilder().WithCookDuration(TimeSpan.FromSeconds(59)).BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);

            Assert.Null(actualCookDuration);
        }

        [Fact]
        public async Task SetsCreatedDate()
        {
            var actualCreatedDate = new DateTime();
            _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>((r) => actualCreatedDate = r.CreatedDate);

            CreateRecipe model = new RecipeBuilder().BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);

            Assert.NotEqual(DateTime.MinValue, actualCreatedDate);
        }

        [Fact]
        public async Task SetsModifiedDate()
        {
            var actualModifiedDate = new DateTime();
            _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
                .Callback<Recipe>((r) => actualModifiedDate = r.ModifiedDate);

            CreateRecipe model = new RecipeBuilder().BuildCreateModel();

            await _sut.CreateAsync(model, _successfulValidatorMock.Object);

            Assert.NotEqual(DateTime.MinValue, actualModifiedDate);
        }
    }
}
