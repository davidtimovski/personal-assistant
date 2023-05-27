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

    public string TaskName { get; set; }
    public string ListName { get; set; }
    public string ActionUserName { get; set; }
    public string ActionUserImageUri { get; set; }
    public IEnumerable<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    public bool Notify()
    {
        return NotificationRecipients.Any();
    }
}
