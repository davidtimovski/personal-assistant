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
    }
}
