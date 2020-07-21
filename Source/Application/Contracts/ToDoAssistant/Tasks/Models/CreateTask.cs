using FluentValidation;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models
{
    public class CreateTask
    {
        public int UserId { get; set; }
        public int ListId { get; set; }
        public string Name { get; set; }
        public bool IsOneTime { get; set; }
        public bool? IsPrivate { get; set; }
    }

    public class CreateTaskValidator : AbstractValidator<CreateTask>
    {
        private const string prefix = nameof(CreateTask);

        public CreateTaskValidator(ITaskService taskService, IListService listService)
        {
            RuleFor(dto => dto.UserId)
                .NotEmpty().WithMessage("Unauthorized")
                .MustAsync(async (dto, userId, val) => await listService.UserOwnsOrSharesAsync(dto.ListId, userId)).WithMessage("Unauthorized")
                .MustAsync(async (dto, userId, val) => !await taskService.ExistsAsync(dto.Name, dto.ListId, userId)).WithMessage("AlreadyExists")
                .MustAsync(async (dto, userId, val) => await taskService.CountAsync(dto.ListId) < 250).WithMessage("TasksPerListLimitReached");

            RuleFor(dto => dto.Name)
                .NotEmpty().WithMessage("Tasks.ModifyTask.NameIsRequired")
                .MaximumLength(50).WithMessage("Tasks.ModifyTask.NameMaxLength");
        }
    }
}
