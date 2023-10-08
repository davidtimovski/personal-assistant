using FluentValidation;
using ToDoAssistant.Application.Services;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class CopyList
{
    public required int Id { get; init; }
    public required int UserId { get; init; }
    public required string Name { get; init; }
    public required string Icon { get; init; }
}

public class CopyListValidator : AbstractValidator<CopyList>, IValidator<CopyList>
{
    public CopyListValidator(IListService listService)
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must((dto, userId) => listService.UserOwnsOrShares(dto.Id, userId)).WithMessage("Unauthorized")
            .Must((dto, userId) => !listService.Exists(dto.Name, userId)).WithMessage("AlreadyExists")
            .Must(userId => listService.Count(userId) < 50).WithMessage("Lists.ListLimitReached");

        RuleFor(dto => dto.Name)
            .NotEmpty().WithMessage("Lists.ModifyList.NameIsRequired")
            .MaximumLength(50).WithMessage("Lists.ModifyList.NameMaxLength");

        RuleFor(dto => dto.Icon)
            .NotEmpty().WithMessage("Lists.ModifyList.IconIsRequired")
            .Must(icon => ListService.IconOptions.Contains(icon)).WithMessage("Lists.ModifyList.InvalidIcon");
    }
}
