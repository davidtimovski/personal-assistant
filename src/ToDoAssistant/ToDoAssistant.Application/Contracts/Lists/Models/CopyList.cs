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
            .Must((dto, userId) =>
            {
                var ownsOrSharesResult = listService.UserOwnsOrShares(dto.Id, userId);
                if (ownsOrSharesResult.Failed)
                {
                    throw new Exception("Failed to perform validation");
                }

                return ownsOrSharesResult.Data;
            }).WithMessage("Unauthorized")
            .Must((dto, userId) =>
            {
                var existsResult = listService.Exists(dto.Name, userId);
                if (existsResult.Failed)
                {
                    throw new Exception("Failed to perform validation");
                }

                return !existsResult.Data;
            }).WithMessage("AlreadyExists")
            .Must(userId =>
            {
                var countResult = listService.Count(userId);
                if (countResult.Failed)
                {
                    throw new Exception("Failed to perform validation");
                }

                return countResult.Data < 50;
            }).WithMessage("Lists.ListLimitReached");

        RuleFor(dto => dto.Name)
            .NotEmpty().WithMessage("Lists.ModifyList.NameIsRequired")
            .MaximumLength(50).WithMessage("Lists.ModifyList.NameMaxLength");

        RuleFor(dto => dto.Icon)
            .NotEmpty().WithMessage("Lists.ModifyList.IconIsRequired")
            .Must(icon => ListService.IconOptions.Contains(icon)).WithMessage("Lists.ModifyList.InvalidIcon");
    }
}
