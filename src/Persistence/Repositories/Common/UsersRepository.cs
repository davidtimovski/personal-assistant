using System.Data;
using System.Threading.Tasks;
using Application.Contracts.Common;
using Dapper;
using Domain.Entities.Common;

namespace Persistence.Repositories.Common;

public class UsersRepository : BaseRepository, IUsersRepository
{
    public UsersRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public User Get(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<User>(@"SELECT * FROM users WHERE id = @id", new { id });
    }

    public User Get(string email)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<User>(@"SELECT * FROM users WHERE email = @email", new { email });
    }

    public int? GetId(string auth0Id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<int?>(@"SELECT user_id FROM user_id_map WHERE auth0_id = @auth0Id", new { auth0Id });
    }

    public bool Exists(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM users WHERE id = @id", new { id });
    }

    public string GetLanguage(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<string>(@"SELECT language FROM users WHERE id = @id", new { id });
    }

    public string GetImageUri(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<string>(@"SELECT image_uri FROM users WHERE id = @id", new { id });
    }

    public async Task UpdateProfileAsync(int id, string name, string language, string culture, string imageUri)
    {
        using IDbConnection conn = OpenConnection();

        await conn.QueryAsync(@"UPDATE users
                                SET name = @name, language = @language, culture = @culture, image_uri = @imageUri
                                WHERE id = @id",
            new
            {
                id,
                name,
                language,
                culture,
                imageUri
            });
    }

    public async Task UpdateToDoNotificationsEnabledAsync(int id, bool enabled)
    {
        using IDbConnection conn = OpenConnection();

        await conn.QueryAsync(@"UPDATE users
                                SET todo_notifications_enabled = @enabled
                                WHERE id = @id",
            new
            {
                id,
                enabled
            });
    }

    public async Task UpdateCookingNotificationsEnabledAsync(int id, bool enabled)
    {
        using IDbConnection conn = OpenConnection();

        await conn.QueryAsync(@"UPDATE users
                                SET cooking_notifications_enabled = @enabled
                                WHERE id = @id",
            new
            {
                id,
                enabled
            });
    }

    public async Task UpdateImperialSystemAsync(int id, bool imperialSystem)
    {
        using IDbConnection conn = OpenConnection();

        await conn.QueryAsync(@"UPDATE users
                                SET imperial_system = @imperialSystem
                                WHERE id = @id",
            new
            {
                id,
                imperialSystem
            });
    }
}
