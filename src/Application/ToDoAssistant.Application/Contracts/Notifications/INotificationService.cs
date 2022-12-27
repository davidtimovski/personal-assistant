using ToDoAssistant.Application.Contracts.Notifications.Models;

namespace ToDoAssistant.Application.Contracts.Notifications;

public interface INotificationService
{
    IEnumerable<NotificationDto> GetAllAndFlagUnseen(int userId);
    int GetUnseenNotificationsCount(int userId);
    Task<int> CreateOrUpdateAsync(CreateOrUpdateNotification model);
}
