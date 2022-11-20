using Application.Contracts;
using Application.Contracts.Models;

namespace CookingAssistant.Application.Contracts.Recipes.Models;

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
