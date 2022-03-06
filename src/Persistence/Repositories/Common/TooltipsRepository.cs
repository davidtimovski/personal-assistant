using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Application.Contracts.Common;
using Domain.Entities.Common;

namespace Persistence.Repositories.Common;

public class TooltipsRepository : BaseRepository, ITooltipsRepository
{
    public TooltipsRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public IEnumerable<Tooltip> GetAll(string application, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<Tooltip>(@"SELECT t.*, (td.""UserId"" IS NOT NULL) AS ""IsDismissed""
                                         FROM ""Tooltips"" AS t
                                         LEFT JOIN ""TooltipsDismissed"" AS td ON t.""Id"" = td.""TooltipId"" AND td.""UserId"" = @UserId
                                         WHERE ""Application"" = @Application",
            new { Application = application, UserId = userId });
    }

    public Tooltip GetByKey(int userId, string key, string application)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<Tooltip>(@"SELECT t.*, (td.""UserId"" IS NOT NULL) AS ""IsDismissed""
                                                       FROM ""Tooltips"" AS t
                                                       LEFT JOIN ""TooltipsDismissed"" AS td ON t.""Id"" = td.""TooltipId"" AND td.""UserId"" = @UserId
                                                       WHERE t.""Key"" = @Key AND t.""Application"" = @Application", new { UserId = userId, Key = key, Application = application });
    }

    public async Task ToggleDismissedAsync(int userId, string key, string application, bool isDismissed)
    {
        using IDbConnection conn = OpenConnection();

        var id = await conn.ExecuteScalarAsync<int>(@"SELECT ""Id"" FROM ""Tooltips"" WHERE ""Key"" = @Key AND ""Application"" = @Application", new { Key = key, Application = application });

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
