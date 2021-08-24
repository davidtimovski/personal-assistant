using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Persistence;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Persistence.Repositories.Common
{
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

            if (isDismissed)
            {
                await conn.QueryAsync(@"INSERT INTO ""TooltipsDismissed"" (""TooltipId"", ""UserId"") 
                                        VALUES (@TooltipId, @UserId)", new { TooltipId = id, UserId = userId });
            }
            else
            {
                await conn.QueryAsync<int>(@"DELETE FROM ""TooltipsDismissed"" WHERE ""TooltipId"" = @TooltipId AND ""UserId"" = @UserId",
                    new { TooltipId = id, UserId = userId });
            }
        }
    }
}
