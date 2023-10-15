using Core.Application.Contracts;
using Core.Application.Contracts.Models;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class DeleteListResult : INotificationResult, IResult
{
    public DeleteListResult()
    {
        Failed = true;
    }

    public DeleteListResult(bool success)
    {
        Failed = !success;
    }

    public bool Failed { get; private set; }

    public string DeletedListName { get; set; } = null!;
    public string ActionUserName { get; set; } = null!;
    public string ActionUserImageUri { get; set; } = null!;
    public IReadOnlyCollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    public bool Notify()
    {
        return NotificationRecipients.Any();
    }
}
