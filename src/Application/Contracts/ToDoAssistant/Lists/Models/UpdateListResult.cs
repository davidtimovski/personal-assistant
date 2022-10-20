using System.Collections.Generic;
using System.Linq;
using Application.Contracts.Common;
using Application.Contracts.Common.Models;

namespace Application.Contracts.ToDoAssistant.Lists.Models;

public class UpdateListResult : INotificationResult
{
    public ListNotificationType Type { get; set; }
    public string OriginalListName { get; set; }
    public string ActionUserName { get; set; }
    public string ActionUserImageUri { get; set; }
    public IEnumerable<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    public bool Notify()
    {
        return NotificationRecipients.Any();
    }
}
