using System.Globalization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace PersonalAssistant.Web.ViewModels.Account;

public class EditProfileViewModel
{
    public required string Name { get; init; }
    public required string? Country { get; init; }
    public required string Language { get; init; }
    public required string Culture { get; init; }
    public required string ImageUri { get; init; }
}

public class EditProfileViewModelValidator : AbstractValidator<EditProfileViewModel>
{
    private static readonly HashSet<string> Countries = new HashSet<string> { "CZ", "MK" };
    private static readonly HashSet<string> Languages = new HashSet<string> { "en-US", "mk-MK" };

    public EditProfileViewModelValidator(IStringLocalizer<EditProfileViewModelValidator> localizer)
    {
        var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(x => !string.IsNullOrEmpty(x.Name)).Select(x => x.Name).ToHashSet();

        RuleFor(dto => dto.Name).NotEmpty().WithMessage(localizer["NameIsRequired"]).MaximumLength(30).WithMessage(localizer["NameMaxLength", 30]);
        RuleFor(dto => dto.Country).Must(country => country is null || Countries.Contains(country)).WithMessage(localizer["InvalidCountry"]);
        RuleFor(dto => dto.Language).NotEmpty().WithMessage(localizer["LanguageIsRequired"]).Must(language => Languages.Contains(language)).WithMessage(localizer["InvalidLanguage"]);
        RuleFor(dto => dto.Culture).NotEmpty().WithMessage(localizer["CultureIsRequired"]).Must(culture => cultures.Contains(culture)).WithMessage(localizer["InvalidCulture"]);
    }
}
