using Core.Application.Contracts;
using Core.Application.Entities;

namespace Core.Persistence.Repositories;

public class PushSubscriptionsRepository : BaseRepository, IPushSubscriptionsRepository
{
    public PushSubscriptionsRepository(CoreContext efContext)
        : base(efContext) { }

    public async Task CreateSubscriptionAsync(PushSubscription subscription, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(PushSubscriptionsRepository)}.{nameof(CreateSubscriptionAsync)}");

        try
        {
            EFContext.PushSubscriptions.Add(subscription);
            await EFContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            metric.Finish();
        }
    }
}
