using FluentValidation;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Entities;

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
            .Must((dto, userId) =>
            {
                var existsResult = taskService.Exists(dto.Id, dto.Name, dto.ListId, userId);
                if (existsResult.Failed)
                {
                    throw new Exception("Failed to perform validation");
                }

                return !existsResult.Data;
            }).WithMessage("AlreadyExists");

        RuleFor(dto => dto.ListId)
            .Must((dto, listId) =>
            {
                var taskResult = taskService.Get(dto.Id);
                if (taskResult.Failed || taskResult.Data is null)
                {
                    throw new Exception("Failed to perform validation");
                }

                bool listChanged = listId != taskResult.Data.ListId;

                var countResult = taskService.Count(dto.ListId);
                if (countResult.Failed)
                {
                    throw new Exception("Failed to perform validation");
                }

                if (listChanged && countResult.Data >= ToDoList.MaxTasks)
                {
                    return false;
                }

                return true;
            }).WithMessage("Tasks.UpdateTask.SelectedListAlreadyContainsMaxTasks");

        RuleFor(dto => dto.AssignedToUserId)
            .Must((dto, assignedToUserId) =>
            {
                if (assignedToUserId is null)
                {
                    return true;
                }

                var ownsOrSharesAsPendingResult = listService.UserOwnsOrSharesAsPending(dto.ListId, assignedToUserId.Value);
                if (ownsOrSharesAsPendingResult.Failed)
                {
                    throw new Exception("Failed to perform validation");
                }

                return !ownsOrSharesAsPendingResult.Data;
            }).WithMessage("Tasks.UpdateTask.TheAssignedUserIsNotAMember");

        RuleFor(dto => dto.Name)
            .NotEmpty().WithMessage("Tasks.ModifyTask.NameIsRequired")
            .MaximumLength(50).WithMessage("Tasks.ModifyTask.NameMaxLength");
    }
}
