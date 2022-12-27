using Application.Domain.Common;
using Core.Application.Contracts;

namespace Core.Persistence.Repositories;

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
