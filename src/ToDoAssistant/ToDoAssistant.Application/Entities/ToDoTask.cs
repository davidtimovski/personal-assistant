using Core.Application;
using Core.Application.Entities;

namespace ToDoAssistant.Application.Entities;

public class ToDoTask : Entity
{
    public int Id { get; set; }
    public int ListId { get; set; }
    public string Name { get; set; } = null!;
    public string? Url { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsOneTime { get; set; }
    public bool IsHighPriority { get; set; }
    public int? PrivateToUserId { get; set; }
    public int? AssignedToUserId { get; set; }
    public short Order { get; set; }

    public ToDoList? List { get; set; }
    public User? AssignedToUser { get; set; }
}
