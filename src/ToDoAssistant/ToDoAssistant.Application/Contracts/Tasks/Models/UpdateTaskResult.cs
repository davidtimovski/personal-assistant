using Core.Application.Contracts;
using Core.Application.Contracts.Models;

namespace ToDoAssistant.Application.Contracts.Tasks.Models;

public class UpdateTaskResult : INotificationResult
{
    public UpdateTaskResult(string originalTaskName, int listId, string listName, bool notifySignalR)
    {
        OriginalTaskName = originalTaskName;
        ListId = listId;
        ListName = listName;
        NotifySignalR = notifySignalR;
    }

    public string OriginalTaskName { get; } = null!;
    public int ListId { get; }
    public string ListName { get; } = null!;
    public bool NotifySignalR { get; }

    public int? OldListId { get; set; }
    public string? OldListName { get; set; }
    public string ActionUserName { get; set; } = null!;
    public string ActionUserImageUri { get; set; } = null!;
    public List<NotificationRecipient> NotificationRecipients { get; set; } = new();
    public List<NotificationRecipient> RemovedNotificationRecipients { get; set; } = new();
    public List<NotificationRecipient> CreatedNotificationRecipients { get; set; } = new();
    public NotificationRecipient? AssignedNotificationRecipient { get; set; }

    public bool Notify()
    {
        return NotificationRecipients.Any() || RemovedNotificationRecipients.Any() || CreatedNotificationRecipients.Any() || AssignedNotificationRecipient != null;
    }
}
