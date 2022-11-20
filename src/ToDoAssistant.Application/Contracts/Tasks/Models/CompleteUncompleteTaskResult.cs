using Application.Contracts;
using Application.Contracts.Models;

namespace ToDoAssistant.Application.Contracts.Tasks.Models;

public class CompleteUncompleteTaskResult : INotificationResult
{
    public CompleteUncompleteTaskResult(bool notifySignalR)
    {
        NotifySignalR = notifySignalR;
    }

    public CompleteUncompleteTaskResult(int listId, bool notifySignalR)
    {
        ListId = listId;
        NotifySignalR = notifySignalR;
    }

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
