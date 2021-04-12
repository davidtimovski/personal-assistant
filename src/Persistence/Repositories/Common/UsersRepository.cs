using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Persistence;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Persistence.Repositories.Common
{
    public class UsersRepository : BaseRepository, IUsersRepository
    {
        public UsersRepository(PersonalAssistantContext efContext)
            : base(efContext) { }

        public async Task<User> GetAsync(int id)
        {
            return await Dapper.QueryFirstOrDefaultAsync<User>(@"SELECT * FROM ""AspNetUsers"" WHERE ""Id"" = @Id", new { Id = id });
        }

        public async Task<User> GetAsync(string email)
        {
            return await Dapper.QueryFirstOrDefaultAsync<User>(@"SELECT * FROM ""AspNetUsers""
                                                                WHERE ""Email"" = @Email AND ""EmailConfirmed"" = TRUE",
                new { Email = email });
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*) FROM ""AspNetUsers"" WHERE ""Id"" = @id",
                new { Id = id });
        }

        public async Task<string> GetLanguageAsync(int id)
        {
            return await Dapper.QueryFirstOrDefaultAsync<string>(@"SELECT ""Language"" FROM ""AspNetUsers"" WHERE ""Id"" = @Id", new { Id = id });
        }

        public async Task<string> GetImageUriAsync(int id)
        {
            return await Dapper.QueryFirstOrDefaultAsync<string>(@"SELECT ""ImageUri"" FROM ""AspNetUsers"" WHERE ""Id"" = @Id", new { Id = id });
        }

        public async Task<IEnumerable<User>> GetToBeNotifiedOfListChangeAsync(int listId, int excludeUserId)
        {
            return await Dapper.QueryAsync<User>(@"SELECT u.*
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

        public async Task<IEnumerable<User>> GetToBeNotifiedOfRecipeChangeAsync(int recipeId, int excludeUserId)
        {
            return await Dapper.QueryAsync<User>(@"SELECT u.*
                                                FROM ""AspNetUsers"" AS u
                                                INNER JOIN ""CookingAssistant.Shares"" AS s ON u.""Id"" = s.""UserId""
                                                WHERE u.""Id"" != @ExcludeUserId AND s.""RecipeId"" = @RecipeId AND s.""IsAccepted"" AND u.""ToDoNotificationsEnabled""
                                                UNION
                                                SELECT u.*
                                                FROM ""AspNetUsers"" AS u
                                                INNER JOIN ""CookingAssistant.Recipes"" AS r ON u.""Id"" = r.""UserId""
                                                WHERE u.""Id"" != @ExcludeUserId AND r.""Id"" = @RecipeId AND u.""ToDoNotificationsEnabled""",
                                            new { RecipeId = recipeId, ExcludeUserId = excludeUserId });
        }

        public async Task<bool> CheckIfUserCanBeNotifiedOfListChangeAsync(int listId, int userId)
        {
            return await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                         FROM ""AspNetUsers"" AS u
                                                         INNER JOIN ""ToDoAssistant.Shares"" AS s ON u.""Id"" = s.""UserId""
                                                         WHERE u.""Id"" = @UserId AND s.""ListId"" = @ListId AND s.""IsAccepted"" AND u.""ToDoNotificationsEnabled"" AND s.""NotificationsEnabled""",
                                                         new { ListId = listId, UserId = userId });
        }

        public async Task<bool> CheckIfUserCanBeNotifiedOfRecipeChangeAsync(int recipeId, int userId)
        {
            return await Dapper.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                         FROM ""AspNetUsers"" AS u
                                                         INNER JOIN ""CookingAssistant.Shares"" AS s ON u.""Id"" = s.""UserId""
                                                         WHERE u.""Id"" = @UserId AND s.""RecipeId"" = @RecipeId AND s.""IsAccepted"" AND u.""ToDoNotificationsEnabled""",
                                                         new { RecipeId = recipeId, UserId = userId });
        }

        public async Task<IEnumerable<User>> GetToBeNotifiedOfListDeletionAsync(int listId)
        {
            return await Dapper.QueryAsync<User>(@"SELECT u.*
                                                FROM ""AspNetUsers"" AS u
                                                INNER JOIN ""ToDoAssistant.Shares"" AS s ON u.""Id"" = s.""UserId""
                                                WHERE s.""ListId"" = @ListId AND s.""IsAccepted"" AND u.""ToDoNotificationsEnabled"" AND s.""NotificationsEnabled""",
                                            new { ListId = listId });
        }

        public async Task<IEnumerable<User>> GetToBeNotifiedOfRecipeDeletionAsync(int recipeId)
        {
            return await Dapper.QueryAsync<User>(@"SELECT u.*
                                                FROM ""AspNetUsers"" AS u
                                                INNER JOIN ""CookingAssistant.Shares"" AS s ON u.""Id"" = s.""UserId""
                                                WHERE s.""RecipeId"" = @RecipeId AND s.""IsAccepted"" AND u.""ToDoNotificationsEnabled""",
                                            new { RecipeId = recipeId });
        }

        public async Task<IEnumerable<User>> GetToBeNotifiedOfRecipeSentAsync(int recipeId)
        {
            return await Dapper.QueryAsync<User>(@"SELECT u.*
                                                FROM ""AspNetUsers"" AS u
                                                INNER JOIN ""CookingAssistant.SendRequests"" AS sr ON u.""Id"" = sr.""UserId""
                                                WHERE sr.""RecipeId"" = @RecipeId AND u.""CookingNotificationsEnabled""",
                                            new { RecipeId = recipeId });
        }

        public async Task UpdateToDoNotificationsEnabledAsync(int id, bool enabled)
        {
            await Dapper.QueryAsync(@"UPDATE ""AspNetUsers""
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
            await Dapper.QueryAsync(@"UPDATE ""AspNetUsers""
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
            await Dapper.QueryAsync(@"UPDATE ""AspNetUsers""
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
