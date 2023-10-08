using ToDoAssistant.Application.Contracts.Lists.Models;

namespace Application.UnitTests.Builders;

internal class ListBuilder
{
    private string name;
    private string? tasksText;

    internal ListBuilder()
    {
        name = "Dummy name";
    }

    internal ListBuilder WithName(string newName)
    {
        name = newName;
        return this;
    }

    internal ListBuilder WithTasksText(string newTasksText)
    {
        tasksText = newTasksText;
        return this;
    }

    internal CreateList BuildCreateModel() => new CreateList
    {
        Name = name,
        TasksText = tasksText,
        UserId = 0,
        Icon = "",
        IsOneTimeToggleDefault = false
    };

    internal UpdateList BuildUpdateModel() => new UpdateList
    {
        Name = name,
        Id = 0,
        UserId = 0,
        Icon = "",
        IsOneTimeToggleDefault = false,
        NotificationsEnabled = false
    };

    internal UpdateSharedList BuildUpdateSharedModel() => new UpdateSharedList
    {
        Id = 0,
        UserId = 0,
        NotificationsEnabled = false
    };

    internal CopyList BuildCopyModel() => new CopyList
    {
        Name = name,
        Id = 0,
        UserId = 0,
        Icon = ""
    };
}
