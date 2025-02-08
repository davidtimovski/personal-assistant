using Core.Application.Entities;

namespace Core.Application.Contracts;

public interface IPushSubscriptionsRepository
{
    Task CreateSubscriptionAsync(PushSubscription subscription, ISpan metricsSpan, CancellationToken cancellationToken);
}
