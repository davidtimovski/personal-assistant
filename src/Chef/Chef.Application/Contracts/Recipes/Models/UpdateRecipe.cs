using FluentValidation;

namespace Chef.Application.Contracts.Recipes.Models;

public class UpdateRecipe
{
    public required int UserId { get; init; }
    public required int Id { get; init; }
    public required string Name { get; init; } = null!;
    public required string? Description { get; init; }
    public required List<UpdateRecipeIngredient> Ingredients { get; init; }
    public required string? Instructions { get; init; }
    public required TimeSpan PrepDuration { get; init; }
    public required TimeSpan CookDuration { get; init; }
    public required byte Servings { get; init; }
    public required string? ImageUri { get; init; }
    public required string? VideoUrl { get; init; }
}

public class UpdateRecipeValidator : AbstractValidator<UpdateRecipe>
{
    public UpdateRecipeValidator(IRecipeService recipeService)
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must((dto, userId) => recipeService.Exists(dto.Id, userId)).WithMessage("Unauthorized")
            .Must((dto, userId) => !recipeService.Exists(dto.Id, dto.Name, userId)).WithMessage("AlreadyExists")
            .Must((userId) => recipeService.Count(userId) < 250).WithMessage("Recipes.RecipeLimitReached");

        RuleFor(dto => dto.Name)
            .NotEmpty().WithMessage("Recipes.ModifyRecipe.NameIsRequired")
            .MaximumLength(50).WithMessage("Recipes.ModifyRecipe.NameMaxLength");

        RuleFor(dto => dto.Description).MaximumLength(250).WithMessage("Recipes.ModifyRecipe.DescriptionMaxLength");

        RuleForEach(dto => dto.Ingredients).SetValidator(new UpdateRecipeIngredientValidator());

        RuleFor(dto => dto.Ingredients).Must(ingredients =>
            {
                foreach (var ingredient in ingredients)
                {
                    if (ingredients.Where(x => string.Equals(x.Name.Trim(), ingredient.Name.Trim(), StringComparison.OrdinalIgnoreCase)).Count() > 1)
                    {
                        return false;
                    }
                }
                return true;
            }).WithMessage("Recipes.ModifyRecipe.DuplicateIngredients");

        RuleFor(dto => dto.Instructions).MaximumLength(5000).WithMessage("Recipes.ModifyRecipe.InstructionsMaxLength");

        RuleFor(dto => dto.PrepDuration)
            .Must(prepDuration => prepDuration >= TimeSpan.FromMinutes(0) && prepDuration < TimeSpan.FromHours(5)).WithMessage("Recipes.ModifyRecipe.PrepDurationRange");

        RuleFor(dto => dto.CookDuration)
            .Must(cookDuration => cookDuration >= TimeSpan.FromMinutes(0) && cookDuration < TimeSpan.FromHours(5)).WithMessage("Recipes.ModifyRecipe.CookDurationRange");

        RuleFor(dto => (int)dto.Servings)
            .InclusiveBetween(1, 50).WithMessage("Recipes.ModifyRecipe.ServingsMustBeBetween");

        RuleFor(dto => dto.VideoUrl)
            .Must(videoUrl => string.IsNullOrEmpty(videoUrl)
                              || videoUrl.Contains("youtube.com", StringComparison.OrdinalIgnoreCase)
                              || videoUrl.Contains("youtu.be", StringComparison.OrdinalIgnoreCase)).WithMessage("Recipes.ModifyRecipe.OnlyYouTubeVideosAreCurrentlySupported");
    }
}

public class UpdateRecipeIngredient
{
    public required int? Id { get; init; }
    public required string Name { get; init; }
    public required float? Amount { get; init; }
    public required string? Unit { get; init; }
}
