using System.Threading.Tasks;
using FluentValidation;
using Moq;
using PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients;
using PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients.Models;
using PersonalAssistant.Application.Mappings;
using PersonalAssistant.Application.Services.CookingAssistant;
using PersonalAssistant.Application.UnitTests.Builders;
using PersonalAssistant.Domain.Entities.CookingAssistant;
using Xunit;

namespace PersonalAssistant.Application.UnitTests.ServiceTests.IngredientServiceTests
{
    public class UpdateTests
    {
        private readonly Mock<IValidator<UpdateIngredient>> _successfulValidatorMock;
        private readonly Mock<IIngredientsRepository> _ingredientsRepositoryMock = new Mock<IIngredientsRepository>();
        private readonly IIngredientService _sut;

        public UpdateTests()
        {
            _successfulValidatorMock = ValidatorMocker.GetSuccessful<UpdateIngredient>();

            _sut = new IngredientService(
                null,
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
                .Callback<Ingredient>((i) => actualName = i.Name);

            UpdateIngredient model = new IngredientBuilder().WithName(" Ingredient name ").BuildUpdateModel();

            await _sut.UpdateAsync(model, _successfulValidatorMock.Object);
            const string expected = "Ingredient name";

            Assert.Equal(expected, actualName);
        }

        [Fact]
        public async Task NullsName_IfItsLinkedToTask()
        {
            string actualName = null;
            _ingredientsRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Ingredient>()))
                .Callback<Ingredient>((i) => actualName = i.Name);

            UpdateIngredient model = new IngredientBuilder().WithTaskId().BuildUpdateModel();

            await _sut.UpdateAsync(model, _successfulValidatorMock.Object);

            Assert.Null(actualName);
        }
    }
}
