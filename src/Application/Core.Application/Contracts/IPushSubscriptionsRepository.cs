using Application.Domain.Common;
using Sentry;

namespace Core.Application.Contracts;

public interface IPushSubscriptionsRepository
{
    Task CreateSubscriptionAsync(PushSubscription subscription, ITransaction tr);
}
