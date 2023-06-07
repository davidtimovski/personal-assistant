using FluentValidation;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class UpdateSharedList
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public bool NotificationsEnabled { get; set; }
}

public class UpdateSharedListValidator : AbstractValidator<UpdateSharedList>
{
    public UpdateSharedListValidator(IListService listService)
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must((dto, userId) => listService.UserOwnsOrSharesAsAdmin(dto.Id, userId)).WithMessage("Unauthorized");
    }
}
