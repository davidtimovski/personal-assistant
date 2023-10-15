using FluentValidation;
using ToDoAssistant.Application.Services;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class CreateList
{
    public required int UserId { get; init; }
    public required string Name { get; init; }
    public required string Icon { get; init; }
    public required bool IsOneTimeToggleDefault { get; init; }
    public required string? TasksText { get; init; }
}

public class CreateListValidator : AbstractValidator<CreateList>
{
    public CreateListValidator(IListService listService)
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must((dto, userId) =>
            {
                var existsResult = listService.Exists(dto.Name, userId);
                if (existsResult.Failed)
                {
                    throw new Exception("Failed to perform validation");
                }

                return !existsResult.Data;
            }).WithMessage("AlreadyExists")
            .Must(userId =>
            {
                var countResult = listService.Count(userId);
                if (countResult.Failed)
                {
                    throw new Exception("Failed to perform validation");
                }

                return countResult.Data < 50;
            }).WithMessage("Lists.ListLimitReached");

        RuleFor(dto => dto.Name)
            .NotEmpty().WithMessage("Lists.ModifyList.NameIsRequired")
            .MaximumLength(50).WithMessage("Lists.ModifyList.NameMaxLength");

        RuleFor(dto => dto.Icon)
            .NotEmpty().WithMessage("Lists.ModifyList.IconIsRequired")
            .Must(icon => ListService.IconOptions.Contains(icon)).WithMessage("Lists.ModifyList.InvalidIcon");

        RuleFor(dto => dto.TasksText).Must(tasksText =>
        {
            if (string.IsNullOrEmpty(tasksText))
            {
                return true;
            }
            var tasks = tasksText.Split("\n").Where(x => !string.IsNullOrWhiteSpace(x));
            return tasks.Any();
        }).WithMessage("Lists.CreateList.NoTasks").Must(tasksText =>
        {
            if (string.IsNullOrEmpty(tasksText))
            {
                return true;
            }
            var tasks = tasksText.Split("\n").Where(x => !string.IsNullOrWhiteSpace(x));
            var duplicates = tasks.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            return !duplicates.Any();
        }).WithMessage("Lists.CreateList.DuplicateTasks").Must(tasksText =>
        {
            if (string.IsNullOrEmpty(tasksText))
            {
                return true;
            }
            var tasks = tasksText.Split("\n").Where(x => !string.IsNullOrWhiteSpace(x));
            return tasks.Count() < 250;
        }).WithMessage("TasksPerListLimitReached").Must(tasksText =>
        {
            if (string.IsNullOrEmpty(tasksText))
            {
                return true;
            }
            var tasks = tasksText.Split("\n").Where(x => !string.IsNullOrWhiteSpace(x) && x.Length > 50);
            return !tasks.Any();
        }).WithMessage("Lists.CreateList.TasksNameMaxLength");
    }
}
