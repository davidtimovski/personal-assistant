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

    public int UserId { get; private set; }
    public int ActionUserId { get; private set; }
    public int? ListId { get; private set; }
    public int? TaskId { get; private set; }
    public string Message { get; private set; }
}
