using System.Threading.Tasks;
using Application.Contracts.Common;
using Domain.Entities.Common;

namespace Persistence.Repositories.Common
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
