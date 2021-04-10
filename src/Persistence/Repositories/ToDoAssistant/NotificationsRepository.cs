using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using Persistence;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications;
using PersonalAssistant.Domain.Entities.Common;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Persistence.Repositories.ToDoAssistant
{
    public class NotificationsRepository : BaseRepository, INotificationsRepository
    {
        public NotificationsRepository(IOptions<DatabaseSettings> databaseSettings, PersonalAssistantContext efContext)
            : base(databaseSettings.Value.DefaultConnectionString, efContext) { }

        public async Task<IEnumerable<Notification>> GetAllAndFlagUnseenAsync(int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var sql = @"SELECT n.*, u.""Id"", u.""ImageUri""
                            FROM ""ToDoAssistant.Notifications"" AS n
                            INNER JOIN ""AspNetUsers"" AS u ON n.""ActionUserId"" = u.""Id""
                            WHERE ""UserId"" = @UserId
                            ORDER BY ""ModifiedDate"" DESC";

            var notifications = await conn.QueryAsync<Notification, User, Notification>(sql,
                (notification, user) =>
                {
                    notification.User = user;
                    return notification;
                }, new { UserId = userId }, null, true);

            // Flag unseen as seen
            var unseenNotificationIds = notifications.Where(x => !x.IsSeen).Select(x => x.Id).ToList();
            if (unseenNotificationIds.Count > 0)
            {
                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Notifications"" SET ""IsSeen"" = TRUE WHERE ""Id"" = ANY(@UnseenNotificationIds)",
                    new { UnseenNotificationIds = unseenNotificationIds });
            }

            return notifications;
        }

        public async Task DeleteOldAsync(DateTime from)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"DELETE FROM ""ToDoAssistant.Notifications"" WHERE ""CreatedDate"" < @DeleteFrom", new { DeleteFrom = from });
        }

        public async Task DeleteForUserAndListAsync(int userId, int listId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"DELETE FROM ""ToDoAssistant.Notifications"" WHERE ""UserId"" = @UserId AND ""ListId"" = @ListId",
                new { UserId = userId, ListId = listId });
        }

        public async Task<int> GetUnseenNotificationsCountAsync(int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<int>(@"SELECT COUNT(*) FROM ""ToDoAssistant.Notifications"" WHERE ""UserId"" = @UserId AND ""IsSeen"" = FALSE",
                new { UserId = userId });
        }

        public async Task<int> CreateOrUpdateAsync(Notification notification)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var id = await conn.QueryFirstOrDefaultAsync<int?>(@"SELECT ""Id""
                                                                     FROM ""ToDoAssistant.Notifications"" 
                                                                     WHERE ""UserId"" = @UserId AND ""Message"" = @Message AND ""IsSeen"" = FALSE", notification);

            if (id != null)
            {
                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Notifications""
                                              SET ""ModifiedDate"" = @ModifiedDate
                                              WHERE ""Id"" = @Id", new { Id = id, notification.ModifiedDate });
                return id.Value;
            }
            else
            {
                return (await conn.QueryAsync<int>(@"INSERT INTO ""ToDoAssistant.Notifications"" (""UserId"", ""ActionUserId"", ""ListId"", ""TaskId"", ""Message"", ""IsSeen"", ""CreatedDate"", ""ModifiedDate"")
                                                         VALUES (@UserId, @ActionUserId, @ListId, @TaskId, @Message, @IsSeen, @CreatedDate, @ModifiedDate) returning ""Id""", notification)).Single();
            }
        }
    }
}
