using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications.Models;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications
{
    public interface INotificationService
    {
        IEnumerable<NotificationDto> GetAllAndFlagUnseen(int userId);
        int GetUnseenNotificationsCount(int userId);
        Task<int> CreateOrUpdateAsync(CreateOrUpdateNotification model);
    }
}
