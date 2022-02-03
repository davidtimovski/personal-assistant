using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Application.Contracts.ToDoAssistant.Tasks;
using Domain.Entities.ToDoAssistant;

namespace Persistence.Repositories.ToDoAssistant;

public class TasksRepository : BaseRepository, ITasksRepository
{
    public TasksRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public ToDoTask Get(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<ToDoTask>(@"SELECT * FROM ""ToDoAssistant.Tasks"" WHERE ""Id"" = @Id", new { Id = id });
    }

    public ToDoTask Get(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<ToDoTask>(@"SELECT t.* 
                                                        FROM ""ToDoAssistant.Tasks"" AS t
                                                        INNER JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                                                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                        WHERE t.""Id"" = @Id AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))",
            new { Id = id, UserId = userId });
    }

    public ToDoTask GetForUpdate(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        var task = conn.QueryFirstOrDefault<ToDoTask>(@"SELECT t.* 
                                                            FROM ""ToDoAssistant.Tasks"" AS t
                                                            INNER JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                                                            LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                            WHERE t.""Id"" = @Id AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))",
            new { Id = id, UserId = userId });

        return task;
    }

    public List<string> GetRecipes(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<string>(@"SELECT r.""Name""
                                        FROM ""CookingAssistant.Ingredients"" AS i
                                        INNER JOIN ""CookingAssistant.RecipesIngredients"" AS ri ON i.""Id"" = ri.""IngredientId""
                                        LEFT JOIN ""CookingAssistant.Recipes"" AS r ON ri.""RecipeId"" = r.""Id""
                                        LEFT JOIN ""CookingAssistant.Shares"" AS s ON ri.""RecipeId"" = s.""RecipeId""
                                        WHERE i.""TaskId"" = @TaskId AND (r.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))
                                        ORDER BY r.""Name""",
            new { TaskId = id, UserId = userId }).ToList();
    }

    public bool Exists(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                              FROM ""ToDoAssistant.Tasks"" AS t
                                              INNER JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                                              LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                              WHERE t.""Id"" = @Id AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))",
            new { Id = id, UserId = userId });
    }

    public bool Exists(string name, int listId, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) 
                                              FROM ""ToDoAssistant.Tasks"" AS t
                                              INNER JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                                              LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                              WHERE UPPER(t.""Name"") = UPPER(@Name) 
                                              AND t.""ListId"" = @ListId 
                                              AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))
                                              AND (t.""PrivateToUserId"" IS NULL OR t.""PrivateToUserId"" = @UserId)",
            new { Name = name, ListId = listId, UserId = userId });
    }

    public bool Exists(List<string> names, int listId, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) 
                                              FROM ""ToDoAssistant.Tasks"" AS t
                                              INNER JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                                              LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                              WHERE UPPER(t.""Name"") = ANY(@Names) 
                                              AND t.""ListId"" = @ListId 
                                              AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))
                                              AND (t.""PrivateToUserId"" IS NULL OR t.""PrivateToUserId"" = @UserId)",
            new { Names = names, ListId = listId, UserId = userId });
    }

    public bool Exists(int id, string name, int listId, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                              FROM ""ToDoAssistant.Tasks"" AS t
                                              INNER JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                                              WHERE t.""Id"" != @Id 
                                              AND UPPER(t.""Name"") = UPPER(@Name) 
                                              AND t.""ListId"" = @ListId AND l.""UserId"" = @UserId
                                              AND (t.""PrivateToUserId"" IS NULL OR t.""PrivateToUserId"" = @UserId)",
            new { Id = id, Name = name, ListId = listId, UserId = userId });
    }

    public bool IsPrivate(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM ""ToDoAssistant.Tasks"" WHERE ""Id"" = @Id AND ""PrivateToUserId"" = @UserId",
            new { Id = id, UserId = userId });
    }

    public int Count(int listId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<int>(@"SELECT COUNT(*) FROM ""ToDoAssistant.Tasks"" WHERE ""ListId"" = @ListId", new { ListId = listId });
    }

    public async Task<int> CreateAsync(ToDoTask task, int userId)
    {
        if (task.PrivateToUserId.HasValue)
        {
            var privateTasks = PrivateTasks(task.ListId, userId).Where(x => !x.IsCompleted);
            foreach (ToDoTask privateTask in privateTasks)
            {
                privateTask.Order += 1;
            }
        }
        else
        {
            var publicTasks = PublicTasks(task.ListId).Where(x => !x.IsCompleted);
            foreach (ToDoTask publicTask in publicTasks)
            {
                publicTask.Order += 1;
            }
        }

        EFContext.Tasks.Add(task);

        await EFContext.SaveChangesAsync();

        return task.Id;
    }

    public async Task<IEnumerable<ToDoTask>> BulkCreateAsync(IEnumerable<ToDoTask> tasks, bool tasksArePrivate, int userId)
    {
        int listId = tasks.First().ListId;

        if (tasksArePrivate)
        {
            var privateTasks = PrivateTasks(listId, userId).Where(x => !x.IsCompleted);
            foreach (ToDoTask privateTask in privateTasks)
            {
                privateTask.Order += (short)tasks.Count();
            }
        }
        else
        {
            var publicTasks = PublicTasks(listId).Where(x => !x.IsCompleted);
            foreach (ToDoTask publicTask in publicTasks)
            {
                publicTask.Order += (short)tasks.Count();
            }
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
                    publicTask.Order -= 1;
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
                    privateTask.Order -= 1;
                }

                var tasksCount = GetPublicTasksCount(task.ListId, existingTask.IsCompleted);
                task.Order = ++tasksCount;
            }
        }
        else
        {
            using IDbConnection conn = OpenConnection();

            var newListIsShared = conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                                                 FROM ""ToDoAssistant.Shares""
                                                                 WHERE ""ListId"" = @ListId AND ""IsAccepted"" IS NOT FALSE",
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
                    privateTask.Order -= 1;
                }

                if (newListIsShared)
                {
                    // If the task was moved to a shared list calculate the Order by counting the user's private tasks
                    var tasksCount = GetPrivateTasksCount(task.ListId, existingTask.IsCompleted, userId);
                    task.Order = ++tasksCount;
                }
                else
                {
                    // If the task was moved to a non-shared list calculate the Order by counting the public tasks
                    var tasksCount = GetPublicTasksCount(task.ListId, existingTask.IsCompleted);
                    task.Order = ++tasksCount;
                }
            }
            else
            {
                var publicTasks = PublicTasks(existingTask.ListId).Where(x => x.IsCompleted == existingTask.IsCompleted && x.Order > existingTask.Order);
                foreach (ToDoTask publicTask in publicTasks)
                {
                    publicTask.Order -= 1;
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
        ToDoTask task = Get(id);

        var ingredients = EFContext.Ingredients.Where(x => x.TaskId == task.Id);
        foreach (var ingredient in ingredients)
        {
            ingredient.Name = task.Name;
            ingredient.ModifiedDate = DateTime.UtcNow;
        }

        EFContext.Tasks.Remove(task);

        if (task.PrivateToUserId.HasValue)
        {
            // If the task was private reduce the Order of only the user's private tasks
            var privateTasks = PrivateTasks(task.ListId, userId).Where(x => x.IsCompleted == task.IsCompleted && x.Order > task.Order);
            foreach (ToDoTask privateTask in privateTasks)
            {
                privateTask.Order -= 1;
            }
        }
        else
        {
            var publicTasks = PublicTasks(task.ListId).Where(x => x.IsCompleted == task.IsCompleted && x.Order > task.Order);
            foreach (ToDoTask publicTask in publicTasks)
            {
                publicTask.Order -= 1;
            }
        }

        await EFContext.SaveChangesAsync();
    }

    public async Task CompleteAsync(int id, int userId)
    {
        ToDoTask task = Get(id);
            
        if (task.PrivateToUserId.HasValue)
        {
            // If the task was private increase the Order of only the user's completed private tasks
            var privateTasks = PrivateTasks(task.ListId, userId).Where(x => x.IsCompleted);
            foreach (ToDoTask privateTask in privateTasks)
            {
                privateTask.Order += 1;
            }
        }
        else
        {
            var publicTasks = PublicTasks(task.ListId).Where(x => x.IsCompleted);
            foreach (ToDoTask publicTask in publicTasks)
            {
                publicTask.Order += 1;
            }
        }

        ToDoTask dbTask = EFContext.Tasks.Find(id);
        dbTask.IsCompleted = true;
        dbTask.Order = 1;

        if (task.PrivateToUserId.HasValue)
        {
            // If the task was private reduce the Order of only the user's private tasks
            var privateTasks = PrivateTasks(task.ListId, userId).Where(x => !x.IsCompleted && x.Order > task.Order);
            foreach (ToDoTask privateTask in privateTasks)
            {
                privateTask.Order -= 1;
            }
        }
        else
        {
            var publicTasks = PublicTasks(task.ListId).Where(x => !x.IsCompleted && x.Order > task.Order);
            foreach (ToDoTask publicTask in publicTasks)
            {
                publicTask.Order -= 1;
            }
        }

        await EFContext.SaveChangesAsync();
    }

    public async Task UncompleteAsync(int id, int userId)
    {
        ToDoTask task = Get(id);

        short order;
        if (task.PrivateToUserId.HasValue)
        {
            // If the task was private calculate the Order from only the user's private tasks
            var tasksCount = GetPrivateTasksCount(task.ListId, false, userId);
            order = ++tasksCount;
        }
        else
        {
            var tasksCount = GetPublicTasksCount(task.ListId, false);
            order = ++tasksCount;
        }

        if (task.PrivateToUserId.HasValue)
        {
            // If the task was private reduce the Order of only the user's private completed tasks
            var privateTasks = PrivateTasks(task.ListId, userId).Where(x => x.IsCompleted && x.Order > task.Order);
            foreach (ToDoTask privateTask in privateTasks)
            {
                privateTask.Order -= 1;
            }
        }
        else
        {
            var publicTasks = PublicTasks(task.ListId).Where(x => x.IsCompleted && x.Order > task.Order);
            foreach (ToDoTask publicTask in publicTasks)
            {
                publicTask.Order -= 1;
            }
        }

        ToDoTask dbTask = EFContext.Tasks.Find(id);
        dbTask.IsCompleted = false;
        dbTask.Order = order;

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
                    privateTask.Order -= 1;
                }
            }
            else
            {
                var privateTasks = PrivateTasks(task.ListId, userId).Where(x => x.IsCompleted == task.IsCompleted && x.Order <= oldOrder && x.Order >= newOrder);
                foreach (ToDoTask privateTask in privateTasks)
                {
                    privateTask.Order += 1;
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
                    publicTask.Order -= 1;
                }
            }
            else
            {
                var publicTasks = PublicTasks(task.ListId).Where(x => x.IsCompleted == task.IsCompleted && x.Order <= oldOrder && x.Order >= newOrder);
                foreach (ToDoTask publicTask in publicTasks)
                {
                    publicTask.Order += 1;
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
                                               FROM ""ToDoAssistant.Tasks""
                                               WHERE ""ListId"" = @ListId AND ""IsCompleted"" = @IsCompleted AND ""PrivateToUserId"" = @UserId",
            new { ListId = listId, IsCompleted = isCompleted, UserId = userId });
    }

    private short GetPublicTasksCount(int listId, bool isCompleted)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<short>(@"SELECT COUNT(*)
                                               FROM ""ToDoAssistant.Tasks""
                                               WHERE ""ListId"" = @ListId AND ""IsCompleted"" = @IsCompleted AND ""PrivateToUserId"" IS NULL",
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