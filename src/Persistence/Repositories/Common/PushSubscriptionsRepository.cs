using System.Collections.Generic;
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
            await Dapper.ExecuteAsync(@"INSERT INTO ""PushSubscriptions"" (""UserId"", ""Application"", ""Endpoint"", ""AuthKey"", ""P256dhKey"", ""CreatedDate"")
                                          VALUES (@UserId, @Application, @Endpoint, @AuthKey, @P256dhKey, @CreatedDate)", subscription);
        }

        public async Task DeleteSubscriptionAsync(int id)
        {
            await Dapper.ExecuteAsync(@"DELETE FROM ""PushSubscriptions"" WHERE ""Id"" = @Id", new { Id = id });
        }

        public async Task<IEnumerable<PushSubscription>> GetAllAsync(int userId, string application)
        {
            return await Dapper.QueryAsync<PushSubscription>(@"SELECT * FROM ""PushSubscriptions"" WHERE ""UserId"" = @UserId AND ""Application"" = @Application",
                new { UserId = userId, Application = application });
        }
    }
}
