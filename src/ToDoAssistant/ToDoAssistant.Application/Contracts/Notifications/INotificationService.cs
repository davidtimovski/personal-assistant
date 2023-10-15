using Core.Application.Contracts;
using Sentry;
using ToDoAssistant.Application.Contracts.Notifications.Models;

namespace ToDoAssistant.Application.Contracts.Notifications;

public interface INotificationService
{
    Result<IEnumerable<NotificationDto>> GetAllAndFlagUnseen(int userId);
    Result<int> GetUnseenNotificationsCount(int userId);
    Task<Result<int>> CreateOrUpdateAsync(CreateOrUpdateNotification model, ISpan metricsSpan, CancellationToken cancellationToken);
}
