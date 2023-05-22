using ToDoAssistant.Application.Contracts.Lists.Models;

namespace Application.UnitTests.Builders;

internal class ListBuilder
{
    private string name;
    private string tasksText;

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

    internal CreateList BuildCreateModel()
    {
        return new CreateList
        {
            Name = name,
            TasksText = tasksText
        };
    }

    internal UpdateList BuildUpdateModel()
    {
        return new UpdateList
        {
            Name = name
        };
    }

    internal UpdateSharedList BuildUpdateSharedModel()
    {
        return new UpdateSharedList();
    }

    internal CopyList BuildCopyModel()
    {
        return new CopyList
        {
            Name = name
        };
    }
}
