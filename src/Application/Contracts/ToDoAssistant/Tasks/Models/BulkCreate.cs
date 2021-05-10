using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models
{
    public class BulkCreate
    {
        public int UserId { get; set; }
        public int ListId { get; set; }
        public string TasksText { get; set; }
        public bool TasksAreOneTime { get; set; }
        public bool TasksArePrivate { get; set; }
    }

    public class BulkCreateValidator : AbstractValidator<BulkCreate>
    {
        public BulkCreateValidator(ITaskService taskService, IListService listService)
        {
            RuleFor(dto => dto.UserId)
                .NotEmpty().WithMessage("Unauthorized")
                .Must((dto, userId) => listService.UserOwnsOrShares(dto.ListId, userId)).WithMessage("Unauthorized");

            RuleFor(dto => dto.ListId)
                .Must((dto, listId) =>
                {
                    var taskNames = dto.TasksText.Split("\n").Where(x => !string.IsNullOrWhiteSpace(x));
                    return taskService.Count(listId) + taskNames.Count() <= 250;
                }).WithMessage("TasksPerListLimitReached");

            RuleFor(dto => dto.TasksText).NotEmpty().WithMessage("Tasks.BulkCreate.TextIsRequired").Must(tasksText =>
            {
                var tasks = tasksText.Split("\n").Where(x => !string.IsNullOrWhiteSpace(x));
                return tasks.Any();
            }).WithMessage("Tasks.BulkCreate.NoTasks").Must((dto, tasksText) =>
            {
                IEnumerable<string> taskNames = tasksText.Split("\n").Where(x => !string.IsNullOrWhiteSpace(x));
                return !taskService.Exists(taskNames, dto.ListId, dto.UserId);
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
}
