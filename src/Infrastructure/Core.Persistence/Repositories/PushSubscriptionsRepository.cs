using Application.Domain.Common;
using Core.Application.Contracts;
using Sentry;

namespace Core.Persistence.Repositories;

public class PushSubscriptionsRepository : BaseRepository, IPushSubscriptionsRepository
{
    public PushSubscriptionsRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public async Task CreateSubscriptionAsync(PushSubscription subscription, ITransaction tr)
    {
        var span = tr.StartChild($"{nameof(PushSubscriptionsRepository)}.{nameof(CreateSubscriptionAsync)}");

        EFContext.PushSubscriptions.Add(subscription);
        await EFContext.SaveChangesAsync();

        span.Finish();
    }
}
