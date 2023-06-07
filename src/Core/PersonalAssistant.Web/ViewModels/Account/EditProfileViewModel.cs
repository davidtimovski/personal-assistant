using System.Globalization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace PersonalAssistant.Web.ViewModels.Account;

public class EditProfileViewModel
{
    public string Name { get; set; } = null!;
    public string Language { get; set; } = null!;
    public string Culture { get; set; } = null!;
    public string ImageUri { get; set; } = null!;
}

public class EditProfileViewModelValidator : AbstractValidator<EditProfileViewModel>
{
    private static readonly HashSet<string> Languages = new HashSet<string> { "en-US", "mk-MK" };

    public EditProfileViewModelValidator(IStringLocalizer<EditProfileViewModelValidator> localizer)
    {
        var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(x => !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToHashSet();

        RuleFor(dto => dto.Name).NotEmpty().WithMessage(localizer["NameIsRequired"]).MaximumLength(30).WithMessage(localizer["NameMaxLength", 30]);
        RuleFor(dto => dto.Language).NotEmpty().WithMessage(localizer["LanguageIsRequired"]).Must(language => Languages.Contains(language)).WithMessage(localizer["InvalidLanguage"]);
        RuleFor(dto => dto.Culture).NotEmpty().WithMessage(localizer["CultureIsRequired"]).Must(culture => cultures.Contains(culture)).WithMessage(localizer["InvalidCulture"]);
    }
}
