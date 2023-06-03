using Core.Application;
using Core.Application.Entities;

namespace ToDoAssistant.Application.Entities;

public class Notification : Entity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ActionUserId { get; set; }
    public int? ListId { get; set; }
    public int? TaskId { get; set; }
    public string Message { get; set; } = null!;
    public bool IsSeen { get; set; }

    public User? User { get; set; }
}
