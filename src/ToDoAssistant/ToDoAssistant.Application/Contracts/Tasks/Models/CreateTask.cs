using FluentValidation;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Entities;

namespace ToDoAssistant.Application.Contracts.Tasks.Models;

public class CreateTask
{
    public required int UserId { get; init; }
    public required int ListId { get; init; }
    public required string Name { get; init; }
    public required string? Url { get; init; }
    public required bool IsOneTime { get; init; }
    public required bool? IsPrivate { get; init; }
}

public class CreateTaskValidator : AbstractValidator<CreateTask>
{
    public CreateTaskValidator(ITaskService taskService, IListService listService)
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must((dto, userId) =>
            {
                var ownsOrSharesResult = listService.UserOwnsOrShares(dto.ListId, userId);
                if (ownsOrSharesResult.Failed)
                {
                    throw new Exception("Failed to perform validation");
                }

                return ownsOrSharesResult.Data;
            }).WithMessage("Unauthorized")
            .Must((dto, userId) =>
            {
                var existsResult = taskService.Exists(dto.Name, dto.ListId, userId);
                if (existsResult.Failed)
                {
                    throw new Exception("Failed to perform validation");
                }

                return !existsResult.Data;
            }).WithMessage("AlreadyExists")
            .Must((dto, userId) =>
            {
                var countResult = taskService.Count(dto.ListId);
                if (countResult.Failed)
                {
                    throw new Exception("Failed to perform validation");
                }

                return countResult.Data < ToDoList.MaxTasks;
            }).WithMessage("TasksPerListLimitReached");

        RuleFor(dto => dto.Name)
            .NotEmpty().WithMessage("Tasks.ModifyTask.NameIsRequired")
            .MaximumLength(50).WithMessage("Tasks.ModifyTask.NameMaxLength");
    }
}
