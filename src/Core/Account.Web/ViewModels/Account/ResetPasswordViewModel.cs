using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Account.Web.ViewModels.Account;

public class ResetPasswordViewModel
{
    public string Email { get; set; }
    public float GoogleReCaptchaScore { get; set; }
}

public class ChangePasswordViewModelValidator : AbstractValidator<ResetPasswordViewModel>
{
    public ChangePasswordViewModelValidator(IStringLocalizer<ChangePasswordViewModelValidator> localizer)
    {
        RuleFor(dto => dto.Email).NotEmpty().WithMessage(localizer["EmailIsRequired"]).EmailAddress().WithMessage(localizer["InvalidEmailAddress"]);
        RuleFor(dto => dto.GoogleReCaptchaScore).Must(score => score > 0.5).WithMessage(localizer["YouDontSeemToBeHuman"]);
    }
}
