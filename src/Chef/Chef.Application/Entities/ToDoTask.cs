using Core.Application;

namespace Chef.Application.Entities;

public class ToDoTask : Entity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsCompleted { get; set; }

    public ToDoList List { get; set; }
}
