using FluentValidation;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models
{
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
                .MustAsync(async (dto, userId, val) => await listService.UserOwnsOrSharesAsAdminAsync(dto.Id, userId)).WithMessage("Unauthorized");
        }
    }
}
