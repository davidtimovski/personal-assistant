using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Persistence;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Persistence.Repositories.ToDoAssistant
{
    public class TasksRepository : BaseRepository, ITasksRepository
    {
        public TasksRepository(PersonalAssistantContext efContext)
            : base(efContext) { }

        public async Task<ToDoTask> GetAsync(int id)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryFirstOrDefaultAsync<ToDoTask>(@"SELECT * FROM ""ToDoAssistant.Tasks"" WHERE ""Id"" = @Id", new { Id = id });
        }

        public async Task<ToDoTask> GetAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryFirstOrDefaultAsync<ToDoTask>(@"SELECT t.* 
                                                                    FROM ""ToDoAssistant.Tasks"" AS t
                                                                    INNER JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                                                                    LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                                    WHERE t.""Id"" = @Id AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))",
                                                                    new { Id = id, UserId = userId });
        }

        public async Task<ToDoTask> GetForUpdateAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var task = await conn.QueryFirstOrDefaultAsync<ToDoTask>(@"SELECT t.* 
                                                                    FROM ""ToDoAssistant.Tasks"" AS t
                                                                    INNER JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                                                                    LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                                    WHERE t.""Id"" = @Id AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))",
                                                                    new { Id = id, UserId = userId });

            return task;
        }

        public async Task<List<string>> GetRecipesAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return (await conn.QueryAsync<string>(@"SELECT r.""Name""
                                                    FROM ""CookingAssistant.Ingredients"" AS i
                                                    INNER JOIN ""CookingAssistant.RecipesIngredients"" AS ri ON i.""Id"" = ri.""IngredientId""
                                                    LEFT JOIN ""CookingAssistant.Recipes"" AS r ON ri.""RecipeId"" = r.""Id""
                                                    LEFT JOIN ""CookingAssistant.Shares"" AS s ON ri.""RecipeId"" = s.""RecipeId""
                                                    WHERE i.""TaskId"" = @TaskId AND (r.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))
                                                    ORDER BY r.""Name""",
                                                    new { TaskId = id, UserId = userId })).ToList();
        }

        public async Task<bool> ExistsAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""ToDoAssistant.Tasks"" AS t
                                                        INNER JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                                                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                        WHERE t.""Id"" = @Id AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))",
                                                        new { Id = id, UserId = userId });
        }

        public async Task<bool> ExistsAsync(string name, int listId, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*) 
                                                        FROM ""ToDoAssistant.Tasks"" AS t
                                                        INNER JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                                                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                        WHERE UPPER(t.""Name"") = UPPER(@Name) 
                                                        AND t.""ListId"" = @ListId 
                                                        AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))
                                                        AND (t.""PrivateToUserId"" IS NULL OR t.""PrivateToUserId"" = @UserId)",
                                                        new { Name = name, ListId = listId, UserId = userId });
        }

        public async Task<bool> ExistsAsync(List<string> names, int listId, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*) 
                                                        FROM ""ToDoAssistant.Tasks"" AS t
                                                        INNER JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                                                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                                                        WHERE UPPER(t.""Name"") = ANY(@Names) 
                                                        AND t.""ListId"" = @ListId 
                                                        AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))
                                                        AND (t.""PrivateToUserId"" IS NULL OR t.""PrivateToUserId"" = @UserId)",
                                                        new { Names = names, ListId = listId, UserId = userId });
        }

        public async Task<bool> ExistsAsync(int id, string name, int listId, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                        FROM ""ToDoAssistant.Tasks"" AS t
                                                        INNER JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                                                        WHERE t.""Id"" != @Id 
                                                        AND UPPER(t.""Name"") = UPPER(@Name) 
                                                        AND t.""ListId"" = @ListId AND l.""UserId"" = @UserId
                                                        AND (t.""PrivateToUserId"" IS NULL OR t.""PrivateToUserId"" = @UserId)",
                new { Id = id, Name = name, ListId = listId, UserId = userId });
        }

        public async Task<bool> IsPrivateAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*) FROM ""ToDoAssistant.Tasks"" WHERE ""Id"" = @Id AND ""PrivateToUserId"" = @UserId",
                                                         new { Id = id, UserId = userId });
        }

        public async Task<int> CountAsync(int listId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<int>(@"SELECT COUNT(*) FROM ""ToDoAssistant.Tasks"" WHERE ""ListId"" = @ListId", new { ListId = listId });
        }

        public async Task<int> CreateAsync(ToDoTask task, int userId)
        {
            using PersonalAssistantContext db = EFContext;

            if (task.PrivateToUserId.HasValue)
            {
                var privateTasks = GetPrivateTasks(userId, db).Where(x => x.ListId == task.ListId && !x.IsCompleted);
                foreach (ToDoTask privateTask in privateTasks)
                {
                    privateTask.Order += 1;
                }
            }
            else
            {
                var publicTasks = GetPublicTasks(db).Where(x => x.ListId == task.ListId && !x.IsCompleted);
                foreach (ToDoTask dbTask in publicTasks)
                {
                    dbTask.Order += 1;
                }
            }

            db.Tasks.Add(task); 

            await db.SaveChangesAsync();

            return task.Id;
        }

        public async Task<IEnumerable<ToDoTask>> BulkCreateAsync(IEnumerable<ToDoTask> tasks, bool tasksArePrivate, int userId)
        {
            using PersonalAssistantContext db = EFContext;

            int listId = tasks.First().ListId;

            if (tasksArePrivate)
            {
                var privateTasks = GetPrivateTasks(userId, db).Where(x => x.ListId == listId && !x.IsCompleted);
                foreach (ToDoTask privateTask in privateTasks)
                {
                    privateTask.Order += (short)tasks.Count();
                }
            }
            else
            {
                var publicTasks = GetPublicTasks(db).Where(x => x.ListId == listId && !x.IsCompleted);
                foreach (ToDoTask dbTask in publicTasks)
                {
                    dbTask.Order += (short)tasks.Count();
                }
            }

            short order = 1;
            foreach (ToDoTask task in tasks)
            {
                task.Order = order;

                db.Tasks.Add(task);

                order++;
            }

            await db.SaveChangesAsync();

            return tasks;
        }

        public async Task UpdateAsync(ToDoTask task, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var existingTask = await conn.QueryFirstOrDefaultAsync<ToDoTask>(@"SELECT * FROM ""ToDoAssistant.Tasks"" WHERE ""Id"" = @Id", new { task.Id });

            using PersonalAssistantContext db = EFContext;

            if (existingTask.ListId != task.ListId)
            {
                var newListIsShared = await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
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
                    var privateTasks = GetPrivateTasks(userId, db).Where(x => x.ListId == existingTask.ListId && x.IsCompleted == existingTask.IsCompleted && x.Order > existingTask.Order);
                    foreach (ToDoTask privateTask in privateTasks)
                    {
                        privateTask.Order -= 1;
                    }

                    if (newListIsShared)
                    {
                        // If the task was moved to a shared list calculate the Order by counting the user's private tasks
                        var tasksCount = GetPrivateTasksCount(task.ListId, existingTask.IsCompleted, userId, conn);
                        task.Order = ++tasksCount;
                    }
                    else
                    {
                        // If the task was moved to a non-shared list calculate the Order by counting the public tasks
                        var tasksCount = GetPublicTasksCount(task.ListId, existingTask.IsCompleted, conn);
                        task.Order = ++tasksCount;
                    }
                }
                else
                {
                    var publicTasks = GetPublicTasks(db).Where(x => x.ListId == existingTask.ListId && x.IsCompleted == existingTask.IsCompleted && x.Order > existingTask.Order);
                    foreach (ToDoTask publicTask in publicTasks)
                    {
                        publicTask.Order -= 1;
                    }
                }
            }
            else
            {
                task.Order = existingTask.Order;

                // If the task was public and it was made private reduce the Order of the public tasks
                if (!existingTask.PrivateToUserId.HasValue && task.PrivateToUserId.HasValue)
                {
                    var publicTasks = GetPublicTasks(db).Where(x => x.ListId == existingTask.ListId && x.IsCompleted == existingTask.IsCompleted && x.Order > existingTask.Order);
                    foreach (ToDoTask publicTask in publicTasks)
                    {
                        publicTask.Order -= 1;
                    }

                    var tasksCount = GetPrivateTasksCount(task.ListId, existingTask.IsCompleted, userId, conn);
                    task.Order = ++tasksCount;
                }
                // If the task was private and it was made public reduce the Order of only the user's private tasks
                else if (existingTask.PrivateToUserId.HasValue && !task.PrivateToUserId.HasValue)
                {
                    var privateTasks = GetPrivateTasks(userId, db).Where(x => x.ListId == existingTask.ListId && x.IsCompleted == existingTask.IsCompleted && x.Order > existingTask.Order);
                    foreach (ToDoTask privateTask in privateTasks)
                    {
                        privateTask.Order -= 1;
                    }

                    var tasksCount = GetPublicTasksCount(task.ListId, existingTask.IsCompleted, conn);
                    task.Order = ++tasksCount;
                }
            }

            ToDoTask dbTask = await db.Tasks.FindAsync(task.Id);

            dbTask.Name = task.Name;
            dbTask.ListId = task.ListId;
            dbTask.IsOneTime = task.IsOneTime;
            dbTask.PrivateToUserId = task.PrivateToUserId;
            dbTask.AssignedToUserId = task.AssignedToUserId;
            dbTask.Order = task.Order;
            dbTask.ModifiedDate = task.ModifiedDate;

            await db.SaveChangesAsync();
        }

        public async Task<ToDoTask> DeleteAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var task = await conn.QueryFirstOrDefaultAsync<ToDoTask>(@"SELECT * FROM ""ToDoAssistant.Tasks"" WHERE ""Id"" = @Id", new { Id = id });

            using PersonalAssistantContext db = EFContext;

            var ingredients = db.Ingredients.Where(x => x.TaskId == task.Id);
            foreach (var ingredient in ingredients)
            {
                ingredient.Name = task.Name;
                ingredient.ModifiedDate = DateTime.UtcNow;
            }

            db.Tasks.Remove(task);

            if (task.PrivateToUserId.HasValue)
            {
                // If the task was private reduce the Order of only the user's private tasks
                var privateTasks = GetPrivateTasks(userId, db).Where(x => x.ListId == task.ListId && x.IsCompleted == task.IsCompleted && x.Order > task.Order);
                foreach (ToDoTask privateTask in privateTasks)
                {
                    privateTask.Order -= 1;
                }
            }
            else
            {
                var publicTasks = GetPublicTasks(db).Where(x => x.ListId == task.ListId && x.IsCompleted == task.IsCompleted && x.Order > task.Order);
                foreach (ToDoTask publicTask in publicTasks)
                {
                    publicTask.Order -= 1;
                }
            }

            await db.SaveChangesAsync();

            return task;
        }

        public async Task<ToDoTask> CompleteAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted);

            var task = await conn.QueryFirstOrDefaultAsync<ToDoTask>(@"SELECT * FROM ""ToDoAssistant.Tasks"" WHERE ""Id"" = @Id", new { Id = id }, transaction);

            short order = 1;
            if (task.PrivateToUserId.HasValue)
            {
                // If the task was private increase the Order of only the user's completed private tasks
                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                          SET ""Order"" = ""Order"" + 1 
                                          WHERE ""ListId"" = @ListId AND ""IsCompleted"" = TRUE AND ""PrivateToUserId"" = @UserId",
                                          new { task.ListId, UserId = userId }, transaction);
            }
            else
            {
                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                          SET ""Order"" = ""Order"" + 1 
                                          WHERE ""ListId"" = @ListId AND ""IsCompleted"" = TRUE AND ""PrivateToUserId"" IS NULL",
                                          new { task.ListId }, transaction);
            }

            await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks"" SET ""IsCompleted"" = TRUE, ""Order"" = @Order WHERE ""Id"" = @Id",
                new { Id = id, Order = order }, transaction);

            if (task.PrivateToUserId.HasValue)
            {
                // If the task was private reduce the Order of only the user's private tasks
                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                          SET ""Order"" = ""Order"" - 1
                                          WHERE ""ListId"" = @ListId AND ""IsCompleted"" = FALSE AND ""Order"" > @Order AND ""PrivateToUserId"" = @UserId",
                                          new { task.ListId, task.Order, UserId = userId }, transaction);
            }
            else
            {
                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                          SET ""Order"" = ""Order"" - 1
                                          WHERE ""ListId"" = @ListId AND ""IsCompleted"" = FALSE AND ""Order"" > @Order AND ""PrivateToUserId"" IS NULL",
                                          new { task.ListId, task.Order }, transaction);
            }

            transaction.Commit();

            return task;
        }

        public async Task<ToDoTask> UncompleteAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted);

            var task = await conn.QueryFirstOrDefaultAsync<ToDoTask>(@"SELECT * FROM ""ToDoAssistant.Tasks"" WHERE ""Id"" = @Id",
                new { Id = id }, transaction);

            short order;
            if (task.PrivateToUserId.HasValue)
            {
                // If the task was private calculate the Order from only the user's private tasks
                var tasksCount = GetPrivateTasksCount(task.ListId, false, userId, conn);
                order = ++tasksCount;
            }
            else
            {
                var tasksCount = GetPublicTasksCount(task.ListId, false, conn);
                order = ++tasksCount;
            }

            if (task.PrivateToUserId.HasValue)
            {
                // If the task was private reduce the Order of only the user's private completed tasks
                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                          SET ""Order"" = ""Order"" - 1
                                          WHERE ""ListId"" = @ListId AND ""IsCompleted"" = TRUE AND ""Order"" > @Order AND ""PrivateToUserId"" = @UserId",
                                          new { task.ListId, task.Order, UserId = userId }, transaction);
            }
            else
            {
                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                          SET ""Order"" = ""Order"" - 1
                                          WHERE ""ListId"" = @ListId AND ""IsCompleted"" = TRUE AND ""Order"" > @Order AND ""PrivateToUserId"" IS NULL",
                                          new { task.ListId, task.Order }, transaction);
            }

            await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks"" SET ""IsCompleted"" = FALSE, ""Order"" = @Order WHERE ""Id"" = @Id",
                new { Id = id, Order = order }, transaction);

            transaction.Commit();

            return task;
        }

        public async Task ReorderAsync(int id, int userId, short oldOrder, short newOrder, DateTime modifiedDate)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            var task = await conn.QueryFirstOrDefaultAsync<ToDoTask>(@"SELECT * FROM ""ToDoAssistant.Tasks"" WHERE ""Id"" = @Id", new { Id = id }, transaction);
            var parameters = new
            {
                task.ListId,
                task.IsCompleted,
                OldOrder = oldOrder,
                NewOrder = newOrder,
                UserId = userId
            };

            if (task.PrivateToUserId.HasValue)
            {
                // If the task was private reduce/increase the Order of only the user's private tasks
                if (newOrder > oldOrder)
                {
                    await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                              SET ""Order"" = ""Order"" - 1
                                              WHERE ""ListId"" = @ListId 
                                                AND ""IsCompleted"" = @IsCompleted 
                                                AND ""Order"" >= @OldOrder AND ""Order"" <= @NewOrder 
                                                AND ""PrivateToUserId"" = @UserId", parameters, transaction);
                }
                else
                {
                    await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                              SET ""Order"" = ""Order"" + 1 
                                              WHERE ""ListId"" = @ListId 
                                                AND ""IsCompleted"" = @IsCompleted 
                                                AND ""Order"" <= @OldOrder AND ""Order"" >= @NewOrder 
                                                AND ""PrivateToUserId"" = @UserId", parameters, transaction);
                }
            }
            else
            {
                if (newOrder > oldOrder)
                {
                    await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                              SET ""Order"" = ""Order"" - 1
                                              WHERE ""ListId"" = @ListId 
                                                AND ""IsCompleted"" = @IsCompleted 
                                                AND ""Order"" >= @OldOrder AND ""Order"" <= @NewOrder 
                                                AND ""PrivateToUserId"" IS NULL", parameters, transaction);
                }
                else
                {
                    await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                              SET ""Order"" = ""Order"" + 1 
                                              WHERE ""ListId"" = @ListId 
                                                AND ""IsCompleted"" = @IsCompleted 
                                                AND ""Order"" <= @OldOrder AND ""Order"" >= @NewOrder 
                                                AND ""PrivateToUserId"" IS NULL", parameters, transaction);
                }
            }

            await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                      SET ""Order"" = @NewOrder, ""ModifiedDate"" = @ModifiedDate
                                      WHERE ""Id"" = @Id",
                                      new { Id = id, @NewOrder = newOrder, ModifiedDate = modifiedDate }, transaction);

            transaction.Commit();
        }

        private short GetPrivateTasksCount(int listId, bool isCompleted, int userId, DbConnection conn)
        {
            return conn.ExecuteScalar<short>(@"SELECT COUNT(*)
                                               FROM ""ToDoAssistant.Tasks""
                                               WHERE ""ListId"" = @ListId AND ""IsCompleted"" = @IsCompleted AND ""PrivateToUserId"" = @UserId",
                                               new { ListId = listId, IsCompleted = isCompleted, UserId = userId });
        }

        private short GetPublicTasksCount(int listId, bool isCompleted, DbConnection conn)
        {
            return conn.ExecuteScalar<short>(@"SELECT COUNT(*)
                                               FROM ""ToDoAssistant.Tasks""
                                               WHERE ""ListId"" = @ListId AND ""IsCompleted"" = @IsCompleted AND ""PrivateToUserId"" IS NULL",
                                               new { ListId = listId, IsCompleted = isCompleted });
        }

        private IQueryable<ToDoTask> GetPrivateTasks(int userId, PersonalAssistantContext db)
        {
            return db.Tasks.Where(x => x.PrivateToUserId == userId);
        }

        private IQueryable<ToDoTask> GetPublicTasks(PersonalAssistantContext db)
        {
            return db.Tasks.Where(x => !x.PrivateToUserId.HasValue);
        }
    }
}
