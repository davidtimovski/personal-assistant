using Core.Application.Contracts;
using FluentValidation;
using Microsoft.Extensions.Localization;
using ToDoAssistant.Application.Services;

namespace PersonalAssistant.Web.ViewModels.Friendships;

public class CreateFriendshipViewModel
{
    public required string Email { get; init; }
    public required IReadOnlyList<string> Permissions { get; init; }
}

public class CreateFriendshipViewModelValidator : AbstractValidator<CreateFriendshipViewModel>
{
    public CreateFriendshipViewModelValidator(IUsersRepository usersRepository, IStringLocalizer<CreateFriendshipViewModelValidator> localizer)
    {
        RuleFor(dto => dto.Email)
            .NotEmpty().WithMessage(localizer["EmailIsRequired"])
            .EmailAddress().WithMessage(localizer["InvalidEmailAddress"])
            .Must((dto, email) => usersRepository.Exists(email.Trim())).WithMessage(localizer["DoesNotExist"]);

        RuleFor(dto => dto.Permissions).NotEmpty().WithMessage(localizer["PermissionsAreRequired"]);
    }
}
