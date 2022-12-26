using Application.Domain.CookingAssistant;
using Application.UnitTests.Builders;
using CookingAssistant.Application.Contracts.Ingredients;
using CookingAssistant.Application.Contracts.Ingredients.Models;
using CookingAssistant.Application.Mappings;
using CookingAssistant.Application.Services;
using FluentValidation;
using Moq;
using Xunit;

namespace Application.UnitTests.ServiceTests.IngredientServiceTests;

public class GetUserSuggestionsTests
{
    private readonly Mock<IValidator<UpdateIngredient>> _successfulValidatorMock;
    private readonly Mock<IIngredientsRepository> _ingredientsRepositoryMock = new();
    private readonly IIngredientService _sut;

    public GetUserSuggestionsTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<UpdateIngredient>();

        _sut = new IngredientService(
            _ingredientsRepositoryMock.Object,
            MapperMocker.GetMapper<CookingAssistantProfile>(),
            null);
    }

    [Fact]
    public void GetUserSuggestions_SetsMostUsedMetricUnit()
    {
        var ingredientWithRecipeIngredients = new IngredientBuilder().WithRecipeIngredientUnits(new string[] { "oz", "g", "ml", "ml", "oz", "oz", "pinch" }).BuildModel();

        _ingredientsRepositoryMock.Setup(x => x.GetForSuggestions(It.IsAny<int>()))
            .Returns(new List<Ingredient> { ingredientWithRecipeIngredients });

        var suggestions = _sut.GetUserSuggestions(It.IsAny<int>()).ToList();
        const string expectedUnit = "ml";

        Assert.Equal(expectedUnit, suggestions[0].Unit);
    }

    [Fact]
    public void GetUserSuggestions_SetsMostUsedImperialUnit()
    {
        var ingredientWithRecipeIngredients = new IngredientBuilder().WithRecipeIngredientUnits(new string[] { "oz", "g", "oz", "ml", "g", "g", "pinch" }).BuildModel();

        _ingredientsRepositoryMock.Setup(x => x.GetForSuggestions(It.IsAny<int>()))
            .Returns(new List<Ingredient> { ingredientWithRecipeIngredients });

        var suggestions = _sut.GetUserSuggestions(It.IsAny<int>()).ToList();
        const string expectedImperialUnit = "oz";

        Assert.Equal(expectedImperialUnit, suggestions[0].UnitImperial);
    }
}
