using System.Linq;
using FluentValidation;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models
{
    public class CreateList
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public bool IsOneTimeToggleDefault { get; set; }
        public string TasksText { get; set; }
    }

    public class CreateListValidator : AbstractValidator<CreateList>
    {
        private static readonly string[] ListIcons = new string[] { "list", "shopping", "home", "birthday", "cheers", "vacation", "plane", "car", "pickup-truck", "world", "camping", "motorcycle", "bicycle", "ski", "snowboard", "work", "baby", "dog", "cat", "fish", "camera", "medicine", "file", "book", "mountain" };

        public CreateListValidator(IListService listService)
        {
            RuleFor(dto => dto.UserId)
                .NotEmpty().WithMessage("Unauthorized")
                .Must((dto, userId) => !listService.Exists(dto.Name, userId)).WithMessage("AlreadyExists")
                .Must((dto, userId) => listService.Count(userId) < 50).WithMessage("Lists.ListLimitReached");

            RuleFor(dto => dto.Name)
                .NotEmpty().WithMessage("Lists.ModifyList.NameIsRequired")
                .MaximumLength(50).WithMessage("Lists.ModifyList.NameMaxLength");

            RuleFor(dto => dto.Icon)
                .NotEmpty().WithMessage("Lists.ModifyList.IconIsRequired")
                .Must(icon => ListIcons.Contains(icon)).WithMessage("Lists.ModifyList.InvalidIcon");

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
}
