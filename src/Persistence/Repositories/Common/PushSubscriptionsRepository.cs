using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using Persistence;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Persistence.Repositories.Common
{
    public class PushSubscriptionsRepository : BaseRepository, IPushSubscriptionsRepository
    {
        public PushSubscriptionsRepository(IOptions<DatabaseSettings> databaseSettings, PersonalAssistantContext efContext)
            : base(databaseSettings.Value.DefaultConnectionString, efContext) { }

        public async Task CreateSubscriptionAsync(PushSubscription subscription)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"INSERT INTO ""PushSubscriptions"" (""UserId"", ""Application"", ""Endpoint"", ""AuthKey"", ""P256dhKey"", ""CreatedDate"")
                                          VALUES (@UserId, @Application, @Endpoint, @AuthKey, @P256dhKey, @CreatedDate)", subscription);
        }

        public async Task DeleteSubscriptionAsync(int id)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"DELETE FROM ""PushSubscriptions"" WHERE ""Id"" = @Id", new { Id = id });
        }

        public async Task<IEnumerable<PushSubscription>> GetAllAsync(int userId, string application)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryAsync<PushSubscription>(@"SELECT * FROM ""PushSubscriptions"" WHERE ""UserId"" = @UserId AND ""Application"" = @Application",
                new { UserId = userId, Application = application });
        }
    }
}
