﻿using System.Globalization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Account.Web.ViewModels.Account;

public class EditProfileViewModel
{
    public string Name { get; set; }
    public string Language { get; set; }
    public string Culture { get; set; }
    public string ImageUri { get; set; }
}

public class EditProfileViewModelValidator : AbstractValidator<EditProfileViewModel>
{
    public EditProfileViewModelValidator(IStringLocalizer<EditProfileViewModelValidator> localizer)
    {
        var languages = new string[] { "en-US", "mk-MK" };
        var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(x => !string.IsNullOrEmpty(x.Name)).Select(x => x.Name);

        RuleFor(dto => dto.Name).NotEmpty().WithMessage(localizer["NameIsRequired"]).MaximumLength(30).WithMessage(localizer["NameMaxLength", 30]);
        RuleFor(dto => dto.Language).NotEmpty().WithMessage(localizer["LanguageIsRequired"]).Must(language => languages.Contains(language)).WithMessage(localizer["InvalidLanguage"]);
        RuleFor(dto => dto.Culture).NotEmpty().WithMessage(localizer["CultureIsRequired"]).Must(culture => cultures.Contains(culture)).WithMessage(localizer["InvalidCulture"]);
    }
}