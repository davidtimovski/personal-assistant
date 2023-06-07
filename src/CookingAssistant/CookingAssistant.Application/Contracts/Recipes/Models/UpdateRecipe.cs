﻿using FluentValidation;

namespace CookingAssistant.Application.Contracts.Recipes.Models;

public class UpdateRecipe
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<UpdateRecipeIngredient> Ingredients { get; set; } = null!;
    public string? Instructions { get; set; }
    public TimeSpan? PrepDuration { get; set; }
    public TimeSpan? CookDuration { get; set; }
    public byte Servings { get; set; }
    public string? ImageUri { get; set; }
    public string? VideoUrl { get; set; }
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
            .Must(prepDuration => !prepDuration.HasValue || prepDuration >= TimeSpan.FromMinutes(0) && prepDuration < TimeSpan.FromMinutes(120)).WithMessage("Recipes.ModifyRecipe.PrepDurationRange");

        RuleFor(dto => dto.CookDuration)
            .Must(cookDuration => !cookDuration.HasValue || cookDuration >= TimeSpan.FromMinutes(0) && cookDuration < TimeSpan.FromMinutes(120)).WithMessage("Recipes.ModifyRecipe.CookDurationRange");

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
    public int? Id { get; set; }
    public string Name { get; set; } = null!;
    public float? Amount { get; set; }
    public string? Unit { get; set; }
}
