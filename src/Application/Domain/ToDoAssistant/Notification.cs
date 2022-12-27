using Application.Domain.Common;

namespace Application.Domain.ToDoAssistant;

public class Notification : Entity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ActionUserId { get; set; }
    public int? ListId { get; set; }
    public int? TaskId { get; set; }
    public string Message { get; set; }
    public bool IsSeen { get; set; }

    public User User { get; set; }
}
