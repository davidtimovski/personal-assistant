using FluentValidation;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models
{
    public class UpdateTask
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ListId { get; set; }
        public string Name { get; set; }
        public bool IsOneTime { get; set; }
        public bool? IsPrivate { get; set; }
        public int? AssignedToUserId { get; set; }
    }

    public class UpdateTaskValidator : AbstractValidator<UpdateTask>
    {
        public UpdateTaskValidator(ITaskService taskService, IListService listService)
        {
            RuleFor(dto => dto.UserId)
                .NotEmpty().WithMessage("Unauthorized")
                .Must((dto, userId) => !taskService.Exists(dto.Id, dto.Name, dto.ListId, userId)).WithMessage("AlreadyExists");

            RuleFor(dto => dto.ListId)
                .MustAsync(async (dto, listId, val) =>
                {
                    SimpleTask originalTask = await taskService.GetAsync(dto.Id);
                    bool listChanged = listId != originalTask.ListId;

                    if (listChanged && taskService.Count(dto.ListId) == 250)
                    {
                        return false;
                    }

                    return true;
                }).WithMessage("Tasks.UpdateTask.SelectedListAlreadyContainsMaxTasks");

            RuleFor(dto => dto.AssignedToUserId)
                .Must((dto, assignedToUserId) =>
                {
                    if (dto.AssignedToUserId.HasValue && !listService.UserOwnsOrSharesAsPending(dto.ListId, assignedToUserId.Value))
                    {
                        return false;
                    }

                    return true;
                }).WithMessage("Tasks.UpdateTask.TheAssignedUserIsNotAMember");

            RuleFor(dto => dto.Name)
                .NotEmpty().WithMessage("Tasks.ModifyTask.NameIsRequired")
                .MaximumLength(50).WithMessage("Tasks.ModifyTask.NameMaxLength");
        }
    }
}
