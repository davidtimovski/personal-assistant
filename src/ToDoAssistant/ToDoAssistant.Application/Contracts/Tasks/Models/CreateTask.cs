using FluentValidation;
using ToDoAssistant.Application.Contracts.Lists;

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
    private const string prefix = nameof(CreateTask);

    public CreateTaskValidator(ITaskService taskService, IListService listService)
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must((dto, userId) => listService.UserOwnsOrShares(dto.ListId, userId)).WithMessage("Unauthorized")
            .Must((dto, userId) => !taskService.Exists(dto.Name, dto.ListId, userId)).WithMessage("AlreadyExists")
            .Must((dto, userId) => taskService.Count(dto.ListId) < 250).WithMessage("TasksPerListLimitReached");

        RuleFor(dto => dto.Name)
            .NotEmpty().WithMessage("Tasks.ModifyTask.NameIsRequired")
            .MaximumLength(50).WithMessage("Tasks.ModifyTask.NameMaxLength");
    }
}
