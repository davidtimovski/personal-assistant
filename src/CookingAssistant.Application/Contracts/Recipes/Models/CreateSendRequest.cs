using Application.Contracts;
using FluentValidation;

namespace CookingAssistant.Application.Contracts.Recipes.Models;

public class CreateSendRequest
{
    public int RecipeId { get; set; }
    public int UserId { get; set; }
    public List<int> RecipientsIds { get; set; }
}

public class CreateSendRequestValidator : AbstractValidator<CreateSendRequest>
{
    public CreateSendRequestValidator(IRecipeService recipeService, IUserService userService)
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must((dto, userId) => recipeService.Exists(dto.RecipeId, userId)).WithMessage("Unauthorized")
            .Must(userId => recipeService.Count(userId) < 250).WithMessage("RecipeLimitReached");

        RuleFor(dto => dto.RecipientsIds)
            .Must(recipientIds =>
            {
                return recipientIds.Count == recipientIds.Distinct().Count();
            }).WithMessage("AnErrorOccurred")
            .Must((dto, recipientIds) =>
            {
                return !recipientIds.Any(id => id == dto.UserId);
            }).WithMessage("AnErrorOccurred");
    }
}