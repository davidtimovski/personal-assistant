using System.Threading.Tasks;
using Application.Contracts.CookingAssistant.Ingredients;
using Application.Contracts.CookingAssistant.Ingredients.Models;
using Application.Mappings;
using Application.Services.CookingAssistant;
using Application.UnitTests.Builders;
using Domain.Entities.CookingAssistant;
using FluentValidation;
using Moq;
using Xunit;

namespace Application.UnitTests.ServiceTests.IngredientServiceTests;

public class UpdateTests
{
    private readonly Mock<IValidator<UpdateIngredient>> _successfulValidatorMock;
    private readonly Mock<IIngredientsRepository> _ingredientsRepositoryMock = new();
    private readonly IIngredientService _sut;

    public UpdateTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<UpdateIngredient>();

        _sut = new IngredientService(
            _ingredientsRepositoryMock.Object,
            MapperMocker.GetMapper<CookingAssistantProfile>());
    }

    [Fact]
    public async Task ValidatesModel()
    {
        UpdateIngredient model = new IngredientBuilder().BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object);

        _successfulValidatorMock.Verify(x => x.Validate(model));
    }

    [Fact]
    public async Task Validate_Throws_IfInvalidModel()
    {
        UpdateIngredient model = new IngredientBuilder().BuildUpdateModel();
        var failedValidator = ValidatorMocker.GetFailed<UpdateIngredient>();

        await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateAsync(model, failedValidator.Object));
    }

    [Fact]
    public async Task TrimsName_IfItsNotLinkedToTask()
    {
        string actualName = null;
        _ingredientsRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Ingredient>()))
            .Callback<Ingredient>(i => actualName = i.Name);

        UpdateIngredient model = new IngredientBuilder().WithName(" Ingredient name ").BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object);
        const string expected = "Ingredient name";

        Assert.Equal(expected, actualName);
    }
}
