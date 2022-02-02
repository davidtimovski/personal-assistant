using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Auth.ViewModels.Account;

public class ResetPasswordViewModel
{
    public int UserId { get; set; }
    public string Token { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }
}

public class ResetPasswordViewModelValidator : AbstractValidator<ResetPasswordViewModel>
{
    public ResetPasswordViewModelValidator(IStringLocalizer<ResetPasswordViewModelValidator> localizer)
    {
        RuleFor(dto => dto.UserId).NotEmpty();
        RuleFor(dto => dto.Token).NotEmpty();
        RuleFor(dto => dto.NewPassword).NotEmpty().WithMessage(localizer["NewPasswordIsRequired"]).MinimumLength(8).WithMessage(localizer["PasswordMinLength", 8]);
        RuleFor(dto => dto.ConfirmPassword).Must((vm, confirmPassword) => vm.NewPassword == confirmPassword).WithMessage(localizer["PasswordsMustMatch"]);
    }
}