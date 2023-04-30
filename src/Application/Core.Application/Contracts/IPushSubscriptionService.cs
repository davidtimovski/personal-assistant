using Sentry;

namespace Core.Application.Contracts;

public interface IPushSubscriptionService
{
    Task CreateSubscriptionAsync(int userId, string application, string endpoint, string authKey, string p256dhKey, ISpan metricsSpan);
}
