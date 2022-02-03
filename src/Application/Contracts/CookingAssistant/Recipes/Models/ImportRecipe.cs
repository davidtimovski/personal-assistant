using System;
using System.Collections.Generic;
using FluentValidation;
using Application.Contracts.CookingAssistant.Ingredients;

namespace Application.Contracts.CookingAssistant.Recipes.Models;

public class ImportRecipe
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public IEnumerable<IngredientReplacement> IngredientReplacements { get; set; }
    public string ImageUri { get; set; }
}

public class IngredientReplacement
{
    public int Id { get; set; }
    public int ReplacementId { get; set; }
    public bool TransferNutritionData { get; set; }
    public bool TransferPriceData { get; set; }
}

public class ImportRecipeValidator : AbstractValidator<ImportRecipe>
{
    public ImportRecipeValidator(IRecipeService recipeService, IIngredientService ingredientService)
    {
        RuleFor(dto => dto.Id).NotEmpty().WithMessage("AnErrorOccurred");

        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must((dto, userId) => recipeService.SendRequestExists(dto.Id, userId)).WithMessage("Unauthorized");

        RuleForEach(dto => dto.IngredientReplacements).Must((dto, replacements) =>
        {
            foreach (IngredientReplacement replacement in dto.IngredientReplacements)
            {
                if (!ingredientService.ExistsInRecipe(replacement.Id, dto.Id))
                {
                    return false;
                }
            }
            return true;
        }).WithMessage("AnErrorOccurred");
    }
}