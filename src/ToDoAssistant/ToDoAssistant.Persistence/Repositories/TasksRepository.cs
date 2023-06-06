using System.Data;
using Dapper;
using Sentry;
using ToDoAssistant.Application.Contracts.Tasks;
using ToDoAssistant.Application.Entities;

namespace ToDoAssistant.Persistence.Repositories;

public class TasksRepository : BaseRepository, ITasksRepository
{
    public TasksRepository(ToDoAssistantContext efContext)
        : base(efContext) { }

    public ToDoTask Get(int id)
    {
        using IDbConnection conn = OpenConnection();

        return GetById(id, conn);
    }

    public ToDoTask? Get(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<ToDoTask>(@"SELECT t.* 
                                                    FROM todo.tasks AS t
                                                    INNER JOIN todo.lists AS l ON t.list_id = l.id
                                                    LEFT JOIN todo.shares AS s ON l.id = s.list_id
                                                    WHERE t.id = @Id AND (l.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))",
            new { Id = id, UserId = userId });
    }

    public ToDoTask GetForUpdate(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        var task = conn.QueryFirstOrDefault<ToDoTask>(@"SELECT t.* 
                                                        FROM todo.tasks AS t
                                                        INNER JOIN todo.lists AS l ON t.list_id = l.id
                                                        LEFT JOIN todo.shares AS s ON l.id = s.list_id
                                                        WHERE t.id = @Id AND (l.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))",
            new { Id = id, UserId = userId });

        return task;
    }

    public List<string> GetRecipes(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<string>(@"SELECT r.name
                                    FROM cooking.ingredients AS i
                                    INNER JOIN cooking.ingredients_tasks AS it ON i.id = it.ingredient_id AND it.user_id = @UserId
                                    INNER JOIN cooking.recipes_ingredients AS ri ON i.id = ri.ingredient_id
                                    LEFT JOIN cooking.recipes AS r ON ri.recipe_id = r.id
                                    LEFT JOIN cooking.shares AS s ON ri.recipe_id = s.recipe_id
                                    WHERE it.task_id = @TaskId AND (r.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))
                                    ORDER BY r.name",
            new { TaskId = id, UserId = userId }).ToList();
    }

    public bool Exists(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM todo.tasks AS t
                                          INNER JOIN todo.lists AS l ON t.list_id = l.id
                                          LEFT JOIN todo.shares AS s ON l.id = s.list_id
                                          WHERE t.id = @Id AND (l.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))",
            new { Id = id, UserId = userId });
    }

    public bool Exists(string name, int listId, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) 
                                          FROM todo.tasks AS t
                                          INNER JOIN todo.lists AS l ON t.list_id = l.id
                                          LEFT JOIN todo.shares AS s ON l.id = s.list_id
                                          WHERE UPPER(t.name) = UPPER(@Name) 
                                          AND t.list_id = @ListId 
                                          AND (l.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))
                                          AND (t.private_to_user_id IS NULL OR t.private_to_user_id = @UserId)",
            new { Name = name, ListId = listId, UserId = userId });
    }

    public bool Exists(List<string> names, int listId, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) 
                                          FROM todo.tasks AS t
                                          INNER JOIN todo.lists AS l ON t.list_id = l.id
                                          LEFT JOIN todo.shares AS s ON l.id = s.list_id
                                          WHERE UPPER(t.name) = ANY(@Names) 
                                          AND t.list_id = @ListId 
                                          AND (l.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))
                                          AND (t.private_to_user_id IS NULL OR t.private_to_user_id = @UserId)",
            new { Names = names, ListId = listId, UserId = userId });
    }

    public bool Exists(int id, string name, int listId, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM todo.tasks AS t
                                          INNER JOIN todo.lists AS l ON t.list_id = l.id
                                          WHERE t.id != @Id 
                                          AND UPPER(t.name) = UPPER(@Name) 
                                          AND t.list_id = @ListId AND l.user_id = @UserId
                                          AND (t.private_to_user_id IS NULL OR t.private_to_user_id = @UserId)",
            new { Id = id, Name = name, ListId = listId, UserId = userId });
    }

    public bool IsPrivate(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>("SELECT COUNT(*) FROM todo.tasks WHERE id = @Id AND private_to_user_id = @UserId",
            new { Id = id, UserId = userId });
    }

    public int Count(int listId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM todo.tasks WHERE list_id = @ListId", new { ListId = listId });
    }

    public async Task<int> CreateAsync(ToDoTask task, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(TasksRepository)}.{nameof(CreateAsync)}");

        using IDbConnection conn = OpenConnection();
        var transaction = conn.BeginTransaction();

        IEnumerable<int> incrementOrderTaskIds;
        if (task.PrivateToUserId.HasValue)
        {
            incrementOrderTaskIds = GetPrivateTaskIds(task.ListId, userId, false, null, conn);
        }
        else
        {
            incrementOrderTaskIds = GetPublicTaskIds(task.ListId, false, null, conn);
        }

        await conn.ExecuteAsync(@"UPDATE todo.tasks SET ""order"" = ""order"" + 1 WHERE id = ANY(@Ids)",
            new { Ids = incrementOrderTaskIds.ToList() },
            transaction);

        var id = await conn.ExecuteScalarAsync<int>(@"INSERT INTO todo.tasks(
	        list_id, name, url, is_completed, is_one_time, is_high_priority, private_to_user_id, assigned_to_user_id, ""order"", created_date, modified_date)
	        VALUES (@ListId, @Name, @Url, @IsCompleted, @IsOneTime, @IsHighPriority, @PrivateToUserId, @AssignedToUserId, @Order, @CreatedDate, @ModifiedDate) RETURNING id",
            task,
            transaction);

        transaction.Commit();

        metric.Finish();

        return id;
    }

    public async Task<IEnumerable<ToDoTask>> BulkCreateAsync(IEnumerable<ToDoTask> tasks, bool tasksArePrivate, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(TasksRepository)}.{nameof(BulkCreateAsync)}");

        int listId = tasks.First().ListId;
        IQueryable<ToDoTask> otherTasks;

        if (tasksArePrivate)
        {
            otherTasks = PrivateTasks(listId, userId).Where(x => !x.IsCompleted);
        }
        else
        {
            otherTasks = PublicTasks(listId).Where(x => !x.IsCompleted);
        }

        foreach (ToDoTask otherTask in otherTasks)
        {
            otherTask.Order += (short)tasks.Count();
        }

        short order = 1;
        foreach (ToDoTask task in tasks)
        {
            task.Order = order;

            EFContext.Tasks.Add(task);

            order++;
        }

        await EFContext.SaveChangesAsync();

        metric.Finish();

        return tasks;
    }

    public async Task UpdateAsync(ToDoTask task, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(TasksRepository)}.{nameof(UpdateAsync)}");

        using IDbConnection conn = OpenConnection();
        var transaction = conn.BeginTransaction();

        var existingTask = GetById(task.Id, conn);

        IEnumerable<int>? decrementOrderTaskIds = null;
        var orderChanged = false;
        if (existingTask.ListId == task.ListId)
        {
            // If the task was public and it was made private reduce the Order of the public tasks
            if (!existingTask.PrivateToUserId.HasValue && task.PrivateToUserId.HasValue)
            {
                decrementOrderTaskIds = GetPublicTaskIds(existingTask.ListId, existingTask.IsCompleted, existingTask.Order, conn);
                orderChanged = true;
            }
            // If the task was private and it was made public reduce the Order of the user's private tasks
            else if (existingTask.PrivateToUserId.HasValue && !task.PrivateToUserId.HasValue)
            {
                decrementOrderTaskIds = GetPrivateTaskIds(existingTask.ListId, userId, existingTask.IsCompleted, existingTask.Order, conn);
                orderChanged = true;
            }
        }
        else
        {
            orderChanged = true;

            var newListIsShared = conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                                             FROM todo.shares
                                                             WHERE list_id = @ListId AND is_accepted IS NOT FALSE",
                new { task.ListId });

            if (!newListIsShared)
            {
                task.PrivateToUserId = null;
                task.AssignedToUserId = null;
            }

            // Reduce Order of private or public tasks in old list
            if (existingTask.PrivateToUserId.HasValue)
            {
                decrementOrderTaskIds = GetPrivateTaskIds(existingTask.ListId, userId, existingTask.IsCompleted, existingTask.Order, conn);
            }
            else
            {
                decrementOrderTaskIds = GetPublicTaskIds(existingTask.ListId, existingTask.IsCompleted, existingTask.Order, conn);
            }
        }

        if (orderChanged)
        {
            short tasksCount;
            if (task.PrivateToUserId.HasValue)
            {
                tasksCount = GetPrivateTasksCount(task.ListId, userId, existingTask.IsCompleted, conn);
            }
            else
            {
                tasksCount = GetPublicTasksCount(task.ListId, existingTask.IsCompleted, conn);
            }
            task.Order = ++tasksCount;
        }
        else
        {
            task.Order = existingTask.Order;
        }

        if (decrementOrderTaskIds != null)
        {
            await conn.ExecuteAsync(@"UPDATE todo.tasks SET ""order"" = ""order"" - 1 WHERE id = ANY(@Ids)",
               new { Ids = decrementOrderTaskIds.ToList() },
               transaction);
        }
    
        await conn.ExecuteAsync(@"UPDATE todo.tasks
	        SET list_id = @ListId, name = @Name, url = @Url, is_one_time = @IsOneTime, is_high_priority = @IsHighPriority,
                private_to_user_id = @PrivateToUserId, assigned_to_user_id = @AssignedToUserId, ""order"" = @Order, modified_date = @ModifiedDate
	        WHERE id = @Id", task, transaction);

        transaction.Commit();

        metric.Finish();
    }

    public async Task DeleteAsync(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(TasksRepository)}.{nameof(DeleteAsync)}");

        using IDbConnection conn = OpenConnection();
        var transaction = conn.BeginTransaction();

        var task = GetById(id, conn);

        IEnumerable<int> decrementOrderTaskIds;
        if (task.PrivateToUserId.HasValue)
        {
            decrementOrderTaskIds = GetPrivateTaskIds(task.ListId, userId, task.IsCompleted, task.Order, conn);
        }
        else
        {
            decrementOrderTaskIds = GetPublicTaskIds(task.ListId, task.IsCompleted, task.Order, conn);
        }

        await conn.ExecuteAsync(@"UPDATE todo.tasks SET ""order"" = ""order"" - 1 WHERE id = ANY(@Ids)",
            new { Ids = decrementOrderTaskIds.ToList() },
            transaction);

        await conn.ExecuteAsync("DELETE FROM todo.tasks WHERE id = @Id", new { Id = id }, transaction);

        transaction.Commit();

        metric.Finish();
    }

    public async Task CompleteAsync(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(TasksRepository)}.{nameof(CompleteAsync)}");

        using IDbConnection conn = OpenConnection();
        var transaction = conn.BeginTransaction();

        var task = GetById(id, conn);

        IEnumerable<int> incrementOrderTaskIds;
        IEnumerable<int> decrementOrderTaskIds;
        if (task.PrivateToUserId.HasValue)
        {
            incrementOrderTaskIds = GetPrivateTaskIds(task.ListId, userId, true, null, conn);
            decrementOrderTaskIds = GetPrivateTaskIds(task.ListId, userId, false, task.Order, conn);
        }
        else
        {
            incrementOrderTaskIds = GetPublicTaskIds(task.ListId, true, null, conn);
            decrementOrderTaskIds = GetPublicTaskIds(task.ListId, false, task.Order, conn);
        }

        await conn.ExecuteAsync(@"UPDATE todo.tasks SET ""order"" = ""order"" + 1 WHERE id = ANY(@Ids)",
            new { Ids = incrementOrderTaskIds.ToList() },
            transaction);

        await conn.ExecuteAsync(@"UPDATE todo.tasks SET ""order"" = ""order"" - 1 WHERE id = ANY(@Ids)",
            new { Ids = decrementOrderTaskIds.ToList() },
            transaction);

        await conn.ExecuteAsync(@"UPDATE todo.tasks SET is_completed = TRUE, ""order"" = 1, modified_date = @ModifiedDate WHERE id = @Id",
            new { Id = id, ModifiedDate = DateTime.UtcNow },
            transaction);

        transaction.Commit();

        metric.Finish();
    }

    public async Task UncompleteAsync(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(TasksRepository)}.{nameof(UncompleteAsync)}");

        using IDbConnection conn = OpenConnection();
        var transaction = conn.BeginTransaction();

        var task = GetById(id, conn);

        short newOrder;
        IEnumerable<int> decrementOrderTaskIds;
        if (task.PrivateToUserId.HasValue)
        {
            var tasksCount = GetPrivateTasksCount(task.ListId, userId, false, conn);
            newOrder = ++tasksCount;

            decrementOrderTaskIds = GetPrivateTaskIds(task.ListId, userId, true, task.Order, conn);
        }
        else
        {
            var tasksCount = GetPublicTasksCount(task.ListId, false, conn);
            newOrder = ++tasksCount;

            decrementOrderTaskIds = GetPublicTaskIds(task.ListId, true, task.Order, conn);
        }

        await conn.ExecuteAsync(@"UPDATE todo.tasks SET ""order"" = ""order"" - 1 WHERE id = ANY(@Ids)",
            new { Ids = decrementOrderTaskIds.ToList() },
            transaction);

        await conn.ExecuteAsync(@"UPDATE todo.tasks SET is_completed = FALSE, ""order"" = @Order, modified_date = @ModifiedDate WHERE id = @Id",
            new { Id = id, Order = newOrder, ModifiedDate = DateTime.UtcNow },
            transaction);

        transaction.Commit();

        metric.Finish();
    }

    public async Task ReorderAsync(int id, int userId, short oldOrder, short newOrder, DateTime modifiedDate)
    {
        ToDoTask task = Get(id);

        if (task.PrivateToUserId.HasValue)
        {
            // If the task was private reduce/increase the Order of the user's private tasks
            if (newOrder > oldOrder)
            {
                var privateTasks = PrivateTasks(task.ListId, userId).Where(x => x.IsCompleted == task.IsCompleted && x.Order >= oldOrder && x.Order <= newOrder);
                foreach (ToDoTask privateTask in privateTasks)
                {
                    privateTask.Order--;
                }
            }
            else
            {
                var privateTasks = PrivateTasks(task.ListId, userId).Where(x => x.IsCompleted == task.IsCompleted && x.Order <= oldOrder && x.Order >= newOrder);
                foreach (ToDoTask privateTask in privateTasks)
                {
                    privateTask.Order++;
                }
            }
        }
        else
        {
            if (newOrder > oldOrder)
            {
                var publicTasks = PublicTasks(task.ListId).Where(x => x.IsCompleted == task.IsCompleted && x.Order >= oldOrder && x.Order <= newOrder);
                foreach (ToDoTask publicTask in publicTasks)
                {
                    publicTask.Order--;
                }
            }
            else
            {
                var publicTasks = PublicTasks(task.ListId).Where(x => x.IsCompleted == task.IsCompleted && x.Order <= oldOrder && x.Order >= newOrder);
                foreach (ToDoTask publicTask in publicTasks)
                {
                    publicTask.Order++;
                }
            }
        }

        ToDoTask dbTask = EFContext.Tasks.First(x => x.Id == id);
        dbTask.Order = newOrder;
        dbTask.ModifiedDate = modifiedDate;

        await EFContext.SaveChangesAsync();
    }

    private static ToDoTask GetById(int id, IDbConnection conn)
    {
        return conn.QueryFirstOrDefault<ToDoTask>("SELECT * FROM todo.tasks WHERE id = @Id", new { Id = id });
    }

    private static short GetPrivateTasksCount(int listId, int userId, bool isCompleted, IDbConnection conn)
    {
        return conn.ExecuteScalar<short>(@"SELECT COUNT(*)
                                           FROM todo.tasks
                                           WHERE list_id = @ListId AND is_completed = @IsCompleted AND private_to_user_id = @UserId",
            new { ListId = listId, IsCompleted = isCompleted, UserId = userId });
    }

    private static short GetPublicTasksCount(int listId, bool isCompleted, IDbConnection conn)
    {
        return conn.ExecuteScalar<short>(@"SELECT COUNT(*)
                                           FROM todo.tasks
                                           WHERE list_id = @ListId AND is_completed = @IsCompleted AND private_to_user_id IS NULL",
            new { ListId = listId, IsCompleted = isCompleted });
    }

    private static IEnumerable<int> GetPrivateTaskIds(int listId, int userId, bool isCompleted, short? order, IDbConnection conn)
    {
        var query = "SELECT id FROM todo.tasks WHERE list_id = @ListId AND private_to_user_id = @UserId AND is_completed = @IsCompleted";
        if (order.HasValue)
        {
            query += @" AND ""order"" > @Order";
        }

        return conn.Query<int>(query, new { ListId = listId, UserId = userId, IsCompleted = isCompleted, Order = order });
    }

    private static IEnumerable<int> GetPublicTaskIds(int listId, bool isCompleted, short? order, IDbConnection conn)
    {
        var query = "SELECT id FROM todo.tasks WHERE list_id = @ListId AND private_to_user_id IS NULL AND is_completed = @IsCompleted";
        if (order.HasValue)
        {
            query += @" AND ""order"" > @Order";
        }

        return conn.Query<int>(query, new { ListId = listId, IsCompleted = isCompleted, Order = order });
    }

    private IQueryable<ToDoTask> PrivateTasks(int listId, int userId)
    {
        return EFContext.Tasks.Where(x => x.ListId == listId && x.PrivateToUserId == userId);
    }

    private IQueryable<ToDoTask> PublicTasks(int listId)
    {
        return EFContext.Tasks.Where(x => x.ListId == listId && !x.PrivateToUserId.HasValue);
    }
}
