using System.Linq;
using FluentValidation;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models
{
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
        private static readonly string[] ListIcons = new string[] { "list", "shopping", "home", "birthday", "cheers", "vacation", "plane", "car", "pickup-truck", "world", "camping", "motorcycle", "bicycle", "ski", "snowboard", "work", "baby", "dog", "cat", "fish", "camera", "medicine", "file", "book", "mountain" };

        public UpdateListValidator(IListService listService)
        {
            RuleFor(dto => dto.UserId)
                .NotEmpty().WithMessage("Unauthorized")
                .MustAsync(async (dto, userId, val) => await listService.UserOwnsOrSharesAsAdminAsync(dto.Id, userId)).WithMessage("Unauthorized")
                .MustAsync(async (dto, userId, val) => !await listService.UserOwnsOrSharesAsAdminAsync(dto.Id, dto.Name, userId)).WithMessage("AlreadyExists")
                .MustAsync(async (dto, userId, val) => !await listService.ExistsAsync(dto.Id, dto.Name, userId)).WithMessage("AlreadyExists");

            RuleFor(dto => dto.Name)
                .NotEmpty().WithMessage("Lists.ModifyList.NameIsRequired")
                .MaximumLength(50).WithMessage("Lists.ModifyList.NameMaxLength");

            RuleFor(dto => dto.Icon)
                .NotEmpty().WithMessage("Lists.ModifyList.IconIsRequired")
                .Must(icon => ListIcons.Contains(icon)).WithMessage("Lists.ModifyList.InvalidIcon");
        }
    }
}
