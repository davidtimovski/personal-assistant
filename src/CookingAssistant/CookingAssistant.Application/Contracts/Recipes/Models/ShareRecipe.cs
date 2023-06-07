using Core.Application.Contracts;
using FluentValidation;

namespace CookingAssistant.Application.Contracts.Recipes.Models;

public class ShareRecipe
{
    public int UserId { get; set; }
    public int RecipeId { get; set; }
    public List<int> NewShares { get; set; } = null!;
    public List<int> RemovedShares { get; set; } = null!;
}

public class ShareRecipeValidator : AbstractValidator<ShareRecipe>
{
    public ShareRecipeValidator(IRecipeService recipeService, IUserService userService)
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must((dto, userId) => recipeService.Exists(dto.RecipeId, userId)).WithMessage("Unauthorized");

        RuleFor(dto => dto.NewShares).Must((dto, newShares) =>
        {
            foreach (var userId in newShares)
            {
                if (!userService.Exists(userId))
                {
                    return false;
                }
            }
            return true;
        }).WithMessage("AnErrorOccurred");
        RuleFor(dto => dto.RemovedShares).Must((dto, removedShares) =>
        {
            foreach (var userId in removedShares)
            {
                if (!userService.Exists(userId))
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