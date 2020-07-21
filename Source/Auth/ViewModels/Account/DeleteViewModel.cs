using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Auth.ViewModels.Account
{
    public class DeleteViewModel
    {
        public string Password { get; set; }
    }

    public class DeleteViewModelValidator : AbstractValidator<DeleteViewModel>
    {
        public DeleteViewModelValidator(IStringLocalizer<DeleteViewModelValidator> localizer)
        {
            RuleFor(dto => dto.Password).NotEmpty().WithMessage(localizer["PasswordIsRequired"]);
        }
    }
}
