using FluentValidation;
using ToDoAssistant.Application.Contracts.Lists;

namespace ToDoAssistant.Application.Contracts.Tasks.Models;

public class UpdateTask
{
    public required int Id { get; init; }
    public required int UserId { get; init; }
    public required int ListId { get; init; }
    public required string Name { get; init; }
    public required string? Url { get; init; }
    public required bool IsOneTime { get; init; }
    public required bool IsHighPriority { get; init; }
    public required bool? IsPrivate { get; init; }
    public required int? AssignedToUserId { get; init; }
}

public class UpdateTaskValidator : AbstractValidator<UpdateTask>
{
    public UpdateTaskValidator(ITaskService taskService, IListService listService)
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must((dto, userId) => !taskService.Exists(dto.Id, dto.Name, dto.ListId, userId)).WithMessage("AlreadyExists");

        RuleFor(dto => dto.ListId)
            .Must((dto, listId) =>
            {
                SimpleTask originalTask = taskService.Get(dto.Id);
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
                if (assignedToUserId.HasValue && !listService.UserOwnsOrSharesAsPending(dto.ListId, assignedToUserId.Value))
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
