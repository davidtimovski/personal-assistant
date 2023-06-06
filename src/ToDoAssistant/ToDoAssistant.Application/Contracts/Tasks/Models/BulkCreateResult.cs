using Core.Application.Contracts;
using Core.Application.Contracts.Models;

namespace ToDoAssistant.Application.Contracts.Tasks.Models;

public class BulkCreateResult : INotificationResult
{
    public BulkCreateResult(int listId, bool notifySignalR)
    {
        ListId = listId;
        NotifySignalR = notifySignalR;
    }

    public int ListId { get; }
    public bool NotifySignalR { get; }

    public string ListName { get; set; } = null!;
    public IEnumerable<BulkCreatedTask> CreatedTasks { get; set; }
    public string ActionUserName { get; set; } = null!;
    public string ActionUserImageUri { get; set; } = null!;
    public List<NotificationRecipient> NotificationRecipients { get; set; } = new();

    public bool Notify()
    {
        return NotificationRecipients.Any() && CreatedTasks.Any();
    }
}

public class BulkCreatedTask
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}
