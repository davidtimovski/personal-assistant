using System.Globalization;
using Core.Application.Contracts;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace PersonalAssistant.Web.ViewModels.Account;

public class RegisterViewModel
{
    public RegisterViewModel()
    {
        CultureOptions = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Where(x => !string.IsNullOrEmpty(x.Name))
            .OrderBy(x => x.EnglishName)
            .Select(x => new CultureOption(x.Name, x.EnglishName)).ToList();
    }

    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
    public string Language { get; set; } = "";
    public string Culture { get; set; } = "";
    public float GoogleReCaptchaScore { get; set; }
    public List<CultureOption> CultureOptions { get; }
}

public class RegisterViewModelValidator : AbstractValidator<RegisterViewModel>
{
    private static readonly HashSet<string> Languages = new HashSet<string> { "en-US", "mk-MK" };

    public RegisterViewModelValidator(IStringLocalizer<RegisterViewModelValidator> localizer, IUsersRepository usersRepository)
    {
        var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(x => !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToHashSet();

        RuleFor(dto => dto.Name).NotEmpty().WithMessage(localizer["NameIsRequired"])
            .MaximumLength(30).WithMessage(localizer["NameMaxLength", 30]);

        RuleFor(dto => dto.Email).NotEmpty().WithMessage(localizer["EmailIsRequired"])
            .EmailAddress().WithMessage(localizer["InvalidEmailAddress"])
            .Must(email => !usersRepository.Exists(email)).WithMessage(localizer["UserAlreadyExists"]);

        RuleFor(dto => dto.Password).NotEmpty().WithMessage(localizer["PasswordIsRequired"]);

        RuleFor(dto => dto.ConfirmPassword).Must((vm, confirmPassword) => vm.Password == confirmPassword).WithMessage(localizer["PasswordsMustMatch"]);

        RuleFor(dto => dto.Language).NotEmpty().WithMessage(localizer["LanguageIsRequired"])
            .Must(language => Languages.Contains(language)).WithMessage(localizer["InvalidLanguage"]);

        RuleFor(dto => dto.Culture).NotEmpty().WithMessage(localizer["CultureIsRequired"])
            .Must(culture => cultures.Contains(culture)).WithMessage(localizer["InvalidCulture"]);

        RuleFor(dto => dto.GoogleReCaptchaScore).Must(score => score > 0.5).WithMessage(localizer["YouDontSeemToBeHuman"]);
    }
}
