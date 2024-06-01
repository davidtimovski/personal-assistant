using FluentValidation;
using Microsoft.Extensions.Localization;

namespace PersonalAssistant.Web.ViewModels.Friendships;

public class AddFriendViewModel
{
    public string Email { get; set; } = null!;
    public HashSet<string> Permissions { get; set; } = null!;
}

public class AddFriendViewModelValidator : AbstractValidator<AddFriendViewModel>
{
    public AddFriendViewModelValidator(IStringLocalizer<AddFriendViewModelValidator> localizer)
    {
        RuleFor(dto => dto.Email)
            .NotEmpty().WithMessage(localizer["EmailIsRequired"])
            .EmailAddress().WithMessage(localizer["InvalidEmailAddress"]);

        RuleFor(dto => dto.Permissions).NotEmpty().WithMessage(localizer["PermissionsAreRequired"]);
    }
}
