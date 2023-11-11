using System.Data;
using Dapper;
using Sentry;
using ToDoAssistant.Application.Contracts.Notifications;
using ToDoAssistant.Application.Entities;
using User = Core.Application.Entities.User;

namespace ToDoAssistant.Persistence.Repositories;

public class NotificationsRepository : BaseRepository, INotificationsRepository
{
    public NotificationsRepository(ToDoAssistantContext efContext)
        : base(efContext) { }

    public IReadOnlyList<Notification> GetAllAndFlagUnseen(int userId)
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
            conn.Execute("UPDATE todo.notifications SET is_seen = TRUE WHERE id = ANY(@UnseenNotificationIds)",
                new { UnseenNotificationIds = unseenNotificationIds });
        }

        return notifications;
    }

    public int GetUnseenNotificationsCount(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM todo.notifications WHERE user_id = @UserId AND is_seen = FALSE",
            new { UserId = userId });
    }

    public async Task DeleteForUserAndListAsync(int userId, int listId, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(NotificationsRepository)}.{nameof(DeleteForUserAndListAsync)}");

        try
        {
            using IDbConnection conn = OpenConnection();

            await conn.ExecuteAsync(new CommandDefinition("DELETE FROM todo.notifications WHERE user_id = @UserId AND list_id = @ListId",
                new { UserId = userId, ListId = listId },
                cancellationToken: cancellationToken));
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<int> CreateOrUpdateAsync(Notification notification, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(NotificationsRepository)}.{nameof(CreateOrUpdateAsync)}");

        try
        {
            using IDbConnection conn = OpenConnection();

            var id = await conn.QueryFirstOrDefaultAsync<int?>(
                new CommandDefinition(@"SELECT id
                                    FROM todo.notifications 
                                    WHERE user_id = @UserId AND message = @Message AND is_seen = FALSE",
                new { notification.UserId, notification.Message },
                cancellationToken: cancellationToken));

            if (id != null)
            {
                notification.Id = id.Value;

                Notification dbNotification = EFContext.Notifications.First(x => x.Id == id);
                dbNotification.ModifiedDate = notification.ModifiedDate;
            }
            else
            {
                EFContext.Notifications.Add(notification);
            }

            await EFContext.SaveChangesAsync(cancellationToken);

            return notification.Id;
        }
        finally
        {
            metric.Finish();
        }
    }
}
