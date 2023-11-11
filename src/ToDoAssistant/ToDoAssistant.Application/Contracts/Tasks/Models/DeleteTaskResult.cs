using Core.Application.Contracts;
using Core.Application.Contracts.Models;

namespace ToDoAssistant.Application.Contracts.Tasks.Models;

public class DeleteTaskResult : INotificationResult, IResult
{
    public DeleteTaskResult()
    {
        Failed = true;
    }

    public DeleteTaskResult(bool success)
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
    public IReadOnlyList<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    public bool Notify()
    {
        return NotificationRecipients.Any();
    }
}
