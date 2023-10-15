using Core.Application.Contracts;
using Core.Application.Contracts.Models;

namespace Chef.Application.Contracts.Recipes.Models;

public class DeleteRecipeResult : INotificationResult
{
    public string RecipeName { get; set; } = null!;
    public string ActionUserName { get; set; } = null!;
    public string ActionUserImageUri { get; set; } = null!;
    public IReadOnlyCollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    public bool Notify()
    {
        return NotificationRecipients.Any();
    }
}
