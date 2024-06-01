using Core.Application.Contracts;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace PersonalAssistant.Web.ViewModels.Friendships;

public class UpdateFriendshipViewModel
{
    public required int FriendId { get; init; }
    public required IReadOnlyList<string> Permissions { get; init; }
}

public class UpdateFriendshipViewModelValidator : AbstractValidator<UpdateFriendshipViewModel>
{
    public UpdateFriendshipViewModelValidator(IUsersRepository usersRepository, IStringLocalizer<UpdateFriendshipViewModelValidator> localizer)
    {
        RuleFor(dto => dto.FriendId).Must((dto, friendId) => usersRepository.Exists(friendId)).WithMessage(localizer["DoesNotExist"]);

        RuleFor(dto => dto.Permissions).NotEmpty().WithMessage(localizer["PermissionsAreRequired"]);
    }
}
