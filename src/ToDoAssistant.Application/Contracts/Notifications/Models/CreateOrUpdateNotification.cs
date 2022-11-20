namespace ToDoAssistant.Application.Contracts.Notifications.Models;

public class CreateOrUpdateNotification
{
    public CreateOrUpdateNotification(int userId, int actionUserId, int? listId, int? taskId, string message)
    {
        UserId = userId;
        ActionUserId = actionUserId;
        ListId = listId;
        TaskId = taskId;
        Message = message;
    }

    public int UserId { get; set; }
    public int ActionUserId { get; set; }
    public int? ListId { get; set; }
    public int? TaskId { get; set; }
    public string Message { get; set; }
}