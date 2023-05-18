using System.Data;
using Dapper;
using Sentry;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Entities;
using User = Core.Application.Entities.User;

namespace ToDoAssistant.Persistence.Repositories;

public class ListsRepository : BaseRepository, IListsRepository
{
    public ListsRepository(ToDoAssistantContext efContext)
        : base(efContext) { }

    public IEnumerable<ToDoList> GetAllAsOptions(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(GetAllAsOptions)}");

        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT DISTINCT l.*, s.user_id, s.is_accepted
                               FROM todo.lists AS l
                               LEFT JOIN todo.shares AS s ON l.id = s.list_id
                               WHERE l.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted)
                               ORDER BY l.order";

        var result = conn.Query<ToDoList, ListShare, ToDoList>(query,
            (list, share) =>
            {
                if (share != null && share.IsAccepted != false)
                {
                    list.Shares.Add(share);
                }
                return list;
            }, new { UserId = userId }, null, true, "user_id");

        metric.Finish();

        return result;
    }

    public IEnumerable<int> GetNonArchivedSharedListIds(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<int>(@"SELECT id FROM todo.lists WHERE user_id = @UserId AND is_archived = FALSE
                                 UNION ALL
                                 SELECT list_id FROM todo.shares WHERE user_id = @UserId AND is_archived = FALSE AND is_accepted",
            new { UserId = userId });
    }

    public IEnumerable<ToDoList> GetAllWithTasksAndSharingDetails(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(GetAllWithTasksAndSharingDetails)}");

        using IDbConnection conn = OpenConnection();

        var lists = conn.Query<ToDoList>(@"SELECT l.*
                                           FROM todo.lists AS l
                                           LEFT JOIN todo.shares AS s ON l.id = s.list_id 
                                           AND s.user_id = @UserId
                                           AND s.is_accepted 
                                           WHERE (l.user_id = @UserId OR s.user_id = @UserId)",
            new { UserId = userId }).ToList();

        var listIds = lists.Select(x => x.Id).ToArray();
        var shares = conn.Query<ListShare>("SELECT * FROM todo.shares WHERE list_id = ANY(@ListIds)", new { ListIds = listIds }).ToList();

        const string tasksSql = @"SELECT t.*, u.id, u.name, u.image_uri
                                  FROM todo.tasks AS t
                                  LEFT JOIN users AS u ON t.assigned_to_user_id = u.id
                                  WHERE t.list_id = ANY(@ListIds)
                                  AND (t.private_to_user_id IS NULL OR t.private_to_user_id = @UserId)";

        var tasks = conn.Query<ToDoTask, User, ToDoTask>(tasksSql,
            (task, user) =>
            {
                task.AssignedToUser = user;
                return task;
            }, new { ListIds = listIds, UserId = userId }).ToList();


        foreach (var list in lists)
        {
            list.Tasks.AddRange(tasks.Where(x => x.ListId == list.Id));
            list.Shares.AddRange(shares.Where(x => x.ListId == list.Id));
        }

        metric.Finish();

        return lists;
    }

    public IEnumerable<User> GetMembersAsAssigneeOptions(int id, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(GetMembersAsAssigneeOptions)}");

        using IDbConnection conn = OpenConnection();

        var result = conn.Query<User>(@"SELECT DISTINCT u.id, u.name, u.image_uri
                                  FROM users AS u
                                  LEFT JOIN todo.lists AS l ON u.id = l.user_id
                                  LEFT JOIN todo.shares AS s ON u.id = s.user_id
                                  WHERE l.id = @ListId OR (s.list_id = @ListId AND s.is_accepted IS NOT FALSE)
                                  ORDER BY u.name", new { ListId = id });

        metric.Finish();

        return result;
    }

    public ToDoList Get(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<ToDoList>("SELECT * FROM todo.lists WHERE id = @Id", new { Id = id });
    }

    public ToDoList GetWithShares(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(GetWithShares)}");

        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT l.*, s.user_id, s.is_admin, s.is_accepted, 
                               s.order, s.notifications_enabled, s.is_archived
                               FROM todo.lists AS l
                               LEFT JOIN todo.shares AS s ON l.id = s.list_id
                               WHERE l.id = @Id AND (l.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))";

        var result = conn.Query<ToDoList, ListShare, ToDoList>(query,
            (list, share) =>
            {
                if (share != null)
                {
                    list.Shares.Add(share);
                }
                return list;
            }, new { Id = id, UserId = userId }, null, true, "user_id").First();

        metric.Finish();

        return result;
    }

    public ToDoList GetWithOwner(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(GetWithOwner)}");

        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT DISTINCT l.*, u.id, u.email, u.image_uri
                               FROM todo.lists AS l
                               LEFT JOIN todo.shares AS s ON l.id = s.list_id
                               INNER JOIN users AS u ON l.user_id = u.id
                               WHERE l.id = @Id AND (l.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))";

        var result = conn.Query<ToDoList, User, ToDoList>(query,
            (list, user) =>
            {
                list.User = user;
                return list;
            }, new { Id = id, UserId = userId }).FirstOrDefault();

        metric.Finish();

        return result;
    }

    public IEnumerable<ListShare> GetShares(int id, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(GetShares)}");

        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT s.*, u.id, u.email, u.name, u.image_uri
                               FROM todo.shares AS s
                               INNER JOIN users AS u ON s.user_id = u.id
                               WHERE s.list_id = @ListId AND s.is_accepted IS NOT FALSE
                               ORDER BY (CASE WHEN s.is_accepted THEN 1 ELSE 2 END) ASC, s.created_date";

        var result = conn.Query<ListShare, User, ListShare>(query,
            (share, user) =>
            {
                share.User = user;
                return share;
            }, new { ListId = id });

        metric.Finish();

        return result;
    }

    public IEnumerable<ListShare> GetShareRequests(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(GetShareRequests)}");

        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT s.*, l.name, u.name
                               FROM todo.shares AS s
                               INNER JOIN todo.lists AS l ON s.list_id = l.id
                               INNER JOIN users AS u ON l.user_id = u.id
                               WHERE s.user_id = @UserId
                               ORDER BY s.modified_date DESC";

        var result = conn.Query<ListShare, ToDoList, User, ListShare>(query,
            (share, list, user) =>
            {
                share.List = list;
                share.User = user;
                return share;
            }, new { UserId = userId }, null, true, "name,Name");

        metric.Finish();

        return result;
    }

    public int GetPendingShareRequestsCount(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM todo.shares WHERE user_id = @UserId AND is_accepted IS NULL",
            new { UserId = userId });
    }

    public bool CanShareWithUser(int shareWithId, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return !conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                           FROM todo.lists AS l
                                           INNER JOIN todo.shares AS s on l.id = s.list_id
                                           WHERE l.user_id = @UserId AND s.user_id = @ShareWithId AND s.is_accepted = FALSE",
            new { ShareWithId = shareWithId, UserId = userId });
    }

    public bool UserOwnsOrShares(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM todo.lists AS l
                                          LEFT JOIN todo.shares AS s ON l.id = s.list_id
                                          WHERE l.id = @Id AND (l.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))",
            new { Id = id, UserId = userId });
    }

    public bool UserOwnsOrSharesAsPending(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM todo.lists AS l
                                          LEFT JOIN todo.shares AS s ON l.id = s.list_id
                                          WHERE l.id = @Id AND (l.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted IS NOT FALSE))",
            new { Id = id, UserId = userId });
    }

    public bool UserOwnsOrSharesAsAdmin(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM todo.lists AS l
                                          LEFT JOIN todo.shares AS s ON l.id = s.list_id
                                          WHERE l.id = @Id AND (l.user_id = @UserId 
                                              OR (s.user_id = @UserId AND s.is_accepted AND s.is_admin = TRUE))",
            new { Id = id, UserId = userId });
    }

    public bool UserOwnsOrSharesAsAdmin(int id, string name, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM todo.lists AS l
                                          LEFT JOIN todo.shares AS s ON l.id = s.list_id
                                          WHERE l.id != @Id AND UPPER(l.name) = UPPER(@Name) 
                                              AND (l.user_id = @UserId 
                                              OR (s.user_id = @UserId AND s.is_accepted AND s.is_admin = TRUE))",
            new { Id = id, Name = name, UserId = userId });
    }

    public bool UserOwns(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM todo.lists
                                          WHERE id = @Id AND user_id = @UserId",
            new { Id = id, UserId = userId });
    }

    public bool IsShared(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM todo.shares
                                          WHERE list_id = @ListId AND is_accepted IS NOT FALSE",
            new { ListId = id });
    }

    public bool UserHasBlockedSharing(int listId, int userId, int sharedWithId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM todo.shares
                                          WHERE user_id = @SharedWithId AND is_accepted = FALSE AND (SELECT user_id FROM todo.lists WHERE id = @ListId) = @SharerId",
            new { SharedWithId = sharedWithId, SharerId = userId, ListId = listId });
    }

    public bool Exists(string name, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT SUM(total) FROM
                                          (
	                                          SELECT COUNT(*) AS total
	                                          FROM todo.lists
	                                          WHERE UPPER(name) = UPPER(@Name) AND user_id = @UserId
	                                          UNION ALL
	                                          SELECT COUNT(*) AS total
	                                          FROM todo.lists AS l
	                                          LEFT JOIN todo.shares AS s ON s.user_id = @UserId
	                                          WHERE UPPER(l.name) = UPPER(@Name) AND l.user_id = @UserId
                                          ) AS sumTotal",
            new { Name = name, UserId = userId });
    }

    public bool Exists(int id, string name, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT SUM(total) FROM
                                          (
	                                          SELECT COUNT(*) AS total
	                                          FROM todo.lists
	                                          WHERE id != @Id AND UPPER(name) = UPPER(@Name) AND user_id = @UserId
	                                          UNION ALL
	                                          SELECT COUNT(*) AS total
	                                          FROM todo.lists AS l
	                                          LEFT JOIN todo.shares AS s ON s.user_id = @UserId
	                                          WHERE id != @Id AND UPPER(l.name) = UPPER(@Name) AND l.user_id = @UserId
                                          ) AS sumTotal",
            new { Id = id, Name = name, UserId = userId });
    }

    public int Count(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM todo.lists WHERE user_id = @UserId", new { UserId = userId });
    }

    public IEnumerable<User> GetUsersToBeNotifiedOfChange(int id, int excludeUserId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(GetUsersToBeNotifiedOfChange)}");

        using IDbConnection conn = OpenConnection();

        var result = conn.Query<User>(@"SELECT u.*
                                  FROM users AS u
                                  INNER JOIN todo.shares AS s ON u.id = s.user_id
                                  WHERE u.id != @ExcludeUserId AND s.list_id = @ListId AND s.is_accepted AND u.todo_notifications_enabled AND s.notifications_enabled
                                  UNION
                                  SELECT u.*
                                  FROM users AS u
                                  INNER JOIN todo.lists AS l ON u.id = l.user_id
                                  WHERE u.id != @ExcludeUserId AND l.id = @ListId AND u.todo_notifications_enabled AND l.notifications_enabled",
            new { ListId = id, ExcludeUserId = excludeUserId });

        metric.Finish();

        return result;
    }

    public IEnumerable<User> GetUsersToBeNotifiedOfDeletion(int id, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(GetUsersToBeNotifiedOfDeletion)}");

        using IDbConnection conn = OpenConnection();

        var result = conn.Query<User>(@"SELECT u.*
                                  FROM users AS u
                                  INNER JOIN todo.shares AS s ON u.id = s.user_id
                                  WHERE s.list_id = @ListId AND s.is_accepted AND u.todo_notifications_enabled AND s.notifications_enabled",
            new { ListId = id });

        metric.Finish();

        return result;
    }

    public bool CheckIfUserCanBeNotifiedOfChange(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(CheckIfUserCanBeNotifiedOfChange)}");

        using IDbConnection conn = OpenConnection();

        var result = conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM users AS u
                                          INNER JOIN todo.shares AS s ON u.id = s.user_id
                                          WHERE u.id = @UserId AND s.list_id = @ListId AND s.is_accepted AND u.todo_notifications_enabled AND s.notifications_enabled",
            new { ListId = id, UserId = userId });

        metric.Finish();

        return result;
    }

    public async Task<int> CreateAsync(ToDoList list, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(CreateAsync)}");

        using IDbConnection conn = OpenConnection();

        var listsCount = conn.ExecuteScalar<short>(@"SELECT COUNT(*)
                                                     FROM todo.lists AS l
                                                     LEFT JOIN todo.shares AS s ON l.id = s.list_id
                                                     WHERE l.is_archived = FALSE AND l.user_id = @UserId 
                                                         OR (s.user_id = @UserId AND s.is_accepted AND s.is_archived = FALSE)",
            new { list.UserId });
        list.Order = ++listsCount;

        for (short i = 0; i < list.Tasks.Count; i++)
        {
            list.Tasks[i].ListId = list.Id;
            list.Tasks[i].Order = (short)(i + 1);
        }

        EFContext.Lists.Add(list);

        await EFContext.SaveChangesAsync();

        metric.Finish();

        return list.Id;
    }

    public async Task<ToDoList> UpdateAsync(ToDoList list, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(UpdateAsync)}");

        using IDbConnection conn = OpenConnection();

        var originalList = conn.QueryFirst<ToDoList>("SELECT * FROM todo.lists WHERE id = @Id", new { list.Id });

        ToDoList dbList = EFContext.Lists.Find(list.Id);
        dbList.Name = list.Name;
        dbList.Icon = list.Icon;
        dbList.IsOneTimeToggleDefault = list.IsOneTimeToggleDefault;
        dbList.ModifiedDate = list.ModifiedDate;

        if (dbList.UserId == userId)
        {
            dbList.NotificationsEnabled = list.NotificationsEnabled;
        }
        else
        {
            ListShare share = EFContext.ListShares.First(x => x.ListId == list.Id && x.UserId == userId);
            share.NotificationsEnabled = list.NotificationsEnabled;
            share.ModifiedDate = list.ModifiedDate;
        }

        await EFContext.SaveChangesAsync();

        metric.Finish();

        return originalList;
    }

    public async Task UpdateSharedAsync(ToDoList list, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(UpdateSharedAsync)}");

        ToDoList dbList = EFContext.Lists.First(x => x.Id == list.Id && x.UserId == list.UserId);
        dbList.NotificationsEnabled = list.NotificationsEnabled;
        dbList.ModifiedDate = list.ModifiedDate;

        await EFContext.SaveChangesAsync();

        metric.Finish();
    }

    public async Task<string> DeleteAsync(int id, ISpan metricsSpan)
    {
        var now = DateTime.UtcNow;

        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(DeleteAsync)}");

        ToDoList list = Get(id);

        using IDbConnection conn = OpenConnection();

        var affectedUsers = new List<(int, short?)> {
            (list.UserId, list.Order)
        };
        affectedUsers.AddRange(conn.Query<(int, short?)>(@"SELECT user_id, ""order""
                                                           FROM todo.shares
                                                           WHERE list_id = @Id AND is_accepted = TRUE", new { Id = id }));

        EFContext.Lists.Remove(list);

        foreach (var affectedUser in affectedUsers)
        {
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

        metric.Finish();

        return list.Name;
    }

    public async Task SaveSharingDetailsAsync(IEnumerable<ListShare> newShares, IEnumerable<ListShare> editedShares, IEnumerable<ListShare> removedShares, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(SaveSharingDetailsAsync)}");

        EFContext.ListShares.RemoveRange(removedShares);

        foreach (ListShare share in editedShares)
        {
            ListShare dbShare = EFContext.ListShares.First(x => x.ListId == share.ListId && x.UserId == share.UserId && x.IsAccepted != false);
            dbShare.IsAdmin = share.IsAdmin;
            dbShare.ModifiedDate = share.ModifiedDate;
        }

        EFContext.ListShares.AddRange(newShares);

        await EFContext.SaveChangesAsync();

        metric.Finish();
    }

    public async Task<ListShare> LeaveAsync(int id, int userId, ISpan metricsSpan)
    {
        var now = DateTime.UtcNow;

        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(LeaveAsync)}");

        using IDbConnection conn = OpenConnection();

        var share = conn.QueryFirst<ListShare>(@"SELECT * 
                                                 FROM todo.shares 
                                                 WHERE list_id = @ListId AND user_id = @UserId",
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

        var sharesCount = conn.ExecuteScalar<short>("SELECT COUNT(*) FROM todo.shares WHERE list_id = @ListId AND is_accepted IS NOT FALSE", new { ListId = id });
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

        metric.Finish();

        return share;
    }

    public async Task<int> CopyAsync(ToDoList list, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(CopyAsync)}");

        using IDbConnection conn = OpenConnection();

        list.Tasks = conn.Query<ToDoTask>(@"SELECT * FROM todo.tasks
                                            WHERE list_id = @ListId AND (private_to_user_id IS NULL OR private_to_user_id = @UserId)
                                            ORDER BY private_to_user_id NULLS LAST",
            new { ListId = list.Id, list.UserId }).ToList();

        var listsCount = conn.ExecuteScalar<short>(@"SELECT COUNT(*)
                                                     FROM todo.lists AS l
                                                     LEFT JOIN todo.shares AS s ON l.id = s.list_id
                                                     WHERE l.is_archived = FALSE AND l.user_id = @UserId 
                                                         OR (s.user_id = @UserId AND s.is_accepted AND s.is_archived = FALSE)",
            new { list.UserId });

        ToDoList dbList = conn.QueryFirst<ToDoList>("SELECT * FROM todo.lists WHERE id = @Id", new { list.Id });

        list.Id = default;
        list.Order = ++listsCount;
        list.NotificationsEnabled = dbList.NotificationsEnabled;
        list.IsOneTimeToggleDefault = dbList.IsOneTimeToggleDefault;

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

        metric.Finish();

        return list.Id;
    }

    public async Task SetIsArchivedAsync(int id, int userId, bool isArchived, DateTime modifiedDate, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(SetIsArchivedAsync)}");

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
                using IDbConnection conn = OpenConnection();

                var listsCount = conn.ExecuteScalar<short>(@"SELECT COUNT(*)
                                                             FROM todo.lists AS l
                                                             LEFT JOIN todo.shares AS s ON l.id = s.list_id
                                                             WHERE l.is_archived = FALSE AND l.user_id = @UserId 
                                                                 OR (s.user_id = @UserId AND s.is_accepted AND s.is_archived = FALSE)",
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
                using IDbConnection conn = OpenConnection();

                var listsCount = conn.ExecuteScalar<short>(@"SELECT COUNT(*)
                                                             FROM todo.lists AS l
                                                             LEFT JOIN todo.shares AS s ON l.id = s.list_id
                                                             WHERE l.is_archived = FALSE 
                                                                 AND l.user_id = @UserId 
                                                                 OR (s.user_id = @UserId AND s.is_accepted AND s.is_archived = FALSE)",
                    new { UserId = userId });

                share.IsArchived = false;
                share.NotificationsEnabled = true;
                share.Order = ++listsCount;
                share.ModifiedDate = modifiedDate;
            }
        }

        await EFContext.SaveChangesAsync();

        metric.Finish();
    }

    public async Task<bool> UncompleteAllAsync(int id, int userId, DateTime modifiedDate, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(UncompleteAllAsync)}");

        List<ToDoTask> tasks = EFContext.Tasks.Where(x => x.ListId == id).ToList();

        // Public tasks
        List<ToDoTask> completedTasks = tasks.Where(x => x.IsCompleted && !x.PrivateToUserId.HasValue).ToList();
        short uncompletedTasksCount = (short)tasks.Count(x => !x.IsCompleted && !x.PrivateToUserId.HasValue);
        foreach (var task in completedTasks)
        {
            task.IsCompleted = false;
            task.Order = ++uncompletedTasksCount;
            task.ModifiedDate = modifiedDate;
        }

        // Private tasks
        List<ToDoTask> completedPrivateTasks = tasks.Where(x => x.IsCompleted && x.PrivateToUserId.HasValue && x.PrivateToUserId.Value == userId).ToList();
        short uncompletedPrivateTasksCount = (short)tasks.Count(x => !x.IsCompleted && x.PrivateToUserId.HasValue && x.PrivateToUserId.Value == userId);
        foreach (var task in completedPrivateTasks)
        {
            task.IsCompleted = false;
            task.Order = ++uncompletedPrivateTasksCount;
            task.ModifiedDate = modifiedDate;
        }

        await EFContext.SaveChangesAsync();

        metric.Finish();

        return completedTasks.Any();
    }

    public async Task SetShareIsAcceptedAsync(int id, int userId, bool isAccepted, DateTime modifiedDate, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListsRepository)}.{nameof(SetShareIsAcceptedAsync)}");

        using IDbConnection conn = OpenConnection();

        short? order = null;
        if (isAccepted)
        {
            var listsCount = conn.ExecuteScalar<short>(@"SELECT COUNT(*)
                                                         FROM todo.lists AS l
                                                         LEFT JOIN todo.shares AS s ON l.id = s.list_id
                                                         WHERE l.is_archived = FALSE AND l.user_id = @UserId 
                                                            OR (s.user_id = @UserId AND s.is_accepted AND s.is_archived = FALSE)",
                new { UserId = userId });
            order = ++listsCount;
        }

        ListShare share = EFContext.ListShares.First(x => x.ListId == id && x.UserId == userId && x.IsAccepted != true);
        share.IsAccepted = isAccepted;
        share.Order = order;
        share.ModifiedDate = modifiedDate;

        if (!isAccepted)
        {
            var sharesCount = conn.ExecuteScalar<short>("SELECT COUNT(*) FROM todo.shares WHERE list_id = @ListId AND is_accepted IS NOT FALSE", new { ListId = id });

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

        metric.Finish();
    }

    public async Task ReorderAsync(int id, int userId, short oldOrder, short newOrder, DateTime modifiedDate)
    {
        if (newOrder > oldOrder)
        {
            var lists = NonArchivedLists(userId).Where(x => x.Order >= oldOrder && x.Order <= newOrder);
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

        using IDbConnection conn = OpenConnection();

        var userIsOwner = conn.ExecuteScalar<bool>("SELECT COUNT(*) FROM todo.lists WHERE id = @Id AND user_id = @UserId", new { Id = id, UserId = userId });

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
