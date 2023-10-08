using Core.Application.Contracts;
using FluentValidation;

namespace Chef.Application.Contracts.Recipes.Models;

public class CreateSendRequest
{
    public required int UserId { get; init; }
    public required int RecipeId { get; init; }
    public required List<int> RecipientsIds { get; init; }
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
            .Must(recipientIds => recipientIds.Count == recipientIds.Distinct().Count()).WithMessage("AnErrorOccurred")
            .Must((dto, recipientIds) => !recipientIds.Any(id => id == dto.UserId)).WithMessage("AnErrorOccurred");
    }
}
