using Application.Contracts.ToDoAssistant.Tasks.Models;

namespace Application.UnitTests.Builders;

public class TaskBuilder
{
    private string name;
    private int userId;
    private int listId;
    private string tasksText;
    private bool tasksAreOneTime;
    private bool tasksArePrivate;

    public TaskBuilder()
    {
        name = "Dummy name";
        tasksText = "Dummy tasks text";
    }

    public TaskBuilder WithName(string newName)
    {
        name = newName;
        return this;
    }

    public TaskBuilder WithUserId(int newUserId)
    {
        userId = newUserId;
        return this;
    }

    public TaskBuilder WithListId(int newListId)
    {
        listId = newListId;
        return this;
    }

    public TaskBuilder WithTasksText(string newTasksText)
    {
        tasksText = newTasksText;
        return this;
    }

    public TaskBuilder WithTasksAreOneTime(bool newTasksAreOneTime)
    {
        tasksAreOneTime = newTasksAreOneTime;
        return this;
    }

    public TaskBuilder WithTasksArePrivate(bool newTasksArePrivate)
    {
        tasksArePrivate = newTasksArePrivate;
        return this;
    }

    public CreateTask BuildCreateModel()
    {
        return new CreateTask
        {
            Name = name
        };
    }

    public BulkCreate BuildBulkCreateModel()
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

    public UpdateTask BuildUpdateModel()
    {
        return new UpdateTask
        {
            Name = name
        };
    }
}
