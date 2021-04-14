using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Persistence;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;
using PersonalAssistant.Domain.Entities.Common;
using PersonalAssistant.Domain.Entities.CookingAssistant;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Persistence.Repositories.ToDoAssistant
{
    public class ListsRepository : BaseRepository, IListsRepository
    {
        public ListsRepository(PersonalAssistantContext efContext)
            : base(efContext) { }

        public async Task<IEnumerable<ToDoList>> GetAllAsOptionsAsync(int userId)
        {
            var sql = @"SELECT DISTINCT l.*, s.""UserId"", s.""IsAccepted""
                            FROM ""ToDoAssistant.Lists"" AS l
                            LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                            WHERE l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted"")
                            ORDER BY l.""Order""";

            return await Dapper.QueryAsync<ToDoList, ListShare, ToDoList>(sql,
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
            var lists = await Dapper.QueryAsync<ToDoList>(@"SELECT l.*
                                                          FROM ""ToDoAssistant.Lists"" AS l
                                                          LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId"" 
                                                            AND s.""UserId"" = @UserId
                                                            AND s.""IsAccepted"" 
                                                          WHERE (l.""UserId"" = @UserId OR s.""UserId"" = @UserId)",
                                                          new { UserId = userId });

            var listIds = lists.Select(x => x.Id).ToArray();
            var shares = await Dapper.QueryAsync<ListShare>(@"SELECT * FROM ""ToDoAssistant.Shares"" WHERE ""ListId"" = ANY(@ListIds)", new { ListIds = listIds });

            var tasksSql = @"SELECT t.*, u.""Id"", u.""ImageUri""
                             FROM ""ToDoAssistant.Tasks"" AS t
                             LEFT JOIN ""AspNetUsers"" AS u ON t.""AssignedToUserId"" = u.""Id""
                             WHERE t.""ListId"" = ANY(@ListIds)
                             AND (t.""PrivateToUserId"" IS NULL OR t.""PrivateToUserId"" = @UserId)";

            var tasks = await Dapper.QueryAsync<ToDoTask, User, ToDoTask>(tasksSql,
                (task, user) =>
                {
                    task.AssignedToUser = user;
                    return task;
                }, new { ListIds = listIds, UserId = userId }, null, true);


            foreach (var list in lists)
            {
                list.Tasks.AddRange(tasks.Where(x => x.ListId == list.Id));
                list.Shares.AddRange(shares.Where(x => x.ListId == list.Id));
            }

            return lists;
        }

        public async Task<IEnumerable<User>> GetMembersAsAssigneeOptionsAsync(int id)
        {
            return await Dapper.QueryAsync<User>(@"SELECT DISTINCT u.""Id"", u.""Name"", u.""ImageUri""
                                                FROM ""AspNetUsers"" AS u
                                                LEFT JOIN ""ToDoAssistant.Lists"" AS l ON u.""Id"" = l.""UserId""
                                                LEFT JOIN ""ToDoAssistant.Shares"" AS s ON u.""Id"" = s.""UserId""
                                                WHERE l.""Id"" = @ListId OR (s.""ListId"" = @ListId AND s.""IsAccepted"" IS NOT FALSE)
                                                ORDER BY u.""Name""", new { ListId = id });
        }

        public async Task<ToDoList> GetAsync(int id)
        {
            return await Dapper.QueryFirstOrDefaultAsync<ToDoList>(@"SELECT * FROM ""ToDoAssistant.Lists"" WHERE ""Id"" = @Id", new { Id = id });
        }

        public async Task<ToDoList> GetAsync(int id, int userId)
        {
            var sql = @"SELECT l.*, s.""UserId"", s.""IsAdmin"", s.""IsAccepted"", 
                            s.""Order"", s.""NotificationsEnabled"", s.""IsArchived""
                        FROM ""ToDoAssistant.Lists"" AS l
                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                        WHERE l.""Id"" = @Id AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))";

            return (await Dapper.QueryAsync<ToDoList, ListShare, ToDoList>(sql,
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
            var sql = @"SELECT DISTINCT l.*, users.""Id"", users.""Email"", users.""ImageUri""
                        FROM ""ToDoAssistant.Lists"" AS l
                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                        INNER JOIN ""AspNetUsers"" AS users ON l.""UserId"" = users.""Id""
                        WHERE l.""Id"" = @Id AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))";

            return (await Dapper.QueryAsync<ToDoList, User, ToDoList>(sql,
                (list, user) =>
                {
                    list.User = user;
                    return list;
                }, new { Id = id, UserId = userId }, null, true)).FirstOrDefault();
        }

        public async Task<IEnumerable<ListShare>> GetSharesAsync(int id)
        {
            var sql = @"SELECT s.*, u.""Id"", u.""Email"", u.""ImageUri""
                        FROM ""ToDoAssistant.Shares"" AS s
                        INNER JOIN ""AspNetUsers"" AS u ON s.""UserId"" = u.""Id""
                        WHERE s.""ListId"" = @ListId AND s.""IsAccepted"" IS NOT FALSE
                        ORDER BY (CASE WHEN s.""IsAccepted"" THEN 1 ELSE 2 END) ASC, s.""CreatedDate""";

            return await Dapper.QueryAsync<ListShare, User, ListShare>(sql,
                (share, user) =>
                {
                    share.User = user;
                    return share;
                }, new { ListId = id }, null, true);
        }

        public async Task<IEnumerable<ListShare>> GetShareRequestsAsync(int userId)
        {
            var sql = @"SELECT s.*, l.""Name"", u.""Name""
                        FROM ""ToDoAssistant.Shares"" AS s
                        INNER JOIN ""ToDoAssistant.Lists"" AS l ON s.""ListId"" = l.""Id""
                        INNER JOIN ""AspNetUsers"" AS u ON l.""UserId"" = u.""Id""
                        WHERE s.""UserId"" = @UserId
                        ORDER BY s.""ModifiedDate"" DESC";

            return await Dapper.QueryAsync<ListShare, ToDoList, User, ListShare>(sql,
                (share, list, user) =>
                {
                    share.List = list;
                    share.User = user;
                    return share;
                }, new { UserId = userId }, null, true, "Name");
        }

        public async Task<int> GetPendingShareRequestsCountAsync(int userId)
        {
            return await Dapper.ExecuteScalarAsync<int>(@"SELECT COUNT(*) FROM ""ToDoAssistant.Shares"" WHERE ""UserId"" = @UserId AND ""IsAccepted"" IS NULL",
                new { UserId = userId });
        }

        public async Task<bool> CanShareWithUserAsync(int shareWithId, int userId)
        {
            return !await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""ToDoAssistant.Lists"" AS l
                                                        INNER JOIN ""ToDoAssistant.Shares"" AS s on l.""Id"" = s.""ListId""
                                                        WHERE l.""UserId"" = @UserId AND s.""UserId"" = @ShareWithId AND s.""IsAccepted"" = FALSE",
                                                          new { ShareWithId = shareWithId, UserId = userId });
        }

        public async Task<bool> UserOwnsOrSharesAsync(int id, int userId)
        {
            return await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""ToDoAssistant.Lists"" AS l
                                                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                        WHERE l.""Id"" = @Id AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))",
                new { Id = id, UserId = userId });
        }

        public async Task<bool> UserOwnsOrSharesAsPendingAsync(int id, int userId)
        {
            return await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""ToDoAssistant.Lists"" AS l
                                                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                        WHERE l.""Id"" = @Id AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted"" IS NOT FALSE))",
                new { Id = id, UserId = userId });
        }

        public async Task<bool> UserOwnsOrSharesAsAdminAsync(int id, int userId)
        {
            return await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""ToDoAssistant.Lists"" AS l
                                                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                        WHERE l.""Id"" = @Id AND (l.""UserId"" = @UserId 
                                                            OR (s.""UserId"" = @UserId AND s.""IsAccepted"" AND s.""IsAdmin"" = TRUE))",
                new { Id = id, UserId = userId });
        }

        public async Task<bool> UserOwnsOrSharesAsAdminAsync(int id, string name, int userId)
        {
            return await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""ToDoAssistant.Lists"" AS l
                                                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                        WHERE l.""Id"" != @Id AND UPPER(l.""Name"") = UPPER(@Name) 
                                                            AND (l.""UserId"" = @UserId 
                                                            OR (s.""UserId"" = @UserId AND s.""IsAccepted"" AND s.""IsAdmin"" = TRUE))",
                new { Id = id, Name = name, UserId = userId });
        }

        public async Task<bool> UserOwnsAsync(int id, int userId)
        {
            return await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""ToDoAssistant.Lists""
                                                        WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
                new { Id = id, UserId = userId });
        }

        public async Task<bool> IsSharedAsync(int id)
        {
            return await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""ToDoAssistant.Shares""
                                                        WHERE ""ListId"" = @ListId AND ""IsAccepted"" IS NOT FALSE",
                new { ListId = id });
        }

        public async Task<bool> UserHasBlockedSharingAsync(int userId, int sharedWithId)
        {
            return await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""ToDoAssistant.Lists"" AS l
                                                        INNER JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                        WHERE l.""UserId"" = @UserId AND s.""UserId"" = @SharedWithId AND ""IsAccepted"" = FALSE",
                                                         new { UserId = userId, SharedWithId = sharedWithId });
        }

        public async Task<bool> ExistsAsync(string name, int userId)
        {
            return await Dapper.ExecuteScalarAsync<bool>(@"SELECT SUM(total) FROM
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
            return await Dapper.ExecuteScalarAsync<bool>(@"SELECT SUM(total) FROM
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
            return await Dapper.ExecuteScalarAsync<int>(@"SELECT COUNT(*) FROM ""ToDoAssistant.Lists"" WHERE ""UserId"" = @UserId", new { UserId = userId });
        }

        public async Task<int> CreateAsync(ToDoList list)
        {
            var listsCount = Dapper.ExecuteScalar<short>(@"SELECT COUNT(*)
                                                           FROM ""ToDoAssistant.Lists"" AS l
                                                           LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                           WHERE l.""IsArchived"" = FALSE AND l.""UserId"" = @UserId 
                                                               OR (s.""UserId"" = @UserId AND s.""IsAccepted"" AND s.""IsArchived"" = FALSE)",
                                                           new { list.UserId });
            list.Order = ++listsCount;

            for (short i = 0; i < list.Tasks.Count(); i++)
            {
                list.Tasks[i].ListId = list.Id;
                list.Tasks[i].Order = (short)(i + 1);
            }

            EFContext.Lists.Add(list);

            await EFContext.SaveChangesAsync();

            return list.Id;
        }

        public async Task<ToDoList> UpdateAsync(ToDoList list)
        {
            ToDoList originalList = Dapper.QueryFirst<ToDoList>(@"SELECT * FROM ""ToDoAssistant.Lists"" WHERE ""Id"" = @Id", new { Id = list.Id });

            ToDoList dbList = EFContext.Lists.Find(list.Id);
            dbList.Name = list.Name;
            dbList.Icon = list.Icon;
            dbList.IsOneTimeToggleDefault = list.IsOneTimeToggleDefault;
            dbList.NotificationsEnabled = list.NotificationsEnabled;
            dbList.ModifiedDate = list.ModifiedDate;

            await EFContext.SaveChangesAsync();

            return originalList;
        }

        public async Task UpdateSharedAsync(ToDoList list)
        {
            ToDoList dbList = EFContext.Lists.First(x => x.Id == list.Id && x.UserId == list.UserId);
            dbList.NotificationsEnabled = list.NotificationsEnabled;
            dbList.ModifiedDate = list.ModifiedDate;

            await EFContext.SaveChangesAsync();
        }

        public async Task<string> DeleteAsync(int id)
        {
            var now = DateTime.UtcNow;

            var tasksLinkedToIngredients = from task in EFContext.Tasks
                                           join ingredient in EFContext.Ingredients on task.Id equals ingredient.TaskId
                                           where task.ListId == id
                                           select task;

            foreach (ToDoTask task in tasksLinkedToIngredients)
            {
                Ingredient ingredient = EFContext.Ingredients.First(x => x.TaskId == task.Id);
                ingredient.Name = task.Name;
                ingredient.ModifiedDate = now;
            }

            ToDoList list = await GetAsync(id);

            var affectedUsers = new List<(int, short?)> {
                (list.UserId, list.Order)
            };
            affectedUsers.AddRange(Dapper.Query<(int, short?)>(@"SELECT ""UserId"", ""Order""
                                                                 FROM ""ToDoAssistant.Shares""
                                                                 WHERE ""ListId"" = @Id AND ""IsAccepted"" = TRUE", new { Id = id }));

            EFContext.Lists.Remove(list);

            foreach (var affectedUser in affectedUsers)
            {
                var parameters = new { UserId = affectedUser.Item1, Order = affectedUser.Item2 };

                var lists = EFContext.Lists.Where(x => x.UserId == affectedUser.Item1 && !x.IsArchived && x.Order > affectedUser.Item2);
                foreach (ToDoList dbList in lists)
                {
                    dbList.Order -= 1;
                }

                var shares = EFContext.ListShares.Where(x => x.UserId == affectedUser.Item1 && x.IsAccepted == true && !x.IsArchived && x.Order > affectedUser.Item2);
                foreach (ListShare share in shares)
                {
                    share.Order -= 1;
                }
            }

            await EFContext.SaveChangesAsync();

            return list.Name;
        }

        public async Task SaveSharingDetailsAsync(IEnumerable<ListShare> newShares, IEnumerable<ListShare> editedShares, IEnumerable<ListShare> removedShares)
        {
            EFContext.ListShares.RemoveRange(removedShares);

            foreach (ListShare share in editedShares)
            {
                ListShare dbShare = EFContext.ListShares.First(x => x.ListId == share.ListId && x.UserId == share.UserId && x.IsAccepted != false);
                dbShare.IsAdmin = share.IsAdmin;
                dbShare.ModifiedDate = share.ModifiedDate;
            }

            EFContext.ListShares.AddRange(newShares);

            await EFContext.SaveChangesAsync();
        }

        public async Task<ListShare> LeaveAsync(int id, int userId)
        {
            var now = DateTime.UtcNow;

            var tasksLinkedToIngredients = from task in EFContext.Tasks
                                           join ingredient in EFContext.Ingredients on task.Id equals ingredient.TaskId
                                           where task.ListId == id
                                           select task;

            foreach (ToDoTask task in tasksLinkedToIngredients)
            {
                Ingredient ingredient = EFContext.Ingredients.First(x => x.TaskId == task.Id);
                ingredient.Name = task.Name;
                ingredient.ModifiedDate = now;
            }

            var share = Dapper.QueryFirst<ListShare>(@"SELECT * 
                                                       FROM ""ToDoAssistant.Shares"" 
                                                       WHERE ""ListId"" = @ListId AND ""UserId"" = @UserId",
                                                       new { ListId = id, UserId = userId });

            EFContext.ListShares.Remove(share);

            var lists = NonArchivedLists(userId).Where(x => x.Order > share.Order);
            foreach (ToDoList dbList in lists)
            {
                dbList.Order -= 1;
            }

            var shares = AcceptedShares(userId).Where(x => !x.IsArchived && x.Order > share.Order);
            foreach (ListShare dbShare in shares)
            {
                dbShare.Order -= 1;
            }

            var sharesCount = Dapper.ExecuteScalar<short>(@"SELECT COUNT(*) FROM ""ToDoAssistant.Shares"" WHERE ""ListId"" = @ListId AND ""IsAccepted"" IS NOT FALSE", new { ListId = id });
            if (sharesCount == 1)
            {
                var uncompletedTasks = EFContext.Tasks.Where(x => x.ListId == id && !x.IsCompleted && !x.PrivateToUserId.HasValue);
                var uncompletedOrderOffset = (short)uncompletedTasks.Count();
                foreach (ToDoTask task in uncompletedTasks)
                {
                    task.Order += uncompletedOrderOffset;
                }

                var completedTasks = EFContext.Tasks.Where(x => x.ListId == id && x.IsCompleted && !x.PrivateToUserId.HasValue);
                var completedOrderOffset = (short)completedTasks.Count();
                foreach (ToDoTask task in completedTasks)
                {
                    task.Order += completedOrderOffset;
                }

                var userPrivateTasks = EFContext.Tasks.Where(x => x.ListId == id && x.PrivateToUserId == userId);
                foreach (ToDoTask task in userPrivateTasks)
                {
                    EFContext.Tasks.Remove(task);
                }

                var privateTasks = EFContext.Tasks.Where(x => x.ListId == id && x.PrivateToUserId.HasValue && x.PrivateToUserId != userId);
                foreach (ToDoTask task in privateTasks)
                {
                    task.PrivateToUserId = null;
                }
            }

            await EFContext.SaveChangesAsync();

            return share;
        }

        public async Task<int> CopyAsync(ToDoList list)
        {
            list.Tasks = (Dapper.Query<ToDoTask>(@"SELECT * FROM ""ToDoAssistant.Tasks""
                                                   WHERE ""ListId"" = @ListId AND (""PrivateToUserId"" IS NULL OR ""PrivateToUserId"" = @UserId)
                                                   ORDER BY ""PrivateToUserId"" NULLS LAST",
                                                   new { ListId = list.Id, list.UserId })).ToList();

            var listsCount = Dapper.ExecuteScalar<short>(@"SELECT COUNT(*)
                                                           FROM ""ToDoAssistant.Lists"" AS l
                                                           LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                           WHERE l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted"")",
                                                           new { list.UserId });
            list.Id = default;
            list.Order = ++listsCount;

            foreach (ToDoTask task in list.Tasks)
            {
                task.Id = default;
                task.ListId = default;
                task.PrivateToUserId = null;
                task.AssignedToUserId = null;
                task.CreatedDate = task.ModifiedDate = list.CreatedDate;
            }

            EFContext.Lists.Add(list);

            await EFContext.SaveChangesAsync();

            return list.Id;
        }

        public async Task SetIsArchivedAsync(int id, int userId, bool isArchived, DateTime modifiedDate)
        {
            ToDoList list = EFContext.Lists.FirstOrDefault(x => x.Id == id && x.UserId == userId);

            if (list != null)
            {
                if (isArchived)
                {
                    var lists = NonArchivedLists(userId).Where(x => x.Order >= list.Order);
                    foreach (ToDoList dbList in lists)
                    {
                        dbList.Order -= 1;
                    }

                    var shares = NonArchivedShares(userId).Where(x => x.Order >= list.Order);
                    foreach (ListShare dbShare in shares)
                    {
                        dbShare.Order -= 1;
                    }

                    list.IsArchived = true;
                    list.NotificationsEnabled = false;
                    list.Order = null;
                    list.ModifiedDate = modifiedDate;
                }
                else
                {
                    var listsCount = Dapper.ExecuteScalar<short>(@"SELECT COUNT(*)
                                                                   FROM ""ToDoAssistant.Lists"" AS l
                                                                   LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                                   WHERE l.""IsArchived"" = FALSE AND l.""UserId"" = @UserId 
                                                                       OR (s.""UserId"" = @UserId AND s.""IsAccepted"" AND s.""IsArchived"" = FALSE)",
                                                                   new { UserId = userId });

                    list.IsArchived = false;
                    list.NotificationsEnabled = true;
                    list.Order = ++listsCount;
                    list.ModifiedDate = modifiedDate;
                }
            }
            else
            {
                ListShare share = EFContext.ListShares.First(x => x.ListId == id && x.UserId == userId);

                if (isArchived)
                {
                    var lists = NonArchivedLists(userId).Where(x => x.Order >= share.Order);
                    foreach (ToDoList dbList in lists)
                    {
                        dbList.Order -= 1;
                    }

                    var shares = NonArchivedShares(userId).Where(x => x.Order >= share.Order);
                    foreach (ListShare dbShare in shares)
                    {
                        dbShare.Order -= 1;
                    }

                    share.IsArchived = true;
                    share.NotificationsEnabled = false;
                    share.Order = null;
                    share.ModifiedDate = modifiedDate;
                }
                else
                {
                    var listsCount = Dapper.ExecuteScalar<short>(@"SELECT COUNT(*)
                                                                   FROM ""ToDoAssistant.Lists"" AS l
                                                                   LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                                   WHERE l.""IsArchived"" = FALSE 
                                                                       AND l.""UserId"" = @UserId 
                                                                       OR (s.""UserId"" = @UserId AND s.""IsAccepted"" AND s.""IsArchived"" = FALSE)",
                                                                   new { UserId = userId });

                    share.IsArchived = false;
                    share.NotificationsEnabled = true;
                    share.Order = ++listsCount;
                    share.ModifiedDate = modifiedDate;
                }
            }

            await EFContext.SaveChangesAsync();
        }

        public async Task<bool> SetTasksAsNotCompletedAsync(int id, int userId, DateTime modifiedDate)
        {
            List<ToDoTask> tasks = EFContext.Tasks.Where(x => x.ListId == id).ToList();
            short uncompletedTasksCount = (short)tasks.Where(x => !x.IsCompleted && !x.PrivateToUserId.HasValue).Count();
            short uncompletedPrivateTasksCount = (short)tasks.Where(x => !x.IsCompleted && x.PrivateToUserId.HasValue && x.PrivateToUserId.Value == userId).Count();

            // Public tasks
            IEnumerable<ToDoTask> completedTasks = tasks.Where(x => x.IsCompleted && !x.PrivateToUserId.HasValue);
            foreach (var task in completedTasks)
            {
                task.IsCompleted = false;
                task.Order = ++uncompletedTasksCount;
                task.ModifiedDate = modifiedDate;
            }

            // Private tasks
            IEnumerable<ToDoTask> completedPrivateTasks = tasks.Where(x => x.IsCompleted && x.PrivateToUserId.HasValue && x.PrivateToUserId.Value == userId);
            foreach (var task in completedPrivateTasks)
            {
                task.IsCompleted = false;
                task.Order = ++uncompletedPrivateTasksCount;
                task.ModifiedDate = modifiedDate;
            }

            await EFContext.SaveChangesAsync();

            return completedTasks.Any();
        }

        public async Task SetShareIsAcceptedAsync(int id, int userId, bool isAccepted, DateTime modifiedDate)
        {
            short? order = null;
            if (isAccepted)
            {
                var listsCount = Dapper.ExecuteScalar<short>(@"SELECT COUNT(*)
                                                               FROM ""ToDoAssistant.Lists"" AS l
                                                               LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                               WHERE l.""IsArchived"" = FALSE AND l.""UserId"" = @UserId 
                                                                   OR (s.""UserId"" = @UserId AND s.""IsAccepted"" AND s.""IsArchived"" = FALSE)",
                                                                   new { UserId = userId });
                order = ++listsCount;
            }

            ListShare share = EFContext.ListShares.First(x => x.ListId == id && x.UserId == userId && !x.IsAccepted.HasValue);
            share.IsAccepted = isAccepted;
            share.Order = order;
            share.ModifiedDate = modifiedDate;

            if (!isAccepted)
            {
                var sharesCount = Dapper.ExecuteScalar<short>(@"SELECT COUNT(*) FROM ""ToDoAssistant.Shares"" WHERE ""ListId"" = @ListId AND ""IsAccepted"" IS NOT FALSE", new { ListId = id });

                // If this is the last share make all private tasks public
                if (sharesCount == 1)
                {
                    var privateTasks = EFContext.Tasks.Where(x => x.ListId == id && x.PrivateToUserId.HasValue);
                    foreach (ToDoTask task in privateTasks)
                    {
                        task.PrivateToUserId = null;
                    }
                }
            }

            await EFContext.SaveChangesAsync();
        }

        public async Task ReorderAsync(int id, int userId, short oldOrder, short newOrder, DateTime modifiedDate)
        {
            if (newOrder > oldOrder)
            {
                var lists = NonArchivedLists(userId).Where(x => x.Order <= newOrder);
                foreach (ToDoList dbList in lists)
                {
                    dbList.Order -= 1;
                }

                var shares = AcceptedShares(userId).Where(x => !x.IsArchived && x.Order >= oldOrder && x.Order <= newOrder);
                foreach (ListShare dbShare in shares)
                {
                    dbShare.Order -= 1;
                }
            }
            else
            {
                var lists = NonArchivedLists(userId).Where(x => x.Order <= oldOrder && x.Order >= newOrder);
                foreach (ToDoList dbList in lists)
                {
                    dbList.Order += 1;
                }

                var shares = AcceptedShares(userId).Where(x => !x.IsArchived && x.Order <= oldOrder && x.Order >= newOrder);
                foreach (ListShare dbShare in shares)
                {
                    dbShare.Order += 1;
                }
            }

            var userIsOwner = Dapper.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM ""ToDoAssistant.Lists"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId", new { Id = id, UserId = userId });

            if (userIsOwner)
            {
                ToDoList list = EFContext.Lists.Find(id);
                list.Order = newOrder;
                list.ModifiedDate = modifiedDate;
            }
            else
            {
                ListShare share = EFContext.ListShares.First(x => x.ListId == id && x.UserId == userId);
                share.Order = newOrder;
                share.ModifiedDate = modifiedDate;
            }

            await EFContext.SaveChangesAsync();
        }

        private IQueryable<ToDoList> NonArchivedLists(int userId)
        {
            return EFContext.Lists.Where(x => x.UserId == userId && !x.IsArchived);
        }

        private IQueryable<ListShare> NonArchivedShares(int userId)
        {
            return EFContext.ListShares.Where(x => x.UserId == userId && !x.IsArchived);
        }

        private IQueryable<ListShare> AcceptedShares(int userId)
        {
            return EFContext.ListShares.Where(x => x.UserId == userId && x.IsAccepted == true);
        }
    }
}
