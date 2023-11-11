using System.Data;
using Core.Application.Contracts;
using Core.Application.Entities;
using Dapper;
using Sentry;

namespace Core.Persistence.Repositories;

public class TooltipsRepository : BaseRepository, ITooltipsRepository
{
    public TooltipsRepository(CoreContext efContext)
        : base(efContext) { }

    public IReadOnlyList<Tooltip> GetAll(string application, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(TooltipsRepository)}.{nameof(GetAll)}");

        try
        {
            using IDbConnection conn = OpenConnection();

            var result = conn.Query<Tooltip>(@"SELECT t.*, (td.user_id IS NOT NULL) AS is_dismissed
                                     FROM tooltips AS t
                                     LEFT JOIN tooltips_dismissed AS td ON t.id = td.tooltip_id AND td.user_id = @UserId
                                     WHERE application = @Application",
                new { Application = application, UserId = userId }).ToList();

            return result;
        }
        finally
        {
            metric.Finish();
        }
    }

    public Tooltip GetByKey(int userId, string key, string application, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(TooltipsRepository)}.{nameof(GetByKey)}");

        try
        {
            using IDbConnection conn = OpenConnection();

            var result = conn.QueryFirstOrDefault<Tooltip>(@"SELECT t.*, (td.user_id IS NOT NULL) AS is_dismissed
                                                   FROM tooltips AS t
                                                   LEFT JOIN tooltips_dismissed AS td ON t.id = td.tooltip_id AND td.user_id = @UserId
                                                   WHERE t.key = @Key AND t.application = @Application", new { UserId = userId, Key = key, Application = application });

            return result;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(TooltipsRepository)}.{nameof(ToggleDismissedAsync)}");

        try
        {
            using IDbConnection conn = OpenConnection();

            var id = await conn.ExecuteScalarAsync<int>(new CommandDefinition("SELECT id FROM tooltips WHERE key = @Key AND application = @Application",
                new { Key = key, Application = application },
                cancellationToken: cancellationToken));

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

            await EFContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            metric.Finish();
        }
    }
}
