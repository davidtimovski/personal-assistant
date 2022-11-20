using System.Data;
using Application.Contracts;
using Dapper;
using Domain.Common;

namespace Persistence.Repositories;

public class TooltipsRepository : BaseRepository, ITooltipsRepository
{
    public TooltipsRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public IEnumerable<Tooltip> GetAll(string application, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<Tooltip>(@"SELECT t.*, (td.user_id IS NOT NULL) AS is_dismissed
                                     FROM tooltips AS t
                                     LEFT JOIN tooltips_dismissed AS td ON t.id = td.tooltip_id AND td.user_id = @UserId
                                     WHERE application = @Application",
            new { Application = application, UserId = userId });
    }

    public Tooltip GetByKey(int userId, string key, string application)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<Tooltip>(@"SELECT t.*, (td.user_id IS NOT NULL) AS is_dismissed
                                                   FROM tooltips AS t
                                                   LEFT JOIN tooltips_dismissed AS td ON t.id = td.tooltip_id AND td.user_id = @UserId
                                                   WHERE t.key = @Key AND t.application = @Application", new { UserId = userId, Key = key, Application = application });
    }

    public async Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed)
    {
        using IDbConnection conn = OpenConnection();

        var id = await conn.ExecuteScalarAsync<int>(@"SELECT id FROM tooltips WHERE key = @Key AND application = @Application", new { Key = key, Application = application });

        var dismissedTooltip = new TooltipDismissed
        {
            TooltipId = id,
            UserId = userId
        };

        if (isDismissed)
        {
            EFContext.TooltipsDismissed.Add(dismissedTooltip);
        }
        else
        {
            EFContext.TooltipsDismissed.Remove(dismissedTooltip);
        }

        await EFContext.SaveChangesAsync();
    }
}
