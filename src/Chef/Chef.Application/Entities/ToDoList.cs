using Core.Application;

namespace Chef.Application.Entities;

public class ToDoList : Entity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}
