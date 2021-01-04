using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models
{
    public class CreateRecipe
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<UpdateRecipeIngredient> Ingredients { get; set; }
        public string Instructions { get; set; }
        public TimeSpan? PrepDuration { get; set; }
        public TimeSpan? CookDuration { get; set; }
        public byte Servings { get; set; }
        public string ImageUri { get; set; }
        public string VideoUrl { get; set; }
    }

    public class CreateRecipeValidator : AbstractValidator<CreateRecipe>
    {
        public CreateRecipeValidator(IRecipeService recipeService, ITaskService taskService)
        {
            RuleFor(dto => dto.UserId)
                .NotEmpty().WithMessage("Unauthorized")
                .MustAsync(async (dto, userId, val) => !await recipeService.ExistsAsync(dto.Name, userId)).WithMessage("AlreadyExists")
                .MustAsync(async (userId, val) => (await recipeService.CountAsync(userId)) < 250).WithMessage("Recipes.RecipeLimitReached");

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
            }).WithMessage("Recipes.ModifyRecipe.DuplicateIngredients")
            .MustAsync(async (dto, ingredients, val) =>
            {
                var ingredientsLinkedToTasks = ingredients.Where(x => x.TaskId.HasValue);
                foreach (UpdateRecipeIngredient ingredient in ingredientsLinkedToTasks)
                {
                    if (!await taskService.ExistsAsync(ingredient.TaskId.Value, dto.UserId))
                    {
                        return false;
                    }
                }
                return true;
            }).WithMessage("Recipes.ModifyRecipe.IngredientIsLinkedToNonExistentTask");

            RuleFor(dto => dto.Instructions).MaximumLength(5000).WithMessage("Recipes.ModifyRecipe.InstructionsMaxLength");

            RuleFor(dto => dto.PrepDuration)
                .Must(prepDuration => !prepDuration.HasValue || prepDuration >= TimeSpan.FromMinutes(0) && prepDuration < TimeSpan.FromMinutes(120)).WithMessage("Recipes.ModifyRecipe.PrepDurationRange");

            RuleFor(dto => dto.CookDuration)
                .Must(cookDuration => !cookDuration.HasValue || cookDuration >= TimeSpan.FromMinutes(0) && cookDuration < TimeSpan.FromMinutes(120)).WithMessage("Recipes.ModifyRecipe.CookDurationRange");

            RuleFor(dto => (int)dto.Servings)
                .InclusiveBetween(1, 50).WithMessage("Recipes.ModifyRecipe.ServingsMustBeBetween");

            RuleFor(dto => dto.VideoUrl)
                .Must(videoUrl => string.IsNullOrEmpty(videoUrl) || videoUrl.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) || videoUrl.Contains("youtu.be", StringComparison.OrdinalIgnoreCase)).WithMessage("Recipes.ModifyRecipe.OnlyYouTubeVideosAreCurrentlySupported");
        }
    }

    public class UpdateRecipeIngredientValidator : AbstractValidator<UpdateRecipeIngredient>
    {
        private readonly string[] units = new string[] { "g", "ml", "oz", "cup", "tbsp", "tsp" };

        public UpdateRecipeIngredientValidator()
        {
            RuleFor(dto => dto.Name)
                .Must((dto, name) =>
                {
                    return dto.TaskId.HasValue ? true : !string.IsNullOrWhiteSpace(name);
                }).WithMessage("Recipes.ModifyRecipe.IngredientNameIsRequired")
                .MaximumLength(50).WithMessage("Recipes.ModifyRecipe.IngredientNameMaxLength");

            RuleFor(dto => dto.Amount)
                .InclusiveBetween(0.1f, 10000).WithMessage("Recipes.ModifyRecipe.AmountMustBeBetween");

            RuleFor(dto => dto.Unit)
                .Must(unit => string.IsNullOrEmpty(unit) || units.Contains(unit)).WithMessage("Recipes.ModifyRecipe.InvalidUnit");
        }
    }
}
