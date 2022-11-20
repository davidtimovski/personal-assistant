using FluentValidation;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class UpdateList
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public bool IsOneTimeToggleDefault { get; set; }
    public bool NotificationsEnabled { get; set; }
}

public class UpdateListValidator : AbstractValidator<UpdateList>
{
    public UpdateListValidator(IListService listService)
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must((dto, userId) => listService.UserOwnsOrSharesAsAdmin(dto.Id, userId)).WithMessage("Unauthorized")
            .Must((dto, userId) => !listService.UserOwnsOrSharesAsAdmin(dto.Id, dto.Name, userId)).WithMessage("AlreadyExists")
            .Must((dto, userId) => !listService.Exists(dto.Id, dto.Name, userId)).WithMessage("AlreadyExists");

        RuleFor(dto => dto.Name)
            .NotEmpty().WithMessage("Lists.ModifyList.NameIsRequired")
            .MaximumLength(50).WithMessage("Lists.ModifyList.NameMaxLength");

        RuleFor(dto => dto.Icon)
            .NotEmpty().WithMessage("Lists.ModifyList.IconIsRequired")
            .Must(icon => listService.IconOptions.Contains(icon)).WithMessage("Lists.ModifyList.InvalidIcon");
    }
}
