﻿using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Persistence.Repositories.Common
{
    public class UsersRepository : BaseRepository, IUsersRepository
    {
        public UsersRepository(IOptions<DatabaseSettings> databaseSettings)
            : base(databaseSettings.Value.DefaultConnectionString) { }

        public async Task<User> GetAsync(int id)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryFirstOrDefaultAsync<User>(@"SELECT * FROM ""AspNetUsers"" WHERE ""Id"" = @Id", new { Id = id });
        }

        public async Task<User> GetAsync(string email)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryFirstOrDefaultAsync<User>(@"SELECT * FROM ""AspNetUsers""
                                                                WHERE ""Email"" = @Email AND ""EmailConfirmed"" = TRUE",
                new { Email = email });
        }

        public async Task<bool> ExistsAsync(int id)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*) FROM ""AspNetUsers"" WHERE ""Id"" = @id",
                new { Id = id });
        }

        public async Task<string> GetLanguageAsync(int id)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryFirstOrDefaultAsync<string>(@"SELECT ""Language"" FROM ""AspNetUsers"" WHERE ""Id"" = @Id", new { Id = id });
        }

        public async Task<string> GetImageUriAsync(int id)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryFirstOrDefaultAsync<string>(@"SELECT ""ImageUri"" FROM ""AspNetUsers"" WHERE ""Id"" = @Id", new { Id = id });
        }

        public async Task<IEnumerable<User>> GetToBeNotifiedOfListChangeAsync(int listId, int excludeUserId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryAsync<User>(@"SELECT u.*
                                                FROM ""AspNetUsers"" AS u
                                                INNER JOIN ""ToDoAssistant.Shares"" AS s ON u.""Id"" = s.""UserId""
                                                WHERE u.""Id"" != @ExcludeUserId AND s.""ListId"" = @ListId AND s.""IsAccepted"" AND u.""ToDoNotificationsEnabled"" AND s.""NotificationsEnabled""
                                                UNION
                                                SELECT u.*
                                                FROM ""AspNetUsers"" AS u
                                                INNER JOIN ""ToDoAssistant.Lists"" AS l ON u.""Id"" = l.""UserId""
                                                WHERE u.""Id"" != @ExcludeUserId AND l.""Id"" = @ListId AND u.""ToDoNotificationsEnabled"" AND l.""NotificationsEnabled""",
                                            new { ListId = listId, ExcludeUserId = excludeUserId });
        }

        public async Task<bool> CheckIfUserCanBeNotifiedOfListChangeAsync(int listId, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                             FROM ""AspNetUsers"" AS u
                                                             INNER JOIN ""ToDoAssistant.Shares"" AS s ON u.""Id"" = s.""UserId""
                                                             WHERE u.""Id"" = @UserId AND s.""ListId"" = @ListId AND s.""IsAccepted"" AND u.""ToDoNotificationsEnabled"" AND s.""NotificationsEnabled""",
                                                         new { ListId = listId, UserId = userId });
        }

        public async Task<IEnumerable<User>> GetToBeNotifiedOfListDeletionAsync(int listId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryAsync<User>(@"SELECT u.*
                                                FROM ""AspNetUsers"" AS u
                                                INNER JOIN ""ToDoAssistant.Shares"" AS s ON u.""Id"" = s.""UserId""
                                                WHERE s.""ListId"" = @ListId AND s.""IsAccepted"" AND u.""ToDoNotificationsEnabled"" AND s.""NotificationsEnabled""",
                                            new { ListId = listId });
        }

        public async Task<IEnumerable<User>> GetToBeNotifiedOfRecipeSentAsync(int recipeId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryAsync<User>(@"SELECT u.*
                                                FROM ""AspNetUsers"" AS u
                                                INNER JOIN ""CookingAssistant.SendRequests"" AS sr ON u.""Id"" = sr.""UserId""
                                                WHERE sr.""RecipeId"" = @RecipeId AND u.""CookingNotificationsEnabled""",
                                            new { RecipeId = recipeId });
        }

        public async Task UpdateToDoNotificationsEnabledAsync(int id, bool enabled)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            await conn.QueryAsync(@"UPDATE ""AspNetUsers""
                                        SET ""ToDoNotificationsEnabled"" = @Enabled
                                        WHERE ""Id"" = @Id",
                                    new
                                    {
                                        Id = id,
                                        Enabled = enabled
                                    });
        }

        public async Task UpdateCookingNotificationsEnabledAsync(int id, bool enabled)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            await conn.QueryAsync(@"UPDATE ""AspNetUsers""
                                        SET ""CookingNotificationsEnabled"" = @Enabled
                                        WHERE ""Id"" = @Id",
                                    new
                                    {
                                        Id = id,
                                        Enabled = enabled
                                    });
        }

        public async Task UpdateImperialSystemAsync(int id, bool imperialSystem)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            await conn.QueryAsync(@"UPDATE ""AspNetUsers""
                                        SET ""ImperialSystem"" = @ImperialSystem
                                        WHERE ""Id"" = @Id",
                                    new
                                    {
                                        Id = id,
                                        ImperialSystem = imperialSystem
                                    });
        }
    }
}