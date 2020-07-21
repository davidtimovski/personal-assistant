using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications
{
    public interface INotificationsRepository
    {
        Task<IEnumerable<Notification>> GetAllAndFlagUnseenAsync(int userId);
        Task DeleteOldAsync(DateTime from);
        Task DeleteForUserAndListAsync(int userId, int listId);
        Task<int> GetUnseenNotificationsCountAsync(int userId);
        Task<int> CreateOrUpdateAsync(Notification notification);
    }
}
