using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Auth.ViewModels.Account
{
    public class ChangePasswordViewModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModelValidator : AbstractValidator<ChangePasswordViewModel>
    {
        public ChangePasswordViewModelValidator(IStringLocalizer<ChangePasswordViewModelValidator> localizer)
        {
            RuleFor(dto => dto.CurrentPassword).NotEmpty().WithMessage(localizer["CurrentPasswordIsRequired"]);
            RuleFor(dto => dto.NewPassword).NotEmpty().WithMessage(localizer["NewPasswordIsRequired"]).MinimumLength(8).WithMessage(localizer["PasswordMinLength", 8]);
            RuleFor(dto => dto.ConfirmPassword).Must((vm, confirmPassword) => vm.NewPassword == confirmPassword).WithMessage(localizer["PasswordsMustMatch"]);
        }
    }
}
