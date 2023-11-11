using Sentry;
using ToDoAssistant.Application.Entities;

namespace ToDoAssistant.Application.Contracts.Notifications;

public interface INotificationsRepository
{
    IReadOnlyList<Notification> GetAllAndFlagUnseen(int userId);
    int GetUnseenNotificationsCount(int userId);
    Task DeleteForUserAndListAsync(int userId, int listId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<int> CreateOrUpdateAsync(Notification notification, ISpan metricsSpan, CancellationToken cancellationToken);
}
