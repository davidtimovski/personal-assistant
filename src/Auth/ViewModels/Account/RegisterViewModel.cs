using System.Linq;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Auth.ViewModels.Account
{
    public class RegisterViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Language { get; set; }
        public float GoogleReCaptchaScore { get; set; }
    }

    public class RegisterViewModelValidator : AbstractValidator<RegisterViewModel>
    {
        public RegisterViewModelValidator(IStringLocalizer<RegisterViewModelValidator> localizer)
        {
            var languages = new string[] { "en-US", "mk-MK" };

            RuleFor(dto => dto.Name).NotEmpty().WithMessage(localizer["NameIsRequired"]).MaximumLength(30).WithMessage(localizer["NameMaxLength", 30]);
            RuleFor(dto => dto.Email).NotEmpty().WithMessage(localizer["EmailIsRequired"]).EmailAddress().WithMessage(localizer["InvalidEmailAddress"]);
            RuleFor(dto => dto.Password).NotEmpty().WithMessage(localizer["PasswordIsRequired"]).MinimumLength(8).WithMessage(localizer["PasswordMinLength", 8]);
            RuleFor(dto => dto.ConfirmPassword).Must((vm, confirmPassword) => vm.Password == confirmPassword).WithMessage(localizer["PasswordsMustMatch"]);
            RuleFor(dto => dto.Language).NotEmpty().WithMessage(localizer["LanguageIsRequired"]).Must(language => languages.Contains(language)).WithMessage(localizer["InvalidLanguage"]);
            RuleFor(dto => dto.GoogleReCaptchaScore).Must(score => score > 0.5).WithMessage(localizer["YouDontSeemToBeHuman"]);
        }
    }
}
