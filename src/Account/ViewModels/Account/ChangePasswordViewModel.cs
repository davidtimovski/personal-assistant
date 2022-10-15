using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Account.ViewModels.Account;

public class ChangePasswordViewModel
{
    public string Email { get; set; }
}

public class ChangePasswordViewModelValidator : AbstractValidator<ChangePasswordViewModel>
{
    public ChangePasswordViewModelValidator(IStringLocalizer<ChangePasswordViewModelValidator> localizer)
    {
        RuleFor(dto => dto.Email).NotEmpty().WithMessage(localizer["EmailIsRequired"]).EmailAddress().WithMessage(localizer["InvalidEmailAddress"]);
    }
}
