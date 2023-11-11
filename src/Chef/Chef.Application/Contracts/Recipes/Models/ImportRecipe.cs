using Chef.Application.Contracts.Ingredients;
using FluentValidation;

namespace Chef.Application.Contracts.Recipes.Models;

public class ImportRecipe
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public IReadOnlyList<IngredientReplacement> IngredientReplacements { get; set; } = new List<IngredientReplacement>();
    public string ImageUri { get; set; } = null!;
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
