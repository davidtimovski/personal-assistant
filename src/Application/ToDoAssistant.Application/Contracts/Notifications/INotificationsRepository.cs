using Application.Domain.ToDoAssistant;

namespace ToDoAssistant.Application.Contracts.Notifications;

public interface INotificationsRepository
{
    IEnumerable<Notification> GetAllAndFlagUnseen(int userId);
    int GetUnseenNotificationsCount(int userId);
    Task DeleteForUserAndListAsync(int userId, int listId);
    Task<int> CreateOrUpdateAsync(Notification notification);
}
