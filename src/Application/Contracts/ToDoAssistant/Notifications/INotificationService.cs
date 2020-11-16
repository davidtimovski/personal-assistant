using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications.Models;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetAllAndFlagUnseenAsync(int userId);
        Task<int> GetUnseenNotificationsCountAsync(int userId);
        Task<int> CreateOrUpdateAsync(CreateOrUpdateNotification model);
    }
}
