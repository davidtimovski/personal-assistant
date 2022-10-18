using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.ToDoAssistant.Notifications;
using Dapper;
using Domain.Entities.Common;
using Domain.Entities.ToDoAssistant;

namespace Persistence.Repositories.ToDoAssistant;

public class NotificationsRepository : BaseRepository, INotificationsRepository
{
    public NotificationsRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    // TODO: Change/remove references to AspNetUsers
    public IEnumerable<Notification> GetAllAndFlagUnseen(int userId)
    {
        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT n.*, u.""Id"", u.""ImageUri""
                               FROM todo_notifications AS n
                               INNER JOIN ""AspNetUsers"" AS u ON n.action_user_id = u.""Id""
                               WHERE user_id = @UserId
                               ORDER BY modified_date DESC";

        var notifications = conn.Query<Notification, User, Notification>(query,
            (notification, user) =>
            {
                notification.User = user;
                return notification;
            }, new { UserId = userId }).ToList();

        // Flag unseen as seen
        var unseenNotificationIds = notifications.Where(x => !x.IsSeen).Select(x => x.Id).ToList();
        if (unseenNotificationIds.Count > 0)
        {
            conn.Execute(@"UPDATE todo_notifications SET is_seen = TRUE WHERE id = ANY(@UnseenNotificationIds)",
                new { UnseenNotificationIds = unseenNotificationIds });
        }

        return notifications;
    }

    public int GetUnseenNotificationsCount(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<int>(@"SELECT COUNT(*) FROM todo_notifications WHERE user_id = @UserId AND is_seen = FALSE",
            new { UserId = userId });
    }

    public async Task DeleteForUserAndListAsync(int userId, int listId)
    {
        using IDbConnection conn = OpenConnection();

        await conn.ExecuteAsync(@"DELETE FROM todo_notifications WHERE user_id = @UserId AND list_id = @ListId",
            new { UserId = userId, ListId = listId });
    }

    public async Task<int> CreateOrUpdateAsync(Notification notification)
    {
        using IDbConnection conn = OpenConnection();

        var id = await conn.QueryFirstOrDefaultAsync<int?>(@"SELECT id
                                                             FROM todo_notifications 
                                                             WHERE user_id = @UserId AND message = @Message AND is_seen = FALSE",
            new { notification.UserId, notification.Message });

        if (id != null)
        {
            notification.Id = id.Value;

            Notification dbNotification = await EFContext.Notifications.FindAsync(id);
            dbNotification.ModifiedDate = notification.ModifiedDate;
        }
        else
        {
            EFContext.Notifications.Add(notification);
        }

        await EFContext.SaveChangesAsync();

        return notification.Id;
    }
}
