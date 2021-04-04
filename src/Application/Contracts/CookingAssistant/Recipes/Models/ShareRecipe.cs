using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using PersonalAssistant.Application.Contracts.Common;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models
{
    public class ShareRecipe
    {
        public int UserId { get; set; }
        public int RecipeId { get; set; }
        public List<int> NewShares { get; set; }
        public List<int> RemovedShares { get; set; }
    }

    public class ShareRecipeValidator : AbstractValidator<ShareRecipe>
    {
        public ShareRecipeValidator(IRecipeService recipeService, IUserService userService)
        {
            RuleFor(dto => dto.UserId)
                .NotEmpty().WithMessage("Unauthorized")
                .MustAsync(async (dto, userId, val) => await recipeService.ExistsAsync(dto.RecipeId, userId)).WithMessage("Unauthorized");

            RuleFor(dto => dto.NewShares).MustAsync(async (dto, newShares, val) =>
            {
                foreach (var userId in newShares)
                {
                    if (!await userService.ExistsAsync(userId))
                    {
                        return false;
                    }
                }
                return true;
            }).WithMessage("AnErrorOccurred");
            RuleFor(dto => dto.RemovedShares).MustAsync(async (dto, removedShares, val) =>
            {
                foreach (var userId in removedShares)
                {
                    if (!await userService.ExistsAsync(userId))
                    {
                        return false;
                    }
                }
                return true;
            }).WithMessage("AnErrorOccurred");

            RuleFor(dto => dto).Must(dto =>
            {
                var userIds = dto.NewShares.Concat(dto.RemovedShares).ToList();

                return userIds.Count == userIds.Distinct().Count();
            }).WithMessage("AnErrorOccurred")
            .Must(dto =>
            {
                bool sharedWithCurrentUser = dto.NewShares.Any(x => x == dto.UserId)
                    || dto.RemovedShares.Any(x => x == dto.UserId);

                return !sharedWithCurrentUser;
            }).WithMessage("AnErrorOccurred");
        }
    }
}
