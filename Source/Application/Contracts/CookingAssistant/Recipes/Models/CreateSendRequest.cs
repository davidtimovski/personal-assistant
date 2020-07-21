using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using PersonalAssistant.Application.Contracts.Common;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models
{
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
                .MustAsync(async (dto, userId, val) => !await recipeService.ExistsAsync(dto.RecipeId, userId)).WithMessage("AlreadyExists")
                .MustAsync(async (userId, val) => (await recipeService.CountAsync(userId)) < 250).WithMessage("RecipeLimitReached");

            RuleFor(dto => dto.RecipientsIds)
                .Must(recipientIds =>
                {
                    return recipientIds.Count == recipientIds.Distinct().Count();
                }).WithMessage("AnErrorOccurred")
                .Must((dto, recipientIds, val) =>
                {
                    return !recipientIds.Any(id => id == dto.UserId);
                }).WithMessage("AnErrorOccurred");
        }
    }
}
