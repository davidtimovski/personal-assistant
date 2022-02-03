using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Contracts.ToDoAssistant.Notifications.Models;

namespace Application.Contracts.ToDoAssistant.Notifications;

public interface INotificationService
{
    IEnumerable<NotificationDto> GetAllAndFlagUnseen(int userId);
    int GetUnseenNotificationsCount(int userId);
    Task<int> CreateOrUpdateAsync(CreateOrUpdateNotification model);
}