using Core.Application;

namespace CookingAssistant.Application.Entities;

public class ToDoTask : Entity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsCompleted { get; set; }

    public ToDoList List { get; set; }
}
