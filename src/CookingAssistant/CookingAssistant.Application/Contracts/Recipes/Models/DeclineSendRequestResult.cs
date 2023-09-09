using Core.Application.Contracts;
using Core.Application.Contracts.Models;

namespace CookingAssistant.Application.Contracts.Recipes.Models;

public class DeclineSendRequestResult : INotificationResult
{
    public required string RecipeName { get; set; }
    public required string ActionUserName { get; set; }
    public required string ActionUserImageUri { get; set; }
    public required List<NotificationRecipient> NotificationRecipients { get; set; }

    public bool Notify()
    {
        return true;
    }
}
