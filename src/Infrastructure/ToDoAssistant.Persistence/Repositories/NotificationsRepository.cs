using System.Data;
using Application.Domain.ToDoAssistant;
using Core.Persistence;
using Dapper;
using Sentry;
using ToDoAssistant.Application.Contracts.Notifications;
using User = Application.Domain.Common.User;

namespace ToDoAssistant.Persistence.Repositories;

public class NotificationsRepository : BaseRepository, INotificationsRepository
{
    public NotificationsRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public IEnumerable<Notification> GetAllAndFlagUnseen(int userId)
    {
        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT n.*, u.id, u.name, u.image_uri
                               FROM todo.notifications AS n
                               INNER JOIN users AS u ON n.action_user_id = u.id
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
            conn.Execute(@"UPDATE todo.notifications SET is_seen = TRUE WHERE id = ANY(@UnseenNotificationIds)",
                new { UnseenNotificationIds = unseenNotificationIds });
        }

        return notifications;
    }

    public int GetUnseenNotificationsCount(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<int>(@"SELECT COUNT(*) FROM todo.notifications WHERE user_id = @UserId AND is_seen = FALSE",
            new { UserId = userId });
    }

    public async Task DeleteForUserAndListAsync(int userId, int listId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(NotificationsRepository)}.{nameof(DeleteForUserAndListAsync)}");

        using IDbConnection conn = OpenConnection();

        await conn.ExecuteAsync(@"DELETE FROM todo.notifications WHERE user_id = @UserId AND list_id = @ListId",
            new { UserId = userId, ListId = listId });

        metric.Finish();
    }

    public async Task<int> CreateOrUpdateAsync(Notification notification, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(NotificationsRepository)}.{nameof(CreateOrUpdateAsync)}");

        using IDbConnection conn = OpenConnection();

        var id = await conn.QueryFirstOrDefaultAsync<int?>(@"SELECT id
                                                             FROM todo.notifications 
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

        metric.Finish();

        return notification.Id;
    }
}
