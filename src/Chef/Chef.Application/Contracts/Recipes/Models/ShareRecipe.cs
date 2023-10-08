using Core.Application.Contracts;
using FluentValidation;

namespace Chef.Application.Contracts.Recipes.Models;

public class ShareRecipe
{
    public required int UserId { get; init; }
    public required int RecipeId { get; init; }
    public required List<int> NewShares { get; init; }
    public required List<int> RemovedShares { get; init; }
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