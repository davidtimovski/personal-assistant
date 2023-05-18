using ToDoAssistant.Application.Contracts.Tasks.Models;

namespace Application.UnitTests.Builders;

internal class TaskBuilder
{
    private string name;
    private int userId;
    private int listId;
    private string tasksText;
    private bool tasksAreOneTime;
    private bool tasksArePrivate;

    internal TaskBuilder()
    {
        name = "Dummy name";
        tasksText = "Dummy tasks text";
    }

    internal TaskBuilder WithName(string newName)
    {
        name = newName;
        return this;
    }

    internal TaskBuilder WithUserId(int newUserId)
    {
        userId = newUserId;
        return this;
    }

    internal TaskBuilder WithListId(int newListId)
    {
        listId = newListId;
        return this;
    }

    internal TaskBuilder WithTasksText(string newTasksText)
    {
        tasksText = newTasksText;
        return this;
    }

    internal TaskBuilder WithTasksAreOneTime(bool newTasksAreOneTime)
    {
        tasksAreOneTime = newTasksAreOneTime;
        return this;
    }

    internal TaskBuilder WithTasksArePrivate(bool newTasksArePrivate)
    {
        tasksArePrivate = newTasksArePrivate;
        return this;
    }

    internal CreateTask BuildCreateModel()
    {
        return new CreateTask
        {
            Name = name
        };
    }

    internal BulkCreate BuildBulkCreateModel()
    {
        return new BulkCreate
        {
            UserId = userId,
            ListId = listId,
            TasksText = tasksText,
            TasksAreOneTime = tasksAreOneTime,
            TasksArePrivate = tasksArePrivate
        };
    }

    internal UpdateTask BuildUpdateModel()
    {
        return new UpdateTask
        {
            Name = name
        };
    }
}
