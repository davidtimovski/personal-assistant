using Core.Application.Contracts.Models;

namespace Core.Application.Contracts;

/// <summary>
/// Used for push notifications.
/// </summary>
public interface INotificationResult
{
    public string ActionUserName { get; }
    public string ActionUserImageUri { get; }
    public IReadOnlyCollection<NotificationRecipient> NotificationRecipients { get; }

    public bool Notify();
}
