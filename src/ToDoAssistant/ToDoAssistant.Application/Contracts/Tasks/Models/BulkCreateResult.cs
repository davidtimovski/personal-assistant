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
    public List<BulkCreatedTask> CreatedTasks { get; set; } = new();
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
    public required int Id { get; init; }
    public required string Name { get; init; }
}
