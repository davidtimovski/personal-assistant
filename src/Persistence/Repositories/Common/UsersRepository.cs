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

        return conn.QueryFirst<User>(@"SELECT * FROM users WHERE id = @id", new { id });
    }

    public User Get(string email)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirst<User>(@"SELECT * FROM users WHERE email = @email", new { email });
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

    public async Task UpdateAsync(User user)
    {
        User dbUser = EFContext.Users.Find(user.Id);

        dbUser.Name = user.Name;
        dbUser.Language = user.Language;
        dbUser.Culture = user.Culture;
        dbUser.ToDoNotificationsEnabled = user.ToDoNotificationsEnabled;
        dbUser.CookingNotificationsEnabled = user.CookingNotificationsEnabled;
        dbUser.ImperialSystem = user.ImperialSystem;
        dbUser.ImageUri = user.ImageUri;
        dbUser.ModifiedDate = user.ModifiedDate;

        await EFContext.SaveChangesAsync();
    }
}
