using Application.Contracts;
using Application.Contracts.Models;

namespace ToDoAssistant.Application.Contracts.Lists.Models;

public class DeleteListResult : INotificationResult
{
    public string DeletedListName { get; set; }
    public string ActionUserName { get; set; }
    public string ActionUserImageUri { get; set; }
    public IEnumerable<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    public bool Notify()
    {
        return NotificationRecipients.Any();
    }
}
