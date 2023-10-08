using FluentValidation;
using ToDoAssistant.Application.Services;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class UpdateList
{
    public required int Id { get; init; }
    public required int UserId { get; init; }
    public required string Name { get; init; }
    public required string Icon { get; init; }
    public required bool IsOneTimeToggleDefault { get; init; }
    public required bool NotificationsEnabled { get; init; }
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
            .Must(icon => ListService.IconOptions.Contains(icon)).WithMessage("Lists.ModifyList.InvalidIcon");
    }
}
