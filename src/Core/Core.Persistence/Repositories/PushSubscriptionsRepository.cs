using Core.Application.Contracts;
using Core.Application.Entities;
using Sentry;

namespace Core.Persistence.Repositories;

public class PushSubscriptionsRepository : BaseRepository, IPushSubscriptionsRepository
{
    public PushSubscriptionsRepository(CoreContext efContext)
        : base(efContext) { }

    public async Task CreateSubscriptionAsync(PushSubscription subscription, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(PushSubscriptionsRepository)}.{nameof(CreateSubscriptionAsync)}");

        EFContext.PushSubscriptions.Add(subscription);
        await EFContext.SaveChangesAsync(cancellationToken);

        metric.Finish();
    }
}
