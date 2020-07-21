using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Persistence.Repositories.ToDoAssistant
{
    public class TasksRepository : BaseRepository, ITasksRepository
    {
        public TasksRepository(IOptions<DatabaseSettings> databaseSettings)
            : base(databaseSettings.Value.DefaultConnectionString) { }

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

            task.Recipes = (await conn.QueryAsync<string>(@"SELECT r.""Name""
                                                            FROM ""CookingAssistant.Ingredients"" AS i
                                                            INNER JOIN ""CookingAssistant.RecipesIngredients"" AS ri ON i.""Id"" = ri.""IngredientId""
                                                            LEFT JOIN ""CookingAssistant.Recipes"" AS r ON ri.""RecipeId"" = r.""Id""
                                                            WHERE i.""TaskId"" = @TaskId
                                                            ORDER BY r.""Name""",
                                                            new { TaskId = id })).ToList();
            return task;
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
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            if (task.PrivateToUserId.HasValue)
            {
                // If the task is private increase Order of only the user's private tasks
                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                              SET ""Order"" = ""Order"" + 1 
                                              WHERE ""ListId"" = @ListId AND ""IsCompleted"" = FALSE AND ""PrivateToUserId"" = @UserId",
                                          new { task.ListId, UserId = userId }, transaction);
            }
            else
            {
                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                              SET ""Order"" = ""Order"" + 1 
                                              WHERE ""ListId"" = @ListId AND ""IsCompleted"" = FALSE AND ""PrivateToUserId"" IS NULL",
                                          new { task.ListId }, transaction);
            }


            var id = (await conn.QueryAsync<int>(@"INSERT INTO ""ToDoAssistant.Tasks"" (""ListId"", ""Name"", ""IsOneTime"", ""PrivateToUserId"", ""Order"", ""CreatedDate"", ""ModifiedDate"")
                                                       VALUES (@ListId, @Name, @IsOneTime, @PrivateToUserId, @Order, @CreatedDate, @ModifiedDate) returning ""Id""",
                                                   task, transaction)).Single();

            transaction.Commit();

            return id;
        }

        public async Task<IEnumerable<ToDoTask>> BulkCreateAsync(IEnumerable<ToDoTask> tasks, bool tasksArePrivate, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            if (tasksArePrivate)
            {
                // If the tasks are private increase Order of only the user's private tasks
                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                              SET ""Order"" = ""Order"" + @OrderIncrement 
                                              WHERE ""ListId"" = @ListId AND ""IsCompleted"" = FALSE AND ""PrivateToUserId"" = @UserId",
                                          new { tasks.First().ListId, OrderIncrement = tasks.Count(), UserId = userId }, transaction);
            }
            else
            {
                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                              SET ""Order"" = ""Order"" + @OrderIncrement 
                                              WHERE ""ListId"" = @ListId AND ""IsCompleted"" = FALSE AND ""PrivateToUserId"" IS NULL",
                                          new { tasks.First().ListId, OrderIncrement = tasks.Count() }, transaction);
            }


            short order = 1;
            foreach (var task in tasks)
            {
                task.Order = order;
                task.Id = (await conn.QueryAsync<int>(@"INSERT INTO ""ToDoAssistant.Tasks"" (""ListId"", ""Name"", ""IsOneTime"", ""PrivateToUserId"", ""Order"", ""CreatedDate"", ""ModifiedDate"")
                                                        VALUES (@ListId, @Name, @IsOneTime, @PrivateToUserId, @Order, @CreatedDate, @ModifiedDate) returning ""Id""",
                                                        task, transaction)).Single();
                order++;
            }

            transaction.Commit();

            return tasks;
        }

        public async Task UpdateAsync(ToDoTask task, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            var existingTask = await conn.QueryFirstOrDefaultAsync<ToDoTask>(@"SELECT * FROM ""ToDoAssistant.Tasks"" WHERE ""Id"" = @Id", new { task.Id }, transaction);

            if (existingTask.ListId != task.ListId)
            {
                var newListIsShared = await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                                            FROM ""ToDoAssistant.Shares""
                                                                            WHERE ""ListId"" = @ListId AND ""IsAccepted"" IS NOT FALSE",
                                                                            new { task.ListId }, transaction);

                if (!newListIsShared)
                {
                    task.PrivateToUserId = null;
                    task.AssignedToUserId = null;
                }

                if (existingTask.PrivateToUserId.HasValue)
                {
                    // If the task was private only reduce the Order of the user's private tasks
                    await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                            SET ""Order"" = ""Order"" - 1
                                            WHERE ""ListId"" = @ListId AND ""IsCompleted"" = @IsCompleted AND ""Order"" > @Order AND ""PrivateToUserId"" = @UserId",
                                              new { existingTask.ListId, existingTask.IsCompleted, existingTask.Order, UserId = userId }, transaction);

                    if (newListIsShared)
                    {
                        // If the task was moved to a shared list calculate the Order by counting the user's private tasks
                        var tasksCount = await conn.ExecuteScalarAsync<short>(@"SELECT COUNT(*)
                                                                                FROM ""ToDoAssistant.Tasks""
                                                                                WHERE ""ListId"" = @ListId AND ""IsCompleted"" = @IsCompleted AND ""PrivateToUserId"" = @UserId",
                                                                                new { task.ListId, existingTask.IsCompleted, UserId = userId }, transaction);
                        task.Order = ++tasksCount;
                    }
                    else
                    {
                        // If the task was moved to a non-shared list calculate the Order by counting the public tasks
                        var tasksCount = await conn.ExecuteScalarAsync<short>(@"SELECT COUNT(*)
                                                                                FROM ""ToDoAssistant.Tasks""
                                                                                WHERE ""ListId"" = @ListId AND ""IsCompleted"" = @IsCompleted AND ""PrivateToUserId"" IS NULL",
                                                                                new { task.ListId, existingTask.IsCompleted }, transaction);
                        task.Order = ++tasksCount;
                    }
                }
                else
                {
                    await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                              SET ""Order"" = ""Order"" - 1
                                              WHERE ""ListId"" = @ListId AND ""IsCompleted"" = @IsCompleted AND ""Order"" > @Order AND ""PrivateToUserId"" IS NULL",
                                              new { existingTask.ListId, existingTask.IsCompleted, existingTask.Order }, transaction);
                }
            }
            else
            {
                task.Order = existingTask.Order;

                // If the task was public and it was made private reduce the Order of the public tasks
                if (!existingTask.PrivateToUserId.HasValue && task.PrivateToUserId.HasValue)
                {
                    await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                              SET ""Order"" = ""Order"" - 1
                                              WHERE ""ListId"" = @ListId AND ""IsCompleted"" = @IsCompleted AND ""Order"" > @Order AND ""PrivateToUserId"" IS NULL",
                                              new { existingTask.ListId, existingTask.IsCompleted, existingTask.Order }, transaction);

                    var tasksCount = await conn.ExecuteScalarAsync<short>(@"SELECT COUNT(*)
                                                                            FROM ""ToDoAssistant.Tasks""
                                                                            WHERE ""ListId"" = @ListId AND ""IsCompleted"" = @IsCompleted AND ""PrivateToUserId"" = @UserId",
                                                                            new { task.ListId, existingTask.IsCompleted, UserId = userId }, transaction);
                    task.Order = ++tasksCount;
                }
                // If the task was private and it was made public reduce the Order of only the user's private tasks
                else if (existingTask.PrivateToUserId.HasValue && !task.PrivateToUserId.HasValue)
                {
                    await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                              SET ""Order"" = ""Order"" - 1
                                              WHERE ""ListId"" = @ListId AND ""IsCompleted"" = @IsCompleted AND ""Order"" > @Order AND ""PrivateToUserId"" = @UserId",
                                              new { existingTask.ListId, existingTask.IsCompleted, existingTask.Order, UserId = userId }, transaction);

                    var tasksCount = await conn.ExecuteScalarAsync<short>(@"SELECT COUNT(*)
                                                                            FROM ""ToDoAssistant.Tasks""
                                                                            WHERE ""ListId"" = @ListId AND ""IsCompleted"" = @IsCompleted AND ""PrivateToUserId"" IS NULL",
                                                                            new { task.ListId, existingTask.IsCompleted }, transaction);
                    task.Order = ++tasksCount;
                }
            }

            await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks"" SET ""Name"" = @Name, ""ListId"" = @ListId, ""IsOneTime"" = @IsOneTime, 
                                      ""PrivateToUserId"" = @PrivateToUserId, ""AssignedToUserId"" = @AssignedToUserId, 
                                      ""Order"" = @Order, ""ModifiedDate"" = @ModifiedDate
                                      WHERE ""Id"" = @Id", task, transaction);

            transaction.Commit();
        }

        public async Task<ToDoTask> DeleteAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            var task = await conn.QueryFirstOrDefaultAsync<ToDoTask>(@"SELECT * FROM ""ToDoAssistant.Tasks"" WHERE ""Id"" = @Id", new { Id = id }, transaction);

            await conn.ExecuteAsync(@"UPDATE ""CookingAssistant.Ingredients""
                                      SET ""Name"" = @Name, ""ModifiedDate"" = @ModifiedDate
                                      WHERE ""TaskId"" = @Id",
                                      new { task.Id, task.Name, ModifiedDate = DateTime.Now }, transaction);

            await conn.ExecuteAsync(@"DELETE FROM ""ToDoAssistant.Tasks"" WHERE ""Id"" = @Id", new { Id = id }, transaction);

            if (task.PrivateToUserId.HasValue)
            {
                // If the task was private reduce the Order of only the user's private tasks
                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                          SET ""Order"" = ""Order"" - 1
                                          WHERE ""ListId"" = @ListId AND ""IsCompleted"" = @IsCompleted AND ""Order"" > @Order AND ""PrivateToUserId"" = @UserId",
                                          new { task.ListId, task.IsCompleted, task.Order, UserId = userId }, transaction);
            }
            else
            {
                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks""
                                          SET ""Order"" = ""Order"" - 1
                                          WHERE ""ListId"" = @ListId AND ""IsCompleted"" = @IsCompleted AND ""Order"" > @Order AND ""PrivateToUserId"" IS NULL",
                                          new { task.ListId, task.IsCompleted, task.Order }, transaction);
            }

            transaction.Commit();

            return task;
        }

        public async Task<ToDoTask> SetIsCompletedAsync(int id, bool isCompleted, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction(IsolationLevel.ReadCommitted);

            var task = await conn.QueryFirstOrDefaultAsync<ToDoTask>(@"SELECT * FROM ""ToDoAssistant.Tasks"" WHERE ""Id"" = @Id", new { Id = id }, transaction);

            if (isCompleted)
            {
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

                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks"" SET ""IsCompleted"" = @IsCompleted, ""Order"" = @Order WHERE ""Id"" = @Id",
                    new { Id = id, IsCompleted = isCompleted, Order = order }, transaction);

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
            }
            else
            {
                short order;
                if (task.PrivateToUserId.HasValue)
                {
                    // If the task was private calculate the Order from only the user's private tasks
                    var tasksCount = await conn.ExecuteScalarAsync<short>(@"SELECT COUNT(*)
                                                                            FROM ""ToDoAssistant.Tasks""
                                                                            WHERE ""ListId"" = @ListId AND ""IsCompleted"" = FALSE AND ""PrivateToUserId"" = @UserId",
                                                                            new { task.ListId, UserId = userId }, transaction);
                    order = ++tasksCount;
                }
                else
                {
                    var tasksCount = await conn.ExecuteScalarAsync<short>(@"SELECT COUNT(*)
                                                                            FROM ""ToDoAssistant.Tasks""
                                                                            WHERE ""ListId"" = @ListId AND ""IsCompleted"" = FALSE AND ""PrivateToUserId"" IS NULL",
                                                                            new { task.ListId }, transaction);
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

                await conn.ExecuteAsync(@"UPDATE ""ToDoAssistant.Tasks"" SET ""IsCompleted"" = @IsCompleted, ""Order"" = @Order WHERE ""Id"" = @Id",
                    new { Id = id, IsCompleted = isCompleted, Order = order }, transaction);
            }

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
    }
}
