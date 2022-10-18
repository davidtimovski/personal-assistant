using System.Linq;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Account.ViewModels.Account;

public class EditProfileViewModel
{
    public string Name { get; set; }
    public string Language { get; set; }
    public string ImageUri { get; set; }
}

public class EditProfileViewModelValidator : AbstractValidator<EditProfileViewModel>
{
    public EditProfileViewModelValidator(IStringLocalizer<EditProfileViewModelValidator> localizer)
    {
        var languages = new string[] { "en-US", "mk-MK" };

        RuleFor(dto => dto.Name).NotEmpty().WithMessage(localizer["NameIsRequired"]).MaximumLength(30).WithMessage(localizer["NameMaxLength", 30]);
        RuleFor(dto => dto.Language).NotEmpty().WithMessage(localizer["LanguageIsRequired"]).Must(language => languages.Contains(language)).WithMessage(localizer["InvalidLanguage"]);
    }
}