using Application.UnitTests.Builders;
using CookingAssistant.Application.Contracts.Recipes;
using CookingAssistant.Application.Contracts.Recipes.Models;
using CookingAssistant.Application.Mappings;
using CookingAssistant.Application.Services;
using Application.Domain.CookingAssistant;
using FluentValidation;
using Moq;
using Xunit;

namespace Application.UnitTests.ServiceTests.RecipeServiceTests;

public class CreateTests
{
    private readonly Mock<IValidator<CreateRecipe>> _successfulValidatorMock;
    private readonly Mock<IRecipesRepository> _recipesRepositoryMock = new();
    private readonly IRecipeService _sut;

    public CreateTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<CreateRecipe>();

        _sut = new RecipeService(null, null, null, null,
            _recipesRepositoryMock.Object,
            null,
            MapperMocker.GetMapper<CookingAssistantProfile>(),
            null);
    }

    [Fact]
    public async Task ValidatesModel()
    {
        CreateRecipe model = new RecipeBuilder().BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object);

        _successfulValidatorMock.Verify(x => x.Validate(model));
    }

    [Fact]
    public async Task Validate_Throws_IfInvalidModel()
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
            .Callback<Recipe>(r => actualName = r.Name);

        CreateRecipe model = new RecipeBuilder().WithName(" Recipe name ").BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object);
        const string expected = "Recipe name";

        Assert.Equal(expected, actualName);
    }

    [Fact]
    public async Task TrimsDescription_IfPresent()
    {
        string actualDescription = null;
        _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
            .Callback<Recipe>(r => actualDescription = r.Description);

        CreateRecipe model = new RecipeBuilder().WithDescription(" Description ").BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object);
        const string expected = "Description";

        Assert.Equal(expected, actualDescription);
    }

    [Fact]
    public async Task TrimsRecipeIngredientNames_IfTheyAreNotLinkedToTasks()
    {
        List<RecipeIngredient> actualRecipeIngredients = null;
        _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
            .Callback<Recipe>(r => actualRecipeIngredients = r.RecipeIngredients);

        CreateRecipe model = new RecipeBuilder()
            .WithRecipeIngredients(" Ingredient 1", "Ingredient 2 ", " Ingredient 3 ").BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object);
        var expectedRecipeIngredients = new List<RecipeIngredient>
        {
            new() { Ingredient = new Ingredient { Name = "Ingredient 1" } },
            new() { Ingredient = new Ingredient { Name = "Ingredient 2" } },
            new() { Ingredient = new Ingredient { Name = "Ingredient 3" } }
        };

        for (var i = 0; i < expectedRecipeIngredients.Count; i++)
        {
            Assert.Equal(expectedRecipeIngredients[i].Ingredient.Name, actualRecipeIngredients[i].Ingredient.Name);
        }
    }

    [Fact]
    public async Task SetsAmountOfRecipeIngredientsToNull_IfAmountIsZero()
    {
        List<RecipeIngredient> actualRecipeIngredients = null;
        _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
            .Callback<Recipe>(r => actualRecipeIngredients = r.RecipeIngredients);

        CreateRecipe model = new RecipeBuilder().WithRecipeIngredientsWithAmounts(0, 0).BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object);

        foreach (RecipeIngredient recipeIngredient in actualRecipeIngredients)
        {
            Assert.Null(recipeIngredient.Amount);
        }
    }

    [Fact]
    public async Task SetsUnitOfRecipeIngredientsToNull_IfAmountIsZero()
    {
        List<RecipeIngredient> actualRecipeIngredients = null;
        _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
            .Callback<Recipe>(r => actualRecipeIngredients = r.RecipeIngredients);

        CreateRecipe model = new RecipeBuilder().WithRecipeIngredientsWithAmounts(0, 0).BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object);

        foreach (RecipeIngredient recipeIngredient in actualRecipeIngredients)
        {
            Assert.Null(recipeIngredient.Unit);
        }
    }

    [Fact]
    public async Task SetsUnitOfRecipeIngredientsToNull_IfAmountIsNull()
    {
        List<RecipeIngredient> actualRecipeIngredients = null;
        _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
            .Callback<Recipe>(r => actualRecipeIngredients = r.RecipeIngredients);

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
            .Callback<Recipe>(r => actualInstructions = r.Instructions);

        CreateRecipe model = new RecipeBuilder().WithInstructions(instructions).BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object);

        Assert.Equal(expected, actualInstructions);
    }

    [Fact]
    public async Task TrimsInstructions_IfPresent()
    {
        string actualInstructions = null;
        _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
            .Callback<Recipe>(r => actualInstructions = r.Instructions);

        CreateRecipe model = new RecipeBuilder().WithInstructions(" Instructions ").BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object);
        const string expected = "Instructions";

        Assert.Equal(expected, actualInstructions);
    }

    [Fact]
    public async Task SetsPrepDurationToNull_IfLowerThanOneMinute()
    {
        TimeSpan? actualPrepDuration = null;
        _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
            .Callback<Recipe>(r => actualPrepDuration = r.PrepDuration);

        CreateRecipe model = new RecipeBuilder().WithPrepDuration(TimeSpan.FromSeconds(59)).BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object);

        Assert.Null(actualPrepDuration);
    }

    [Fact]
    public async Task SetsCookDurationToNull_IfLowerThanOneMinute()
    {
        TimeSpan? actualCookDuration = null;
        _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
            .Callback<Recipe>(r => actualCookDuration = r.CookDuration);

        CreateRecipe model = new RecipeBuilder().WithCookDuration(TimeSpan.FromSeconds(59)).BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object);

        Assert.Null(actualCookDuration);
    }

    [Fact]
    public async Task SetsCreatedDate()
    {
        var actualCreatedDate = new DateTime();
        _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
            .Callback<Recipe>(r => actualCreatedDate = r.CreatedDate);

        CreateRecipe model = new RecipeBuilder().BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object);

        Assert.NotEqual(DateTime.MinValue, actualCreatedDate);
    }

    [Fact]
    public async Task SetsModifiedDate()
    {
        var actualModifiedDate = new DateTime();
        _recipesRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Recipe>()))
            .Callback<Recipe>(r => actualModifiedDate = r.ModifiedDate);

        CreateRecipe model = new RecipeBuilder().BuildCreateModel();

        await _sut.CreateAsync(model, _successfulValidatorMock.Object);

        Assert.NotEqual(DateTime.MinValue, actualModifiedDate);
    }
}
