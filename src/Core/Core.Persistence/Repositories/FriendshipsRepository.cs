using System.Data;
using Core.Application.Contracts;
using Core.Application.Entities;
using Dapper;
using Sentry;
using User = Core.Application.Entities.User;

namespace Core.Persistence.Repositories;

public class FriendshipsRepository : BaseRepository, IFriendshipsRepository
{
    public FriendshipsRepository(CoreContext efContext)
        : base(efContext) { }

    public IReadOnlyList<Friendship> GetAll(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(FriendshipsRepository)}.{nameof(GetAll)}");

        try
        {
            using IDbConnection conn = OpenConnection();

            const string query = @"SELECT f.*, us.email, us.name, us.image_uri, ur.email, ur.name, ur.image_uri
                                   FROM friendships AS f
                                   INNER JOIN users AS us ON f.sender_id = us.id
                                   INNER JOIN users AS ur ON f.recipient_id = ur.id
                                   WHERE f.sender_id = @UserId OR f.recipient_id = @UserId";

            return conn.Query<Friendship, User, User, Friendship>(query,
                (share, sender, recipient) =>
                {
                    share.Sender = sender;
                    share.Recipient = recipient;
                    return share;
                }, new { UserId = userId }, null, true, "email,email").ToList();
        }
        finally
        {
            metric.Finish();
        }
    }

    public Friendship? Get(int userId1, int userId2, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(FriendshipsRepository)}.{nameof(Get)}");

        try
        {
            using IDbConnection conn = OpenConnection();

            const string query = @"SELECT f.*, us.email, us.name, us.image_uri, ur.email, ur.name, ur.image_uri
                                   FROM friendships AS f
                                   INNER JOIN users AS us ON f.sender_id = us.id
                                   INNER JOIN users AS ur ON f.recipient_id = ur.id
                                   WHERE (f.sender_id = @UserId1 AND f.recipient_id = @UserId2) OR (f.sender_id = @UserId2 AND f.recipient_id = @UserId1)";

            return conn.Query<Friendship, User, User, Friendship>(query,
                (share, sender, recipient) =>
                {
                    share.Sender = sender;
                    share.Recipient = recipient;
                    return share;
                }, new { UserId1 = userId1, UserId2 = userId2 }, null, true, "email,email").FirstOrDefault();
        }
        finally
        {
            metric.Finish();
        }
    }

    public bool Exists(int userId1, int userId2)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>("SELECT COUNT(*) FROM friendships WHERE (sender_id = @UserId1 AND recipient_id = @UserId2) OR (sender_id = @UserId2 AND recipient_id = @UserId1)",
            new { UserId1 = userId1, UserId2 = userId2 });
    }

    public async Task CreateAsync(Friendship friendship, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(FriendshipsRepository)}.{nameof(CreateAsync)}");

        try
        {
            EFContext.Friendships.Add(friendship);

            await EFContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task UpdateAsync(Friendship friendship, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(FriendshipsRepository)}.{nameof(UpdateAsync)}");

        try
        {
            Friendship dbFriendship = EFContext.Friendships.First(x => x.SenderId == friendship.SenderId && x.RecipientId == friendship.RecipientId);

            dbFriendship.Permissions = friendship.Permissions;
            dbFriendship.ModifiedDate = friendship.ModifiedDate;

            await EFContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task AcceptAsync(int userId1, int userId2, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(FriendshipsRepository)}.{nameof(AcceptAsync)}");

        try
        {
            // TOOD

            await EFContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task DeclineAsync(int userId1, int userId2, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(FriendshipsRepository)}.{nameof(DeclineAsync)}");

        try
        {
            // TOOD

            await EFContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task DeleteAsync(int userId1, int userId2, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(FriendshipsRepository)}.{nameof(DeleteAsync)}");

        try
        {
            // TOOD

            await EFContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            metric.Finish();
        }
    }

    //public Tooltip GetByKey(int userId, string key, string application, ISpan metricsSpan)
    //{
    //    var metric = metricsSpan.StartChild($"{nameof(TooltipsRepository)}.{nameof(GetByKey)}");

    //    try
    //    {
    //        using IDbConnection conn = OpenConnection();

    //        var result = conn.QueryFirstOrDefault<Tooltip>(@"SELECT t.*, (td.user_id IS NOT NULL) AS is_dismissed
    //                                               FROM tooltips AS t
    //                                               LEFT JOIN tooltips_dismissed AS td ON t.id = td.tooltip_id AND td.user_id = @UserId
    //                                               WHERE t.key = @Key AND t.application = @Application", new { UserId = userId, Key = key, Application = application });

    //        return result;
    //    }
    //    finally
    //    {
    //        metric.Finish();
    //    }
    //}

    //public async Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed, ISpan metricsSpan, CancellationToken cancellationToken)
    //{
    //    var metric = metricsSpan.StartChild($"{nameof(TooltipsRepository)}.{nameof(ToggleDismissedAsync)}");

    //    try
    //    {
    //        using IDbConnection conn = OpenConnection();

    //        var id = await conn.ExecuteScalarAsync<int>(new CommandDefinition("SELECT id FROM tooltips WHERE key = @Key AND application = @Application",
    //            new { Key = key, Application = application },
    //            cancellationToken: cancellationToken));

    //        var dismissedTooltip = new TooltipDismissed
    //        {
    //            TooltipId = id,
    //            UserId = userId
    //        };

    //        if (isDismissed)
    //        {
    //            EFContext.TooltipsDismissed.Add(dismissedTooltip);
    //        }
    //        else
    //        {
    //            EFContext.TooltipsDismissed.Remove(dismissedTooltip);
    //        }

    //        await EFContext.SaveChangesAsync(cancellationToken);
    //    }
    //    finally
    //    {
    //        metric.Finish();
    //    }
    //}
}
