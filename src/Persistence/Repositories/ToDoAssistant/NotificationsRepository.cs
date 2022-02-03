using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Application.Contracts.ToDoAssistant.Notifications;
using Domain.Entities.Common;
using Domain.Entities.ToDoAssistant;

namespace Persistence.Repositories.ToDoAssistant;

public class NotificationsRepository : BaseRepository, INotificationsRepository
{
    public NotificationsRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public IEnumerable<Notification> GetAllAndFlagUnseen(int userId)
    {
        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT n.*, u.""Id"", u.""ImageUri""
                            FROM ""ToDoAssistant.Notifications"" AS n
                            INNER JOIN ""AspNetUsers"" AS u ON n.""ActionUserId"" = u.""Id""
                            WHERE ""UserId"" = @UserId
                            ORDER BY ""ModifiedDate"" DESC";

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
            conn.Execute(@"UPDATE ""ToDoAssistant.Notifications"" SET ""IsSeen"" = TRUE WHERE ""Id"" = ANY(@UnseenNotificationIds)",
                new { UnseenNotificationIds = unseenNotificationIds });
        }

        return notifications;
    }

    public int GetUnseenNotificationsCount(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<int>(@"SELECT COUNT(*) FROM ""ToDoAssistant.Notifications"" WHERE ""UserId"" = @UserId AND ""IsSeen"" = FALSE",
            new { UserId = userId });
    }

    public async Task DeleteForUserAndListAsync(int userId, int listId)
    {
        using IDbConnection conn = OpenConnection();

        await conn.ExecuteAsync(@"DELETE FROM ""ToDoAssistant.Notifications"" WHERE ""UserId"" = @UserId AND ""ListId"" = @ListId",
            new { UserId = userId, ListId = listId });
    }

    public async Task<int> CreateOrUpdateAsync(Notification notification)
    {
        using IDbConnection conn = OpenConnection();

        var id = await conn.QueryFirstOrDefaultAsync<int?>(@"SELECT ""Id""
                                                                 FROM ""ToDoAssistant.Notifications"" 
                                                                 WHERE ""UserId"" = @UserId AND ""Message"" = @Message AND ""IsSeen"" = FALSE",
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