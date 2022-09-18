using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.ToDoAssistant.Tasks;
using Dapper;
using Domain.Entities.ToDoAssistant;

namespace Persistence.Repositories.ToDoAssistant;

public class TasksRepository : BaseRepository, ITasksRepository
{
    public TasksRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public ToDoTask Get(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<ToDoTask>(@"SELECT * FROM todo_tasks WHERE id = @Id", new { Id = id });
    }

    public ToDoTask Get(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<ToDoTask>(@"SELECT t.* 
                                                    FROM todo_tasks AS t
                                                    INNER JOIN todo_lists AS l ON t.list_id = l.id
                                                    LEFT JOIN todo_shares AS s ON l.id = s.list_id
                                                    WHERE t.id = @Id AND (l.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))",
            new { Id = id, UserId = userId });
    }

    public ToDoTask GetForUpdate(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        var task = conn.QueryFirstOrDefault<ToDoTask>(@"SELECT t.* 
                                                        FROM todo_tasks AS t
                                                        INNER JOIN todo_lists AS l ON t.list_id = l.id
                                                        LEFT JOIN todo_shares AS s ON l.id = s.list_id
                                                        WHERE t.id = @Id AND (l.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))",
            new { Id = id, UserId = userId });

        return task;
    }

    public List<string> GetRecipes(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<string>(@"SELECT r.name
                                    FROM cooking_ingredients AS i
                                    INNER JOIN cooking_ingredients_tasks AS it ON i.id = it.ingredient_id AND it.user_id = @UserId
                                    INNER JOIN cooking_recipes_ingredients AS ri ON i.id = ri.ingredient_id
                                    LEFT JOIN cooking_recipes AS r ON ri.recipe_id = r.id
                                    LEFT JOIN cooking_shares AS s ON ri.recipe_id = s.recipe_id
                                    WHERE it.task_id = @TaskId AND (r.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))
                                    ORDER BY r.name",
            new { TaskId = id, UserId = userId }).ToList();
    }

    public bool Exists(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM todo_tasks AS t
                                          INNER JOIN todo_lists AS l ON t.list_id = l.id
                                          LEFT JOIN todo_shares AS s ON l.id = s.list_id
                                          WHERE t.id = @Id AND (l.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))",
            new { Id = id, UserId = userId });
    }

    public bool Exists(string name, int listId, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) 
                                          FROM todo_tasks AS t
                                          INNER JOIN todo_lists AS l ON t.list_id = l.id
                                          LEFT JOIN todo_shares AS s ON l.id = s.list_id
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
                                          FROM todo_tasks AS t
                                          INNER JOIN todo_lists AS l ON t.list_id = l.id
                                          LEFT JOIN todo_shares AS s ON l.id = s.list_id
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
                                          FROM todo_tasks AS t
                                          INNER JOIN todo_lists AS l ON t.list_id = l.id
                                          WHERE t.id != @Id 
                                          AND UPPER(t.name) = UPPER(@Name) 
                                          AND t.list_id = @ListId AND l.user_id = @UserId
                                          AND (t.private_to_user_id IS NULL OR t.private_to_user_id = @UserId)",
            new { Id = id, Name = name, ListId = listId, UserId = userId });
    }

    public bool IsPrivate(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM todo_tasks WHERE id = @Id AND private_to_user_id = @UserId",
            new { Id = id, UserId = userId });
    }

    public int Count(int listId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<int>(@"SELECT COUNT(*) FROM todo_tasks WHERE list_id = @ListId", new { ListId = listId });
    }

    public async Task<int> CreateAsync(ToDoTask task, int userId)
    {
        IQueryable<ToDoTask> otherTasks;

        if (task.PrivateToUserId.HasValue)
        {
            otherTasks = PrivateTasks(task.ListId, userId).Where(x => !x.IsCompleted);
        }
        else
        {
            otherTasks = PublicTasks(task.ListId).Where(x => !x.IsCompleted);
        }

        foreach (ToDoTask otherTask in otherTasks)
        {
            otherTask.Order++;
        }

        EFContext.Tasks.Add(task);

        await EFContext.SaveChangesAsync();

        return task.Id;
    }

    public async Task<IEnumerable<ToDoTask>> BulkCreateAsync(IEnumerable<ToDoTask> tasks, bool tasksArePrivate, int userId)
    {
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

        return tasks;
    }

    public async Task UpdateAsync(ToDoTask task, int userId)
    {
        var existingTask = Get(task.Id);

        if (existingTask.ListId == task.ListId)
        {
            task.Order = existingTask.Order;

            // If the task was public and it was made private reduce the Order of the public tasks
            if (!existingTask.PrivateToUserId.HasValue && task.PrivateToUserId.HasValue)
            {
                var publicTasks = PublicTasks(existingTask.ListId).Where(x => x.IsCompleted == existingTask.IsCompleted && x.Order > existingTask.Order);
                foreach (ToDoTask publicTask in publicTasks)
                {
                    publicTask.Order--;
                }

                var tasksCount = GetPrivateTasksCount(task.ListId, existingTask.IsCompleted, userId);
                task.Order = ++tasksCount;
            }
            // If the task was private and it was made public reduce the Order of only the user's private tasks
            else if (existingTask.PrivateToUserId.HasValue && !task.PrivateToUserId.HasValue)
            {
                var privateTasks = PrivateTasks(existingTask.ListId, userId).Where(x => x.IsCompleted == existingTask.IsCompleted && x.Order > existingTask.Order);
                foreach (ToDoTask privateTask in privateTasks)
                {
                    privateTask.Order--;
                }

                var tasksCount = GetPublicTasksCount(task.ListId, existingTask.IsCompleted);
                task.Order = ++tasksCount;
            }
        }
        else
        {
            using IDbConnection conn = OpenConnection();

            var newListIsShared = conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                                             FROM todo_shares
                                                             WHERE list_id = @ListId AND is_accepted IS NOT FALSE",
                new { task.ListId });

            if (!newListIsShared)
            {
                task.PrivateToUserId = null;
                task.AssignedToUserId = null;
            }

            if (existingTask.PrivateToUserId.HasValue)
            {
                var privateTasks = PrivateTasks(existingTask.ListId, userId).Where(x => x.IsCompleted == existingTask.IsCompleted && x.Order > existingTask.Order);
                foreach (ToDoTask privateTask in privateTasks)
                {
                    privateTask.Order--;
                }

                short tasksCount;

                if (newListIsShared)
                {
                    // If the task was moved to a shared list calculate the Order by counting the user's private tasks
                    tasksCount = GetPrivateTasksCount(task.ListId, existingTask.IsCompleted, userId);
                }
                else
                {
                    // If the task was moved to a non-shared list calculate the Order by counting the public tasks
                    tasksCount = GetPublicTasksCount(task.ListId, existingTask.IsCompleted);
                }

                task.Order = ++tasksCount;
            }
            else
            {
                var publicTasks = PublicTasks(existingTask.ListId).Where(x => x.IsCompleted == existingTask.IsCompleted && x.Order > existingTask.Order);
                foreach (ToDoTask publicTask in publicTasks)
                {
                    publicTask.Order--;
                }

                var tasksCount = GetPublicTasksCount(task.ListId, existingTask.IsCompleted);
                task.Order = ++tasksCount;
            }
        }

        ToDoTask dbTask = EFContext.Tasks.Find(task.Id);
        dbTask.Name = task.Name;
        dbTask.ListId = task.ListId;
        dbTask.IsOneTime = task.IsOneTime;
        dbTask.IsHighPriority = task.IsHighPriority;
        dbTask.PrivateToUserId = task.PrivateToUserId;
        dbTask.AssignedToUserId = task.AssignedToUserId;
        dbTask.Order = task.Order;
        dbTask.ModifiedDate = task.ModifiedDate;

        await EFContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, int userId)
    {
        ToDoTask task = EFContext.Tasks.Find(id);
        EFContext.Tasks.Remove(task);

        IQueryable<ToDoTask> otherTasks;

        if (task.PrivateToUserId.HasValue)
        {
            // If the task was private reduce the Order of only the user's private tasks
            otherTasks = PrivateTasks(task.ListId, userId).Where(x => x.IsCompleted == task.IsCompleted && x.Order > task.Order);
        }
        else
        {
            otherTasks = PublicTasks(task.ListId).Where(x => x.IsCompleted == task.IsCompleted && x.Order > task.Order);
        }

        foreach (ToDoTask otherTask in otherTasks)
        {
            otherTask.Order--;
        }

        await EFContext.SaveChangesAsync();
    }

    public async Task CompleteAsync(int id, int userId)
    {
        ToDoTask task = EFContext.Tasks.Find(id);

        if (task.PrivateToUserId.HasValue)
        {
            var completedPrivateTasks = PrivateTasks(task.ListId, userId).Where(x => x.IsCompleted);
            foreach (ToDoTask privateTask in completedPrivateTasks)
            {
                privateTask.Order++;
            }

            task.IsCompleted = true;

            var privateTasks = PrivateTasks(task.ListId, userId).Where(x => !x.IsCompleted && x.Order > task.Order);
            foreach (ToDoTask privateTask in privateTasks)
            {
                privateTask.Order--;
            }
        }
        else
        {
            var completedPublicTasks = PublicTasks(task.ListId).Where(x => x.IsCompleted);
            foreach (ToDoTask publicTask in completedPublicTasks)
            {
                publicTask.Order++;
            }

            task.IsCompleted = true;

            var publicTasks = PublicTasks(task.ListId).Where(x => !x.IsCompleted && x.Order > task.Order);
            foreach (ToDoTask publicTask in publicTasks)
            {
                publicTask.Order--;
            }
        }

        task.Order = 1;

        await EFContext.SaveChangesAsync();
    }

    public async Task UncompleteAsync(int id, int userId)
    {
        ToDoTask task = EFContext.Tasks.Find(id);

        short newOrder;
        if (task.PrivateToUserId.HasValue)
        {
            var tasksCount = GetPrivateTasksCount(task.ListId, false, userId);
            newOrder = ++tasksCount;

            var completedPrivateTasks = PrivateTasks(task.ListId, userId).Where(x => x.IsCompleted && x.Order > task.Order);
            foreach (ToDoTask privateTask in completedPrivateTasks)
            {
                privateTask.Order--;
            }
        }
        else
        {
            var tasksCount = GetPublicTasksCount(task.ListId, false);
            newOrder = ++tasksCount;

            var completedPublicTasks = PublicTasks(task.ListId).Where(x => x.IsCompleted && x.Order > task.Order);
            foreach (ToDoTask publicTask in completedPublicTasks)
            {
                publicTask.Order--;
            }
        }

        task.IsCompleted = false;
        task.Order = newOrder;

        await EFContext.SaveChangesAsync();
    }

    public async Task ReorderAsync(int id, int userId, short oldOrder, short newOrder, DateTime modifiedDate)
    {
        ToDoTask task = Get(id);

        if (task.PrivateToUserId.HasValue)
        {
            // If the task was private reduce/increase the Order of only the user's private tasks
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

        ToDoTask dbTask = EFContext.Tasks.Find(id);
        dbTask.Order = newOrder;
        dbTask.ModifiedDate = modifiedDate;

        await EFContext.SaveChangesAsync();
    }

    private short GetPrivateTasksCount(int listId, bool isCompleted, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<short>(@"SELECT COUNT(*)
                                           FROM todo_tasks
                                           WHERE list_id = @ListId AND is_completed = @IsCompleted AND private_to_user_id = @UserId",
            new { ListId = listId, IsCompleted = isCompleted, UserId = userId });
    }

    private short GetPublicTasksCount(int listId, bool isCompleted)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<short>(@"SELECT COUNT(*)
                                           FROM todo_tasks
                                           WHERE list_id = @ListId AND is_completed = @IsCompleted AND private_to_user_id IS NULL",
            new { ListId = listId, IsCompleted = isCompleted });
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
