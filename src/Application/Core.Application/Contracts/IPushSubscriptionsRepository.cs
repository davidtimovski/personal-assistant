using Application.Domain.Common;

namespace Core.Application.Contracts;

public interface IPushSubscriptionsRepository
{
    Task CreateSubscriptionAsync(PushSubscription subscription);
}
