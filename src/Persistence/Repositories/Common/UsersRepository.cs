using System.Data;
using System.Threading.Tasks;
using Dapper;
using Application.Contracts.Common;
using Domain.Entities.Common;

namespace Persistence.Repositories.Common;

public class UsersRepository : BaseRepository, IUsersRepository
{
    public UsersRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public User Get(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<User>(@"SELECT * FROM ""AspNetUsers"" WHERE ""Id"" = @Id", new { Id = id });
    }

    public User Get(string email)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<User>(@"SELECT * FROM ""AspNetUsers"" WHERE ""Email"" = @Email AND ""EmailConfirmed"" = TRUE", new { Email = email });
    }

    public bool Exists(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM ""AspNetUsers"" WHERE ""Id"" = @Id", new { Id = id });
    }

    public string GetLanguage(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<string>(@"SELECT ""Language"" FROM ""AspNetUsers"" WHERE ""Id"" = @Id", new { Id = id });
    }

    public string GetImageUri(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<string>(@"SELECT ""ImageUri"" FROM ""AspNetUsers"" WHERE ""Id"" = @Id", new { Id = id });
    }

    public async Task UpdateToDoNotificationsEnabledAsync(int id, bool enabled)
    {
        using IDbConnection conn = OpenConnection();

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
        using IDbConnection conn = OpenConnection();

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
        using IDbConnection conn = OpenConnection();

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