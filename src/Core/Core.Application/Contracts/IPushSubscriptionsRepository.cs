using Core.Application.Entities;
using Sentry;

namespace Core.Application.Contracts;

public interface IPushSubscriptionsRepository
{
    Task CreateSubscriptionAsync(PushSubscription subscription, ISpan metricsSpan, CancellationToken cancellationToken);
}
