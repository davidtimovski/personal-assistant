using FluentValidation;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Entities;

namespace ToDoAssistant.Application.Contracts.Tasks.Models;

public class BulkCreate
{
    public required int UserId { get; init; }
    public required int ListId { get; init; }
    public required string TasksText { get; init; }
    public required bool TasksAreOneTime { get; init; }
    public required bool TasksArePrivate { get; init; }
}

public class BulkCreateValidator : AbstractValidator<BulkCreate>
{
    public BulkCreateValidator(ITaskService taskService, IListService listService)
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty().WithMessage("Unauthorized")
            .Must((dto, userId) => {
                var ownsOrSharesResult = listService.UserOwnsOrShares(dto.ListId, userId);
                if (ownsOrSharesResult.Failed)
                {
                    throw new Exception("Failed to perform validation");
                }

                return ownsOrSharesResult.Data;
            }).WithMessage("Unauthorized");

        RuleFor(dto => dto.ListId)
            .Must((dto, listId) =>
            {
                var taskNames = dto.TasksText.Split("\n").Where(x => !string.IsNullOrWhiteSpace(x));
                var countResult = taskService.Count(listId);
                if (countResult.Failed)
                {
                    throw new Exception("Failed to perform validation");
                }

                return countResult.Data + taskNames.Count() <= ToDoList.MaxTasks;
            }).WithMessage("TasksPerListLimitReached");

        RuleFor(dto => dto.TasksText).NotEmpty().WithMessage("Tasks.BulkCreate.TextIsRequired").Must(tasksText =>
        {
            var tasks = tasksText.Split("\n").Where(x => !string.IsNullOrWhiteSpace(x));
            return tasks.Any();
        }).WithMessage("Tasks.BulkCreate.NoTasks").Must((dto, tasksText) =>
        {
            var taskNames = tasksText.Split("\n").Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var existsResult = taskService.Exists(taskNames, dto.ListId, dto.UserId);
            if (existsResult.Failed)
            {
                throw new Exception("Failed to perform validation");
            }

            return !existsResult.Data;
        }).WithMessage("Tasks.BulkCreate.SomeTasksAlreadyExist").Must(tasksText =>
        {
            var tasks = tasksText.Split("\n").Where(x => !string.IsNullOrWhiteSpace(x));
            var duplicates = tasks.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            return !duplicates.Any();
        }).WithMessage("Tasks.BulkCreate.DuplicateTasks").Must(tasksText =>
        {
            var tasks = tasksText.Split("\n").Where(x => !string.IsNullOrWhiteSpace(x) && x.Length > 50);
            return !tasks.Any();
        }).WithMessage("Tasks.BulkCreate.TasksNameMaxLength");
    }
}
