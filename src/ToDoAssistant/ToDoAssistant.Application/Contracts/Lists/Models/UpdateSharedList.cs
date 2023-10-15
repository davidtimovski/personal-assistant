using FluentValidation;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class UpdateSharedList
{
    public required int Id { get; init; }
    public required int UserId { get; init; }
    public required bool NotificationsEnabled { get; init; }
}

public class UpdateSharedListValidator : AbstractValidator<UpdateSharedList>
{
    public UpdateSharedListValidator(IListService listService)
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must((dto, userId) =>
            {
                var ownsOrSharesResult = listService.UserOwnsOrSharesAsAdmin(dto.Id, userId);
                if (ownsOrSharesResult.Failed)
                {
                    throw new Exception("Failed to perform validation");
                }

                return ownsOrSharesResult.Data;
            }).WithMessage("Unauthorized");
    }
}
