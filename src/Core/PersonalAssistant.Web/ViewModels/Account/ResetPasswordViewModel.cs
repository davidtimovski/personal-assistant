using FluentValidation;
using Microsoft.Extensions.Localization;

namespace PersonalAssistant.Web.ViewModels.Account;

public class ResetPasswordViewModel
{
    public string Email { get; set; } = null!;
    public float GoogleReCaptchaScore { get; set; }
}

public class ResetPasswordViewModelValidator : AbstractValidator<ResetPasswordViewModel>
{
    public ResetPasswordViewModelValidator(IStringLocalizer<ResetPasswordViewModelValidator> localizer)
    {
        RuleFor(dto => dto.Email)
            .NotEmpty().WithMessage(localizer["EmailIsRequired"])
            .EmailAddress().WithMessage(localizer["InvalidEmailAddress"]);

        RuleFor(dto => dto.GoogleReCaptchaScore).Must(score => score > 0.5).WithMessage(localizer["YouDontSeemToBeHuman"]);
    }
}
