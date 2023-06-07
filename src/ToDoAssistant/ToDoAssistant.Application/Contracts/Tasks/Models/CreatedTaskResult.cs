using Core.Application.Contracts;
using Core.Application.Contracts.Models;

namespace ToDoAssistant.Application.Contracts.Tasks.Models;

public class CreatedTaskResult : INotificationResult
{
    public CreatedTaskResult(int taskId, int listId, bool notifySignalR)
    {
        TaskId = taskId;
        ListId = listId;
        NotifySignalR = notifySignalR;
    }

    public int TaskId { get; }
    public int ListId { get; }
    public bool NotifySignalR { get; }

    public string TaskName { get; set; } = null!;
    public string ListName { get; set; } = null!;
    public string ActionUserName { get; set; } = null!;
    public string ActionUserImageUri { get; set; } = null!;
    public List<NotificationRecipient> NotificationRecipients { get; set; } = new();

    public bool Notify()
    {
        return NotificationRecipients.Any();
    }
}
