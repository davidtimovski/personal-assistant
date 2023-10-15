using Core.Application.Contracts;
using Core.Application.Contracts.Models;

namespace Chef.Application.Contracts.Recipes.Models;

public class DeclineSendRequestResult : INotificationResult
{
    public required string RecipeName { get; set; }
    public required string ActionUserName { get; set; }
    public required string ActionUserImageUri { get; set; }
    public required IReadOnlyCollection<NotificationRecipient> NotificationRecipients { get; set; }

    public bool Notify()
    {
        return NotificationRecipients.Any();
    }
}
