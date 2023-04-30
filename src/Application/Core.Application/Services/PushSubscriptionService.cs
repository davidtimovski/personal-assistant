using Application.Domain.Common;
using Core.Application.Contracts;
using Microsoft.Extensions.Logging;
using Sentry;

namespace Core.Application.Services;

public class PushSubscriptionService : IPushSubscriptionService
{
    private readonly IPushSubscriptionsRepository _pushSubscriptionsRepository;
    private readonly ILogger<PushSubscriptionService> _logger;

    public PushSubscriptionService(
        IPushSubscriptionsRepository pushSubscriptionsRepository,
        ILogger<PushSubscriptionService> logger)
    {
        _pushSubscriptionsRepository = pushSubscriptionsRepository;
        _logger = logger;
    }

    public async Task CreateSubscriptionAsync(int userId, string application, string endpoint, string authKey, string p256dhKey, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(PushSubscriptionService)}.{nameof(CreateSubscriptionAsync)}");

        var subscription = new PushSubscription
        {
            UserId = userId,
            Application = application,
            Endpoint = endpoint,
            AuthKey = authKey,
            P256dhKey = p256dhKey,
            CreatedDate = DateTime.UtcNow
        };

        try
        {
            await _pushSubscriptionsRepository.CreateSubscriptionAsync(subscription, metric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateSubscriptionAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }
}
