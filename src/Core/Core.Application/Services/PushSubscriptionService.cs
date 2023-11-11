using Core.Application.Contracts;
using Core.Application.Entities;
using Core.Application.Utils;
using Microsoft.Extensions.Logging;
using Sentry;

namespace Core.Application.Services;

public class PushSubscriptionService : IPushSubscriptionService
{
    private readonly IPushSubscriptionsRepository _pushSubscriptionsRepository;
    private readonly ILogger<PushSubscriptionService> _logger;

    public PushSubscriptionService(
        IPushSubscriptionsRepository? pushSubscriptionsRepository,
        ILogger<PushSubscriptionService>? logger)
    {
        _pushSubscriptionsRepository = ArgValidator.NotNull(pushSubscriptionsRepository);
        _logger = ArgValidator.NotNull(logger);
    }

    public async Task<Result> CreateSubscriptionAsync(int userId, string application, string endpoint, string authKey, string p256dhKey, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(PushSubscriptionService)}.{nameof(CreateSubscriptionAsync)}");

        try
        {
            var subscription = new PushSubscription
            {
                UserId = userId,
                Application = application,
                Endpoint = endpoint,
                AuthKey = authKey,
                P256dhKey = p256dhKey,
                CreatedDate = DateTime.UtcNow
            };

            await _pushSubscriptionsRepository.CreateSubscriptionAsync(subscription, metric, cancellationToken);

            return new(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateSubscriptionAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }
}
