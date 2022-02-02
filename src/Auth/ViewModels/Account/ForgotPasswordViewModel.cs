using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Auth.ViewModels.Account;

public class ForgotPasswordViewModel
{
    public string Email { get; set; }
    public float GoogleReCaptchaScore { get; set; }
}

public class ForgotPasswordViewModelValidator : AbstractValidator<ForgotPasswordViewModel>
{
    public ForgotPasswordViewModelValidator(IStringLocalizer<ForgotPasswordViewModelValidator> localizer)
    {
        RuleFor(dto => dto.Email).NotEmpty().WithMessage(localizer["EmailIsRequired"]).EmailAddress().WithMessage(localizer["InvalidEmailAddress"]);
        RuleFor(dto => dto.GoogleReCaptchaScore).Must(score => score > 0.5).WithMessage(localizer["YouDontSeemToBeHuman"]);
    }
}