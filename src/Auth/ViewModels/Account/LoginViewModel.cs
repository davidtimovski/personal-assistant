using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Auth.ViewModels.Account;

public enum LoginAlert
{
    None,
    RegistrationEmailSent,
    RegistrationEmailSentGmail,
    RegistrationEmailSentOutlook,
    RegistrationEmailSentYahoo,
    EmailConfirmationRequired,
    RegistrationConfirmed,
    PasswordResetEmailSent,
    PasswordResetEmailSentGmail,
    PasswordResetEmailSentOutlook,
    PasswordResetEmailSentYahoo,
    PasswordReset,
    AccountDeleted,
    LanguageChanged
}

public enum LoginButtonLabel
{
    Default,
    ToDoAssistant,
    CookingAssistant,
    Accountant
}

public class LoginViewModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string ReturnUrl { get; set; }
    public LoginAlert Alert { get; set; }
    public LoginButtonLabel ButtonLabel { get; set; }
}

public class LoginViewModelValidator : AbstractValidator<LoginViewModel>
{
    public LoginViewModelValidator(IStringLocalizer<LoginViewModelValidator> localizer)
    {
        RuleFor(dto => dto).Must(dto => !string.IsNullOrEmpty(dto.Email) && !string.IsNullOrEmpty(dto.Password)).WithMessage(localizer["InvalidLoginAttempt"]);
    }
}