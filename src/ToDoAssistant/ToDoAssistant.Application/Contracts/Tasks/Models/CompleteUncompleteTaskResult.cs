using Core.Application.Contracts;
using Core.Application.Contracts.Models;

namespace ToDoAssistant.Application.Contracts.Tasks.Models;

public class CompleteUncompleteTaskResult : INotificationResult, IResult
{
    public CompleteUncompleteTaskResult()
    {
        Failed = true;
    }

    public CompleteUncompleteTaskResult(bool success)
    {
        Failed = !success;
    }

    public bool Failed { get; private set; }

    public int ListId { get; init; }
    public bool NotifySignalR { get; init; }

    public string TaskName { get; init; } = null!;
    public string ListName { get; init; } = null!;
    public string ActionUserName { get; init; } = null!;
    public string ActionUserImageUri { get; init; } = null!;
    public IReadOnlyCollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    public bool Notify()
    {
        return NotificationRecipients.Any();
    }
}
