using System.Threading.Tasks;
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
            EFContext.PushSubscriptions.Add(subscription);
            await EFContext.SaveChangesAsync();
        }
    }
}
