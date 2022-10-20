using System.Collections.Generic;
using Application.Contracts.Common;
using Application.Contracts.Common.Models;

namespace Application.Contracts.CookingAssistant.Recipes.Models;

public class DeclineSendRequestResult : INotificationResult
{
    public string RecipeName { get; set; }
    public string ActionUserName { get; set; }
    public string ActionUserImageUri { get; set; }
    public IEnumerable<NotificationRecipient> NotificationRecipients { get; set; }

    public bool Notify()
    {
        return true;
    }
}
