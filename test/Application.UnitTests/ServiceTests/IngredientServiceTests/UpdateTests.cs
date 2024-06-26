﻿using Application.UnitTests.Builders;
using Chef.Application.Contracts.Ingredients;
using Chef.Application.Contracts.Ingredients.Models;
using Chef.Application.Entities;
using Chef.Application.Mappings;
using Chef.Application.Services;
using Core.Application.Contracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.UnitTests.ServiceTests.IngredientServiceTests;

public class UpdateTests
{
    private readonly Mock<IValidator<UpdateIngredient>> _successfulValidatorMock;
    private readonly Mock<IIngredientsRepository> _ingredientsRepositoryMock = new();
    private readonly Mock<ISpan> _metricsSpanMock = new();
    private readonly IIngredientService _sut;

    public UpdateTests()
    {
        _successfulValidatorMock = ValidatorMocker.GetSuccessful<UpdateIngredient>();

        _metricsSpanMock.Setup(x => x.StartChild(It.IsAny<string>())).Returns(new Mock<ISpan>().Object);

        _sut = new IngredientService(
            new Mock<IUserService>().Object,
            _ingredientsRepositoryMock.Object,
            MapperMocker.GetMapper<ChefProfile>(),
            new Mock<ILogger<IngredientService>>().Object);
    }

    [Fact]
    public async Task ValidatesModel()
    {
        UpdateIngredient model = new IngredientBuilder().BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());

        _successfulValidatorMock.Verify(x => x.Validate(model));
    }

    [Fact]
    public async Task Validate_Throws_IfInvalidModel()
    {
        UpdateIngredient model = new IngredientBuilder().BuildUpdateModel();
        var failedValidator = ValidatorMocker.GetFailed<UpdateIngredient>();

        await Assert.ThrowsAsync<ValidationException>(() => _sut.UpdateAsync(model, failedValidator.Object, It.IsAny<ISpan>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task TrimsName_IfItsNotLinkedToTask()
    {
        string? actualName = null;
        _ingredientsRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Ingredient>(), It.IsAny<ISpan>(), It.IsAny<CancellationToken>()))
            .Callback<Ingredient, ISpan, CancellationToken>((i, _, _) => actualName = i.Name);

        UpdateIngredient model = new IngredientBuilder().WithName(" Ingredient name ").BuildUpdateModel();

        await _sut.UpdateAsync(model, _successfulValidatorMock.Object, _metricsSpanMock.Object, It.IsAny<CancellationToken>());
        const string expected = "Ingredient name";

        Assert.Equal(expected, actualName);
    }
}
