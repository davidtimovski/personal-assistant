using System.Data;
using Core.Application.Contracts;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Sentry;
using User = Core.Application.Entities.User;

namespace Core.Persistence.Repositories;

public class UsersRepository : BaseRepository, IUsersRepository
{
    public UsersRepository(CoreContext efContext)
        : base(efContext) { }

    public User Get(int id)
    {
        using IDbConnection conn = OpenConnection();
        return conn.QueryFirstOrDefault<User>("SELECT * FROM users WHERE id = @id", new { id });
    }

    public User Get(string email)
    {
        using IDbConnection conn = OpenConnection();
        return conn.QueryFirstOrDefault<User>("SELECT * FROM users WHERE email = @email", new { email });
    }

    public int? GetId(string auth0Id)
    {
        using IDbConnection conn = OpenConnection();
        return conn.QueryFirstOrDefault<int?>("SELECT user_id FROM user_id_map WHERE auth0_id = @auth0Id", new { auth0Id });
    }

    public bool Exists(int id)
    {
        using IDbConnection conn = OpenConnection();
        return conn.ExecuteScalar<bool>("SELECT COUNT(*) FROM users WHERE id = @id", new { id });
    }

    public bool Exists(string email)
    {
        using IDbConnection conn = OpenConnection();
        return conn.ExecuteScalar<bool>("SELECT COUNT(*) FROM users WHERE email = @email", new { email });
    }

    public async Task<int> CreateAsync(string auth0Id, User user, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(UsersRepository)}.{nameof(CreateAsync)}");

        EFContext.Users.Add(user);
        await EFContext.SaveChangesAsync();

        using IDbConnection conn = OpenConnection();
        await conn.ExecuteAsync("INSERT INTO user_id_map (user_id, auth0_id) VALUES (@userId, @auth0Id)",
            new { userId = user.Id, auth0Id });

        metric.Finish();

        return user.Id;
    }

    public async Task UpdateAsync(User user, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(UsersRepository)}.{nameof(UpdateAsync)}");

        User dbUser = EFContext.Users.First(x => x.Id == user.Id);

        dbUser.Name = user.Name;
        dbUser.Language = user.Language;
        dbUser.Culture = user.Culture;
        dbUser.ToDoNotificationsEnabled = user.ToDoNotificationsEnabled;
        dbUser.CookingNotificationsEnabled = user.CookingNotificationsEnabled;
        dbUser.ImperialSystem = user.ImperialSystem;
        dbUser.ImageUri = user.ImageUri;
        dbUser.ModifiedDate = user.ModifiedDate;

        await EFContext.SaveChangesAsync();

        metric.Finish();
    }

    public async Task DeleteAsync(int id, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(UsersRepository)}.{nameof(DeleteAsync)}");

        var person = EFContext.Users.Attach(new User { Id = id });
        person.State = EntityState.Deleted;

        await EFContext.SaveChangesAsync();

        metric.Finish();
    }
}
