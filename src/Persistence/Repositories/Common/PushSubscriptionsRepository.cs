using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Persistence;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Persistence.Repositories.Common
{
    public class PushSubscriptionsRepository : BaseRepository, IPushSubscriptionsRepository
    {
        public PushSubscriptionsRepository(PersonalAssistantContext efContext)
            : base(efContext) { }

        public async Task CreateSubscriptionAsync(PushSubscription subscription)
        {
            using IDbConnection conn = OpenConnection();

            await conn.ExecuteAsync(@"INSERT INTO ""PushSubscriptions"" (""UserId"", ""Application"", ""Endpoint"", ""AuthKey"", ""P256dhKey"", ""CreatedDate"")
                                          VALUES (@UserId, @Application, @Endpoint, @AuthKey, @P256dhKey, @CreatedDate)", subscription);
        }

        public async Task DeleteSubscriptionAsync(int id)
        {
            using IDbConnection conn = OpenConnection();

            await conn.ExecuteAsync(@"DELETE FROM ""PushSubscriptions"" WHERE ""Id"" = @Id", new { Id = id });
        }

        public async Task<IEnumerable<PushSubscription>> GetAllAsync(int userId, string application)
        {
            using IDbConnection conn = OpenConnection();

            return await conn.QueryAsync<PushSubscription>(@"SELECT * FROM ""PushSubscriptions"" WHERE ""UserId"" = @UserId AND ""Application"" = @Application",
                new { UserId = userId, Application = application });
        }
    }
}
