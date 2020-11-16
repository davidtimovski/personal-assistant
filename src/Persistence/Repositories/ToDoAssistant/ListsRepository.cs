using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;
using PersonalAssistant.Domain.Entities.Common;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Persistence.Repositories.ToDoAssistant
{
    public class ListsRepository : BaseRepository, IListsRepository
    {
        public ListsRepository(IOptions<DatabaseSettings> databaseSettings)
            : base(databaseSettings.Value.DefaultConnectionString) { }

        public async Task<IEnumerable<ToDoList>> GetAllAsOptionsAsync(int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var sql = @"SELECT DISTINCT l.*, s.""UserId"", s.""IsAccepted""
                            FROM ""ToDoAssistant.Lists"" AS l
                            LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                            WHERE l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted"")
                            ORDER BY l.""Order""";

            return await conn.QueryAsync<ToDoList, Share, ToDoList>(sql,
                (list, share) =>
                {
                    if (share != null && share.IsAccepted != false)
                    {
                        list.Shares.Add(share);
                    }
                    return list;
                }, new { UserId = userId }, null, true, "UserId");
        }

        public async Task<IEnumerable<ToDoList>> GetAllWithTasksAndSharingDetailsAsync(int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var lists = await conn.QueryAsync<ToDoList>(@"SELECT l.*
                                                          FROM ""ToDoAssistant.Lists"" AS l
                                                          LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId"" 
                                                            AND s.""UserId"" = @UserId
                                                            AND s.""IsAccepted"" 
                                                          WHERE (l.""UserId"" = @UserId OR s.""UserId"" = @UserId)",
                                                          new { UserId = userId });

            var listIds = lists.Select(x => x.Id).ToArray();
            var shares = await conn.QueryAsync<Share>(@"SELECT * FROM ""ToDoAssistant.Shares"" WHERE ""ListId"" = ANY(@ListIds)", new { ListIds = listIds });

            var tasks = await conn.QueryAsync<ToDoTask>(@"SELECT *
                                                          FROM ""ToDoAssistant.Tasks""
                                                          WHERE ""ListId"" = ANY(@ListIds)
                                                            AND (""PrivateToUserId"" IS NULL OR ""PrivateToUserId"" = @UserId)",
                                                          new { ListIds = listIds, UserId = userId });

            foreach (var list in lists)
            {
                list.Tasks.AddRange(tasks.Where(x => x.ListId == list.Id));
                list.Shares.AddRange(shares.Where(x => x.ListId == list.Id));
            }

            return lists;
        }

        public async Task<IEnumerable<User>> GetMembersAsAssigneeOptionsAsync(int id)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryAsync<User>(@"SELECT DISTINCT u.""Id"", u.""Name"", u.""ImageUri""
                                                FROM ""AspNetUsers"" AS u
                                                LEFT JOIN ""ToDoAssistant.Lists"" AS l ON u.""Id"" = l.""UserId""
                                                LEFT JOIN ""ToDoAssistant.Shares"" AS s ON u.""Id"" = s.""UserId""
                                                WHERE l.""Id"" = @ListId OR (s.""ListId"" = @ListId AND s.""IsAccepted"" IS NOT FALSE)
                                                ORDER BY u.""Name""", new { ListId = id });
        }

        public async Task<ToDoList> GetAsync(int id)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryFirstOrDefaultAsync<ToDoList>(@"SELECT * FROM ""ToDoAssistant.Lists"" WHERE ""Id"" = @Id", new { Id = id });
        }

        public async Task<ToDoList> GetAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var sql = @"SELECT l.*, s.""UserId"", s.""IsAdmin"", s.""IsAccepted"", 
                            s.""Order"", s.""NotificationsEnabled"", s.""IsArchived""
                        FROM ""ToDoAssistant.Lists"" AS l
                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                        WHERE l.""Id"" = @Id AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))";

            return (await conn.QueryAsync<ToDoList, Share, ToDoList>(sql,
                (list, share) =>
                {
                    if (share != null)
                    {
                        list.Shares.Add(share);
                    }
                    return list;
                }, new { Id = id, UserId = userId }, null, true, "UserId")).First();
        }

        public async Task<ToDoList> GetWithOwnerAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var sql = @"SELECT DISTINCT l.*, users.""Id"", users.""Email"", users.""ImageUri""
                        FROM ""ToDoAssistant.Lists"" AS l
                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                        INNER JOIN ""AspNetUsers"" AS users ON l.""UserId"" = users.""Id""
                        WHERE l.""Id"" = @Id AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))";

            return (await conn.QueryAsync<ToDoList, User, ToDoList>(sql,
                (list, user) =>
                {
                    list.User = user;
                    return list;
                }, new { Id = id, UserId = userId }, null, true)).FirstOrDefault();
        }

        public async Task<IEnumerable<Share>> GetSharesAsync(int id)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var sql = @"SELECT s.*, u.""Id"", u.""Email"", u.""ImageUri""
                        FROM ""ToDoAssistant.Shares"" AS s
                        INNER JOIN ""AspNetUsers"" AS u ON s.""UserId"" = u.""Id""
                        WHERE s.""ListId"" = @ListId AND s.""IsAccepted"" IS NOT FALSE
                        ORDER BY (CASE WHEN s.""IsAccepted"" THEN 1 ELSE 2 END) ASC, s.""CreatedDate""";

            return await conn.QueryAsync<Share, User, Share>(sql,
                (share, user) =>
                {
                    share.User = user;
                    return share;
                }, new { ListId = id }, null, true);
        }

        public async Task<IEnumerable<Share>> GetShareRequestsAsync(int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var sql = @"SELECT s.*, l.""Name"", u.""Name""
                        FROM ""ToDoAssistant.Shares"" AS s
                        INNER JOIN ""ToDoAssistant.Lists"" AS l ON s.""ListId"" = l.""Id""
                        INNER JOIN ""AspNetUsers"" AS u ON l.""UserId"" = u.""Id""
                        WHERE s.""UserId"" = @UserId
                        ORDER BY s.""ModifiedDate"" DESC";

            return await conn.QueryAsync<Share, ToDoList, User, Share>(sql,
                (share, list, user) =>
                {
                    share.List = list;
                    share.User = user;
                    return share;
                }, new { UserId = userId }, null, true, "Name");
        }

        public async Task<int> GetPendingShareRequestsCountAsync(int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<int>(@"SELECT COUNT(*) FROM ""ToDoAssistant.Shares"" WHERE ""UserId"" = @UserId AND ""IsAccepted"" IS NULL",
                new { UserId = userId });
        }

        public async Task<bool> CanShareWithUserAsync(int shareWithId, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return !await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""ToDoAssistant.Lists"" AS l
                                                        INNER JOIN ""ToDoAssistant.Shares"" AS s on l.""Id"" = s.""ListId""
                                                        WHERE l.""UserId"" = @UserId AND s.""UserId"" = @ShareWithId AND s.""IsAccepted"" = FALSE",
                                                          new { ShareWithId = shareWithId, UserId = userId });
        }

        public async Task<bool> UserOwnsOrSharesAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""ToDoAssistant.Lists"" AS l
                                                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                        WHERE l.""Id"" = @Id AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))",
                new { Id = id, UserId = userId });
        }

        public async Task<bool> UserOwnsOrSharesAsPendingAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""ToDoAssistant.Lists"" AS l
                                                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                        WHERE l.""Id"" = @Id AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted"" IS NOT FALSE))",
                new { Id = id, UserId = userId });
        }

        public async Task<bool> UserOwnsOrSharesAsAdminAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""ToDoAssistant.Lists"" AS l
                                                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                        WHERE l.""Id"" = @Id AND (l.""UserId"" = @UserId 
                                                            OR (s.""UserId"" = @UserId AND s.""IsAccepted"" AND s.""IsAdmin"" = TRUE))",
                new { Id = id, UserId = userId });
        }

        public async Task<bool> UserOwnsOrSharesAsAdminAsync(int id, string name, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""ToDoAssistant.Lists"" AS l
                                                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                        WHERE l.""Id"" != @Id AND UPPER(l.""Name"") = UPPER(@Name) 
                                                            AND (l.""UserId"" = @UserId 
                                                            OR (s.""UserId"" = @UserId AND s.""IsAccepted"" AND s.""IsAdmin"" = TRUE))",
                new { Id = id, Name = name, UserId = userId });
        }

        public async Task<bool> UserOwnsAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""ToDoAssistant.Lists""
                                                        WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
                new { Id = id, UserId = userId });
        }

        public async Task<bool> IsSharedAsync(int id)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""ToDoAssistant.Shares""
                                                        WHERE ""ListId"" = @ListId AND ""IsAccepted"" IS NOT FALSE",
                new { ListId = id });
        }

        public async Task<bool> UserHasBlockedSharingAsync(int userId, int sharedWithId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""ToDoAssistant.Lists"" AS l
                                                        INNER JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                        WHERE l.""UserId"" = @UserId AND s.""UserId"" = @SharedWithId AND ""IsAccepted"" = FALSE",
                                                         new { UserId = userId, SharedWithId = sharedWithId });
        }

        public async Task<bool> ExistsAsync(string name, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT SUM(total) FROM
                                                        (
	                                                        SELECT COUNT(*) AS total
	                                                        FROM ""ToDoAssistant.Lists""
	                                                        WHERE UPPER(""Name"") = UPPER(@Name) AND ""UserId"" = @UserId
	                                                        UNION ALL
	                                                        SELECT COUNT(*) AS total
	                                                        FROM ""ToDoAssistant.Lists"" AS lists
	                                                        LEFT JOIN ""ToDoAssistant.Shares"" AS shares ON shares.""UserId"" = @UserId
	                                                        WHERE UPPER(lists.""Name"") = UPPER(@Name) AND lists.""UserId"" = @UserId
                                                        ) AS sumTotal",
                new { Name = name, UserId = userId });
        }

        public async Task<bool> ExistsAsync(int id, string name, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT SUM(total) FROM
                                                        (
	                                                        SELECT COUNT(*) AS total
	                                                        FROM ""ToDoAssistant.Lists""
	                                                        WHERE ""Id"" != @Id AND UPPER(""Name"") = UPPER(@Name) AND ""UserId"" = @UserId
	                                                        UNION ALL
	                                                        SELECT COUNT(*) AS total
	                                                        FROM ""ToDoAssistant.Lists"" AS lists
	                                                        LEFT JOIN ""ToDoAssistant.Shares"" AS shares ON shares.""UserId"" = @UserId
	                                                        WHERE ""Id"" != @Id AND UPPER(lists.""Name"") = UPPER(@Name) AND lists.""UserId"" = @UserId
                                                        ) AS sumTotal",
                new { Id = id, Name = name, UserId = userId });
        }

        public async Task<int> CountAsync(int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<int>(@"SELECT COUNT(*) FROM ""ToDoAssistant.Lists"" WHERE ""UserId"" = @UserId", new { UserId = userId });
        }

        public async Task<int> CreateAsync(ToDoList list)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            var listsCount = await conn.ExecuteScalarAsync<short>(@"SELECT COUNT(*)
                                                                    FROM ""ToDoAssistant.Lists"" AS l
                                                                    LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                                    WHERE l.""IsArchived"" = FALSE AND l.""UserId"" = @UserId 
                                                                        OR (s.""UserId"" = @UserId AND s.""IsAccepted"" AND s.""IsArchived"" = FALSE)",
                                                                    new { list.UserId }, transaction);
            list.Order = ++listsCount;

            list.Id = (await conn.QueryAsync<int>(@"INSERT INTO ""ToDoAssistant.Lists"" (""UserId"", ""Name"", ""Icon"", ""IsOneTimeToggleDefault"", ""Order"", ""CreatedDate"", ""ModifiedDate"") 
                                                    VALUES (@UserId, @Name, @Icon, @IsOneTimeToggleDefault, @Order, @CreatedDate, @ModifiedDate) returning ""Id""", list, transaction)).Single();

            for (short i = 0; i < list.Tasks.Count(); i++)
            {
                list.Tasks[i].ListId = list.Id;
                list.Tasks[i].Order = (short)(i + 1);
            }

            await conn.ExecuteAsync(@"INSERT INTO ""ToDoAssistant.Tasks"" (""ListId"", ""Name"", ""IsOneTime"", ""Order"", ""CreatedDate"", ""ModifiedDate"")
                                      VALUES (@ListId, @Name, @IsOneTime, @Order, @CreatedDate, @ModifiedDate)", list.Tasks, transaction);

            transaction.Commit();

            return list.Id;
        }

        public async Task<ToDoList> UpdateAsync(ToDoList list)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var originalList = await conn.QueryFirstOrDefaultAsync<ToDoList>(@"SELECT * FROM ""ToDoAssistant.Lists"" WHERE ""Id"" = @Id", new { list.Id });

            await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Lists"" 
                                        SET ""Name"" = @Name, ""Icon"" = @Icon, ""IsOneTimeToggleDefault"" = @IsOneTimeToggleDefault, ""NotificationsEnabled"" = @NotificationsEnabled, ""ModifiedDate"" = @ModifiedDate 
                                        WHERE ""Id"" = @Id", list);

            return originalList;
        }

        public async Task UpdateSharedAsync(ToDoList list)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Shares"" 
                                        SET ""NotificationsEnabled"" = @NotificationsEnabled, ""ModifiedDate"" = @ModifiedDate 
                                        WHERE ""ListId"" = @Id AND ""UserId"" = @UserId", list);
        }

        public async Task<string> DeleteAsync(int id)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            var tasksLinkedToIngredients = await conn.QueryAsync<ToDoTask>(@"SELECT tasks.""Id"", tasks.""Name""
                                                                            FROM ""ToDoAssistant.Tasks"" AS tasks
                                                                            INNER JOIN ""CookingAssistant.Ingredients"" AS ingredients ON tasks.""Id"" = ingredients.""TaskId""
                                                                            WHERE ""ListId"" = @ListId", new { ListId = id });
            foreach (var task in tasksLinkedToIngredients)
            {
                await conn.ExecuteAsync(@"UPDATE ""CookingAssistant.Ingredients""
                                            SET ""Name"" = @Name, ""ModifiedDate"" = @ModifiedDate
                                            WHERE ""TaskId"" = @Id",
                                          new { task.Id, task.Name, ModifiedDate = DateTime.Now }, transaction);
            }

            var list = await conn.QueryFirstOrDefaultAsync<ToDoList>(@"SELECT * FROM ""ToDoAssistant.Lists"" WHERE ""Id"" = @Id", new { Id = id });

            var affectedUsers = new List<(int, short?)> {
                    (list.UserId, list.Order)
                };
            affectedUsers.AddRange(await conn.QueryAsync<(int, short?)>(@"SELECT ""UserId"", ""Order""
                                                                            FROM ""ToDoAssistant.Shares""
                                                                            WHERE ""ListId"" = @Id AND ""IsAccepted"" = TRUE", new { Id = id }));

            await conn.ExecuteAsync(@"DELETE FROM ""ToDoAssistant.Lists"" WHERE ""Id"" = @Id", new { Id = id }, transaction);

            foreach (var affectedUser in affectedUsers)
            {
                var parameters = new { UserId = affectedUser.Item1, Order = affectedUser.Item2 };

                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Lists""
                                            SET ""Order"" = ""Order"" - 1
                                            WHERE ""UserId"" = @UserId AND ""IsArchived"" = FALSE AND ""Order"" > @Order",
                                          parameters, transaction);

                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Shares"" 
                                            SET ""Order"" = ""Order"" - 1 
                                            WHERE ""UserId"" = @UserId AND ""IsAccepted"" = TRUE AND ""IsArchived"" = FALSE AND ""Order"" > @Order",
                                          parameters, transaction);
            }

            transaction.Commit();

            return list.Name;
        }

        public async Task SaveSharingDetailsAsync(IEnumerable<Share> newShares, IEnumerable<Share> editedShares, IEnumerable<Share> removedShares)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            await conn.ExecuteAsync(@"DELETE FROM ""ToDoAssistant.Shares""
                                        WHERE ""ListId"" = @ListId AND ""UserId"" = @UserId AND ""IsAccepted"" IS DISTINCT FROM FALSE", removedShares, transaction);

            await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Shares""
                                        SET ""IsAdmin"" = @IsAdmin, ""ModifiedDate"" = @ModifiedDate
                                        WHERE ""ListId"" = @ListId AND ""UserId"" = @UserId AND ""IsAccepted"" IS DISTINCT FROM FALSE", editedShares, transaction);

            await conn.ExecuteAsync(@"INSERT INTO ""ToDoAssistant.Shares"" (""ListId"", ""UserId"", ""IsAdmin"", ""CreatedDate"", ""ModifiedDate"") 
                                        VALUES (@ListId, @UserId, @IsAdmin, @CreatedDate, @ModifiedDate)", newShares, transaction);

            transaction.Commit();
        }

        public async Task<Share> LeaveAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            var tasksLinkedToIngredients = await conn.QueryAsync<ToDoTask>(@"SELECT t.""Id"", t.""Name""
                                                                            FROM ""ToDoAssistant.Tasks"" AS t
                                                                            INNER JOIN ""CookingAssistant.Ingredients"" AS i ON t.""Id"" = i.""TaskId""
                                                                            WHERE ""ListId"" = @ListId", new { ListId = id }, transaction);
            foreach (var task in tasksLinkedToIngredients)
            {
                await conn.ExecuteAsync(@"UPDATE ""CookingAssistant.Ingredients""
                                            SET ""Name"" = @Name, ""ModifiedDate"" = @ModifiedDate
                                            WHERE ""TaskId"" = @Id AND ""UserId"" = @UserId",
                                          new { task.Id, UserId = userId, task.Name, ModifiedDate = DateTime.Now }, transaction);
            }

            var share = await conn.QueryFirstOrDefaultAsync<Share>(@"SELECT * 
                                                                    FROM ""ToDoAssistant.Shares"" 
                                                                    WHERE ""ListId"" = @ListId AND ""UserId"" = @UserId",
                                                                    new { ListId = id, UserId = userId }, transaction);

            await conn.ExecuteAsync(@"DELETE FROM ""ToDoAssistant.Shares"" WHERE ""ListId"" = @ListId AND ""UserId"" = @UserId",
                new { ListId = id, UserId = userId }, transaction);

            await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Lists""
                                        SET ""Order"" = ""Order"" - 1
                                        WHERE ""UserId"" = @UserId AND ""IsArchived"" = FALSE AND ""Order"" > @Order",
                                      new { UserId = userId, share.Order }, transaction);

            await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Shares"" 
                                          SET ""Order"" = ""Order"" - 1 
                                          WHERE ""UserId"" = @UserId AND ""IsAccepted"" = TRUE AND ""IsArchived"" = FALSE AND ""Order"" > @Order",
                                      new { UserId = userId, share.Order }, transaction);


            // If the list is no longer shared make all private tasks public
            var sharesCount = await conn.ExecuteScalarAsync<short>(@"SELECT COUNT(*) FROM ""ToDoAssistant.Shares"" WHERE ""ListId"" = @ListId AND ""IsAccepted"" IS NOT FALSE;",
                                                                     new { ListId = id });
            if (sharesCount == 0)
            {
                var uncompletedPrivateTaskIds = await conn.QueryAsync<int>(@"SELECT ""Id"" FROM ""ToDoAssistant.Tasks"" 
                                                                            WHERE ""ListId"" = @ListId 
                                                                                AND ""IsCompleted"" = FALSE 
                                                                                AND ""PrivateToUserId"" IS NOT NULL",
                                                                            new { ListId = id });
                if (uncompletedPrivateTaskIds.Any())
                {
                    int orderOffset = uncompletedPrivateTaskIds.Count();
                    await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks"" SET ""Order"" = ""Order"" + @OrderOffset 
                                                WHERE ""ListId"" = @ListId AND ""IsCompleted"" = FALSE AND ""PrivateToUserId"" IS NULL",
                                              new { OrderOffset = orderOffset, ListId = id },
                                              transaction);
                }

                var completedPrivateTaskIds = await conn.QueryAsync<int>(@"SELECT ""Id"" FROM ""ToDoAssistant.Tasks"" 
                                                                            WHERE ""ListId"" = @ListId 
                                                                                AND ""IsCompleted"" 
                                                                                AND ""PrivateToUserId"" IS NOT NULL",
                                                                            new { ListId = id });
                if (completedPrivateTaskIds.Any())
                {
                    int orderOffset = completedPrivateTaskIds.Count();
                    await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks"" SET ""Order"" = ""Order"" + @OrderOffset 
                                            WHERE ""ListId"" = @ListId AND ""IsCompleted"" AND ""PrivateToUserId"" IS NULL",
                                              new { OrderOffset = orderOffset, ListId = id },
                                              transaction);
                }

                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks"" SET ""PrivateToUserId"" = NULL WHERE ""ListId"" = @ListId",
                                          new { ListId = id }, transaction);
            }

            transaction.Commit();

            return share;
        }

        public async Task<int> CopyAsync(ToDoList list)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            var tasks = await conn.QueryAsync<ToDoTask>(@"SELECT * FROM ""ToDoAssistant.Tasks""
                                                            WHERE ""ListId"" = @ListId AND (""PrivateToUserId"" IS NULL OR ""PrivateToUserId"" = @UserId)
                                                            ORDER BY ""PrivateToUserId"" NULLS LAST",
                new { ListId = list.Id, list.UserId }, transaction);

            var listsCount = await conn.ExecuteScalarAsync<short>(@"SELECT COUNT(*)
                                                                    FROM ""ToDoAssistant.Lists"" AS l
                                                                    LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                                    WHERE l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted"")",
                                                                    new { list.UserId }, transaction);
            list.Order = ++listsCount;
            int id = (await conn.QueryAsync<int>(@"INSERT INTO ""ToDoAssistant.Lists"" (""UserId"", ""Name"", ""Icon"", ""Order"", ""CreatedDate"", ""ModifiedDate"") 
                                                        VALUES (@UserId, @Name, @Icon, @Order, @CreatedDate, @ModifiedDate) returning ""Id""",
                                                        list, transaction)).Single();
            short order = 1;
            foreach (ToDoTask task in tasks)
            {
                task.ListId = id;
                task.PrivateToUserId = null;
                task.AssignedToUserId = null;
                task.Order = order;
                task.CreatedDate = task.ModifiedDate = list.CreatedDate;

                order++;
            }

            await conn.ExecuteAsync(@"INSERT INTO ""ToDoAssistant.Tasks"" (""ListId"", ""Name"", ""IsOneTime"", ""PrivateToUserId"", ""AssignedToUserId"", ""Order"", ""CreatedDate"", ""ModifiedDate"")
                                    VALUES (@ListId, @Name, @IsOneTime, @PrivateToUserId, @AssignedToUserId, @Order, @CreatedDate, @ModifiedDate)", tasks, transaction);

            transaction.Commit();

            return id;
        }

        public async Task SetIsArchivedAsync(int id, int userId, bool isArchived, DateTime modifiedDate)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            var list = await conn.QueryFirstOrDefaultAsync<ToDoList>(@"SELECT * FROM ""ToDoAssistant.Lists"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
                new { Id = id, UserId = userId });

            if (list != null)
            {
                if (isArchived)
                {
                    await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Lists""
                                                SET ""Order"" = ""Order"" - 1
                                                WHERE ""UserId"" = @UserId AND ""IsArchived"" = FALSE AND ""Order"" >= @Order",
                                              new { UserId = userId, list.Order }, transaction);

                    await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Shares""
                                                SET ""Order"" = ""Order"" - 1
                                                WHERE ""UserId"" = @UserId AND ""IsArchived"" = FALSE AND ""Order"" >= @Order",
                                              new { UserId = userId, list.Order }, transaction);

                    await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Lists"" SET ""IsArchived"" = TRUE, ""NotificationsEnabled"" = FALSE, ""Order"" = NULL, ""ModifiedDate"" = @ModifiedDate
                                            WHERE ""Id"" = @Id AND ""UserId"" = @UserId AND ""IsArchived"" = FALSE",
                                              new { Id = id, UserId = userId, ModifiedDate = modifiedDate }, transaction);
                }
                else
                {
                    var listsCount = await conn.ExecuteScalarAsync<short>(@"SELECT COUNT(*)
                                                                            FROM ""ToDoAssistant.Lists"" AS l
                                                                            LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                                            WHERE l.""IsArchived"" = FALSE AND l.""UserId"" = @UserId 
                                                                                OR (s.""UserId"" = @UserId AND s.""IsAccepted"" AND s.""IsArchived"" = FALSE)",
                                                                            new { UserId = userId }, transaction);

                    await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Lists"" SET ""IsArchived"" = FALSE, ""NotificationsEnabled"" = TRUE, ""Order"" = @Order, ""ModifiedDate"" = @ModifiedDate
                                            WHERE ""Id"" = @Id AND ""UserId"" = @UserId AND ""IsArchived"" = TRUE",
                                              new { Id = id, UserId = userId, Order = ++listsCount, ModifiedDate = modifiedDate }, transaction);
                }
            }
            else
            {
                var share = await conn.QueryFirstOrDefaultAsync<Share>(@"SELECT * FROM ""ToDoAssistant.Shares"" WHERE ""ListId"" = @ListId AND ""UserId"" = @UserId",
                    new { ListId = id, UserId = userId });

                if (isArchived)
                {
                    await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Lists""
                                                SET ""Order"" = ""Order"" - 1
                                                WHERE ""UserId"" = @UserId AND ""IsArchived"" = FALSE AND ""Order"" >= @Order",
                                              new { UserId = userId, share.Order }, transaction);

                    await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Shares""
                                                SET ""Order"" = ""Order"" - 1
                                                WHERE ""UserId"" = @UserId AND ""IsArchived"" = FALSE AND ""Order"" >= @Order",
                                              new { UserId = userId, share.Order }, transaction);

                    await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Shares"" SET ""IsArchived"" = TRUE, ""NotificationsEnabled"" = FALSE, ""Order"" = NULL, ""ModifiedDate"" = @ModifiedDate
                                                WHERE ""ListId"" = @ListId AND ""UserId"" = @UserId AND ""IsArchived"" = FALSE",
                                              new { ListId = id, UserId = userId, ModifiedDate = modifiedDate }, transaction);
                }
                else
                {
                    var listsCount = await conn.ExecuteScalarAsync<short>(@"SELECT COUNT(*)
                                                                            FROM ""ToDoAssistant.Lists"" AS l
                                                                            LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                                            WHERE l.""IsArchived"" = FALSE 
                                                                                AND l.""UserId"" = @UserId 
                                                                                OR (s.""UserId"" = @UserId AND s.""IsAccepted"" AND s.""IsArchived"" = FALSE)",
                                                                            new { UserId = userId }, transaction);

                    await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Shares"" SET ""IsArchived"" = FALSE, ""NotificationsEnabled"" = TRUE, ""Order"" = @Order, ""ModifiedDate"" = @ModifiedDate
                                                WHERE ""ListId"" = @ListId AND ""UserId"" = @UserId AND ""IsArchived""",
                                              new { ListId = id, UserId = userId, Order = ++listsCount, ModifiedDate = modifiedDate }, transaction);
                }
            }

            transaction.Commit();
        }

        public async Task<bool> SetTasksAsNotCompletedAsync(int id, int userId, DateTime modifiedDate)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            IEnumerable<ToDoTask> tasks = await conn.QueryAsync<ToDoTask>(@"SELECT * FROM ""ToDoAssistant.Tasks"" WHERE ""ListId"" = @ListId", new { ListId = id });
            short uncompletedTasksCount = (short)tasks.Where(x => !x.IsCompleted && !x.PrivateToUserId.HasValue).Count();
            short uncompletedPrivateTasksCount = (short)tasks.Where(x => !x.IsCompleted && x.PrivateToUserId.HasValue && x.PrivateToUserId.Value == userId).Count();

            // Public tasks
            List<ToDoTask> completedTasks = tasks.Where(x => x.IsCompleted && !x.PrivateToUserId.HasValue).ToList();
            foreach (var task in completedTasks)
            {
                task.IsCompleted = false;
                task.Order = ++uncompletedTasksCount;
                task.ModifiedDate = modifiedDate;
            }

            await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks"" SET ""IsCompleted"" = FALSE, ""Order"" = @Order, ""ModifiedDate"" = @ModifiedDate WHERE ""Id"" = @Id",
                completedTasks, transaction);

            // Private tasks
            List<ToDoTask> completedPrivateTasks = tasks.Where(x => x.IsCompleted && x.PrivateToUserId.HasValue && x.PrivateToUserId.Value == userId).ToList();
            foreach (var task in completedPrivateTasks)
            {
                task.IsCompleted = false;
                task.Order = ++uncompletedPrivateTasksCount;
                task.ModifiedDate = modifiedDate;
            }

            await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks"" SET ""IsCompleted"" = FALSE, ""Order"" = @Order, ""ModifiedDate"" = @ModifiedDate WHERE ""Id"" = @Id",
                completedPrivateTasks, transaction);

            transaction.Commit();

            return completedTasks.Any();
        }

        public async Task SetShareIsAcceptedAsync(int id, int userId, bool isAccepted, DateTime modifiedDate)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            short? order = null;
            if (isAccepted)
            {
                var listsCount = await conn.ExecuteScalarAsync<short>(@"SELECT COUNT(*)
                                                                    FROM ""ToDoAssistant.Lists"" AS l
                                                                    LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                                    WHERE l.""IsArchived"" = FALSE AND l.""UserId"" = @UserId 
                                                                        OR (s.""UserId"" = @UserId AND s.""IsAccepted"" AND s.""IsArchived"" = FALSE)",
                                                                        new { UserId = userId }, transaction);
                order = ++listsCount;
            }

            await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Shares"" SET ""IsAccepted"" = @IsAccepted, ""Order"" = @Order, ""ModifiedDate"" = @ModifiedDate
                                     WHERE ""ListId"" = @ListId AND ""UserId"" = @UserId AND ""IsAccepted"" IS NULL",
                                      new { ListId = id, UserId = userId, IsAccepted = isAccepted, Order = order, ModifiedDate = modifiedDate }, transaction);

            transaction.Commit();

            // If the list is no longer shared make all private tasks public
            if (!isAccepted)
            {
                var sharesCount = await conn.ExecuteScalarAsync<short>(@"SELECT COUNT(*) FROM ""ToDoAssistant.Shares"" WHERE ""ListId"" = @ListId AND ""IsAccepted"" IS NOT FALSE;",
                                                                         new { ListId = id });

                if (sharesCount == 0)
                {
                    await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks"" SET ""PrivateToUserId"" = NULL WHERE ""ListId"" = @ListId",
                                              new { ListId = id });
                }
            }
        }

        public async Task ReorderAsync(int id, int userId, short oldOrder, short newOrder, DateTime modifiedDate)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            var parameters = new
            {
                UserId = userId,
                OldOrder = oldOrder,
                NewOrder = newOrder
            };

            if (newOrder > oldOrder)
            {
                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Lists""
                                        SET ""Order"" = ""Order"" - 1
                                        WHERE ""UserId"" = @UserId AND ""IsArchived"" = FALSE AND ""Order"" >= @OldOrder AND ""Order"" <= @NewOrder",
                                          parameters, transaction);

                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Shares"" 
                                        SET ""Order"" = ""Order"" - 1 
                                        WHERE ""UserId"" = @UserId AND ""IsAccepted"" = TRUE AND ""IsArchived"" = FALSE AND ""Order"" >= @OldOrder AND ""Order"" <= @NewOrder",
                                          parameters, transaction);
            }
            else
            {
                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Lists""
                                        SET ""Order"" = ""Order"" + 1 
                                        WHERE ""UserId"" = @UserId AND ""IsArchived"" = FALSE AND ""Order"" <= @OldOrder AND ""Order"" >= @NewOrder",
                                          parameters, transaction);

                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Shares"" 
                                        SET ""Order"" = ""Order"" + 1 
                                        WHERE ""UserId"" = @UserId AND ""IsAccepted"" = TRUE AND ""IsArchived"" = FALSE AND ""Order"" <= @OldOrder AND ""Order"" >= @NewOrder",
                                          parameters, transaction);
            }

            var userIsOwner = await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                                FROM ""ToDoAssistant.Lists""
                                                                WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
                                                                new { Id = id, UserId = userId }, transaction);

            if (userIsOwner)
            {
                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Lists""
                                            SET ""Order"" = @NewOrder, ""ModifiedDate"" = @ModifiedDate
                                            WHERE ""Id"" = @Id",
                                          new { Id = id, @NewOrder = newOrder, ModifiedDate = modifiedDate }, transaction);
            }
            else
            {
                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Shares""
                                            SET ""Order"" = @NewOrder, ""ModifiedDate"" = @ModifiedDate
                                            WHERE ""ListId"" = @Id AND ""UserId"" = @UserId",
                                          new { Id = id, UserId = userId, @NewOrder = newOrder, ModifiedDate = modifiedDate }, transaction);
            }

            transaction.Commit();
        }
    }
}
