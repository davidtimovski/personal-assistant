using Application.Contracts;
using Application.Contracts.Models;

namespace CookingAssistant.Application.Contracts.Recipes.Models;

public class LeaveRecipeResult : INotificationResult
{
    public string RecipeName { get; set; }
    public string ActionUserName { get; set; }
    public string ActionUserImageUri { get; set; }
    public IEnumerable<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    public bool Notify()
    {
        return NotificationRecipients.Any();
    }
}
