using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications
{
    public interface INotificationsRepository
    {
        IEnumerable<Notification> GetAllAndFlagUnseen(int userId);
        int GetUnseenNotificationsCount(int userId);
        Task DeleteForUserAndListAsync(int userId, int listId);
        Task<int> CreateOrUpdateAsync(Notification notification);
    }
}
