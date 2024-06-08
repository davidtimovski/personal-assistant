using Application.UnitTests.Builders;
using Chef.Application.Contracts.DietaryProfiles;
using Chef.Application.Contracts.Recipes;
using Chef.Application.Contracts.Recipes.Models;
using Chef.Application.Entities;
using Chef.Application.Mappings;
using Chef.Application.Services;
using Chef.Utility;
using Core.Application.Contracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.UnitTests.ServiceTests.RecipeServiceTests;

public class UpdateTests
{
    private readonly Mock<IValidator<UpdateRecipe>> _successfulValidatorMock;
    private readonly Mock<IRecipesRepository> _recipesRepositoryMock = new();
    private readonly Mock<ISpan> _metricsSpanMock = new();
    private readonly IRecipeService _sut;

    public UpdateTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<UpdateRecipe>();

        _metricsSpanMock.Setup(x => x.StartChild(It.IsAny<string>())).Returns(new Mock<ISpan>().Object);

        _sut = new RecipeService(
            new Mock<IDietaryProfileService>().Object,
            new Mock<IConversion>().Object,
            new Mock<ICdnService>().Object,
            new Mock<IUserService>().Object,
            _recipesRepositoryMock.Object,
            new Mock<ICurrenciesRepository>().Object,
            MapperMocker.GetMapper<ChefProfile>(),
            new Mock<ILogger<RecipeService>>().Object);
    }

    [Fact]
    public async Task ValidatesModel()
    {
        _recipesRepositoryMock.Setup(x => x.GetUsersToBeNotifiedOfRecipeChange(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns(new List<Chef.Application.Entities.User>());

        UpdateRecipe model = new RecipeBuilder().BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        _successfulValidatorMock.Verify(x => x.Validate(model));
    }

    [Fact]
    public async Task Validate_Throws_IfInvalidModel()
    {
        UpdateRecipe model = new RecipeBuilder().BuildUpdateModel();
        var failedValidator = ValidatorMocker.GetFailed<UpdateRecipe>();

        await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateAsync(model, failedValidator.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TrimsName()
    {
        string? actualName = null;
        _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<int>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<Recipe, int, ISpan, CancellationToken>((r, _, _, _) => actualName = r.Name);
        _recipesRepositoryMock.Setup(x => x.GetUsersToBeNotifiedOfRecipeChange(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns(new List<Chef.Application.Entities.User>());

        UpdateRecipe model = new RecipeBuilder().WithName(" Recipe name ").BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());
        const string expected = "Recipe name";

        Assert.Equal(expected, actualName);
    }

    [Fact]
    public async Task TrimsDescription_IfPresent()
    {
        string? actualDescription = null;
        _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<int>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<Recipe, int, ISpan, CancellationToken>((r, _, _, _) => actualDescription = r.Description);
        _recipesRepositoryMock.Setup(x => x.GetUsersToBeNotifiedOfRecipeChange(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns(new List<Chef.Application.Entities.User>());

        UpdateRecipe model = new RecipeBuilder().WithDescription(" Description ").BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());
        const string expected = "Description";

        Assert.Equal(expected, actualDescription);
    }

    [Fact]
    public async Task TrimsRecipeIngredientNames_IfTheyAreNotLinkedToTasks()
    {
        List<RecipeIngredient> actualRecipeIngredients = null!;
        _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<int>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<Recipe, int, ISpan, CancellationToken>((r, _, _, _) => actualRecipeIngredients = r.RecipeIngredients);
        _recipesRepositoryMock.Setup(x => x.GetUsersToBeNotifiedOfRecipeChange(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns(new List<Chef.Application.Entities.User>());

        UpdateRecipe model = new RecipeBuilder()
            .WithRecipeIngredients(" Ingredient 1", "Ingredient 2 ", " Ingredient 3 ").BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());
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
        List<RecipeIngredient> actualRecipeIngredients = null!;
        _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<int>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<Recipe, int, ISpan, CancellationToken>((r, _, _, _) => actualRecipeIngredients = r.RecipeIngredients);
        _recipesRepositoryMock.Setup(x => x.GetUsersToBeNotifiedOfRecipeChange(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns(new List<Chef.Application.Entities.User>());

        UpdateRecipe model = new RecipeBuilder().WithRecipeIngredientsWithAmounts(0, 0).BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        foreach (RecipeIngredient recipeIngredient in actualRecipeIngredients)
        {
            Assert.Null(recipeIngredient.Amount);
        }
    }

    [Fact]
    public async Task SetsUnitOfRecipeIngredientsToNull_IfAmountIsZero()
    {
        List<RecipeIngredient> actualRecipeIngredients = null!;
        _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<int>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<Recipe, int, ISpan, CancellationToken>((r, _, _, _) => actualRecipeIngredients = r.RecipeIngredients);
        _recipesRepositoryMock.Setup(x => x.GetUsersToBeNotifiedOfRecipeChange(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns(new List<Chef.Application.Entities.User>());

        UpdateRecipe model = new RecipeBuilder().WithRecipeIngredientsWithAmounts(0, 0).BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        foreach (RecipeIngredient recipeIngredient in actualRecipeIngredients)
        {
            Assert.Null(recipeIngredient.Unit);
        }
    }

    [Fact]
    public async Task SetsUnitOfRecipeIngredientsToNull_IfAmountIsNull()
    {
        List<RecipeIngredient> actualRecipeIngredients = null!;
        _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<int>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<Recipe, int, ISpan, CancellationToken>((r, _, _, _) => actualRecipeIngredients = r.RecipeIngredients);
        _recipesRepositoryMock.Setup(x => x.GetUsersToBeNotifiedOfRecipeChange(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns(new List<Chef.Application.Entities.User>());

        UpdateRecipe model = new RecipeBuilder().WithRecipeIngredientsWithAmounts(null, null).BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

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
        string? actualInstructions = null;
        _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<int>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<Recipe, int, ISpan, CancellationToken>((r, _, _, _) => actualInstructions = r.Instructions);
        _recipesRepositoryMock.Setup(x => x.GetUsersToBeNotifiedOfRecipeChange(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns(new List<Chef.Application.Entities.User>());
        _recipesRepositoryMock.Setup(x => x.GetUsersToBeNotifiedOfRecipeChange(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns(new List<Chef.Application.Entities.User>());

        UpdateRecipe model = new RecipeBuilder().WithInstructions(instructions).BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        Assert.Equal(expected, actualInstructions);
    }

    [Fact]
    public async Task TrimsInstructions_IfPresent()
    {
        string? actualInstructions = null;
        _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<int>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<Recipe, int, ISpan, CancellationToken>((r, _, _, _) => actualInstructions = r.Instructions);
        _recipesRepositoryMock.Setup(x => x.GetUsersToBeNotifiedOfRecipeChange(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns(new List<Chef.Application.Entities.User>());

        UpdateRecipe model = new RecipeBuilder().WithInstructions(" Instructions ").BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());
        const string expected = "Instructions";

        Assert.Equal(expected, actualInstructions);
    }

    [Fact]
    public async Task SetsPrepDurationToNull_IfLowerThanOneMinute()
    {
        TimeSpan? actualPrepDuration = null;
        _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<int>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<Recipe, int, ISpan, CancellationToken>((r, _, _, _) => actualPrepDuration = r.PrepDuration);
        _recipesRepositoryMock.Setup(x => x.GetUsersToBeNotifiedOfRecipeChange(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns(new List<Chef.Application.Entities.User>());

        UpdateRecipe model = new RecipeBuilder().WithPrepDuration(TimeSpan.FromSeconds(59)).BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        Assert.Null(actualPrepDuration);
    }

    [Fact]
    public async Task SetsCookDurationToNull_IfLowerThanOneMinute()
    {
        TimeSpan? actualCookDuration = null;
        _recipesRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Recipe>(), It.IsAny<int>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<Recipe, int, ISpan, CancellationToken>((r, _, _, _) => actualCookDuration = r.CookDuration);
        _recipesRepositoryMock.Setup(x => x.GetUsersToBeNotifiedOfRecipeChange(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ISpan>()))
            .Returns(new List<Chef.Application.Entities.User>());

        UpdateRecipe model = new RecipeBuilder().WithCookDuration(TimeSpan.FromSeconds(59)).BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        Assert.Null(actualCookDuration);
    }
}
