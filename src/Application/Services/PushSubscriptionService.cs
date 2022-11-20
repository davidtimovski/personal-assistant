using Application.Contracts;
using Domain.Common;
using Microsoft.Extensions.Logging;

namespace Application.Services;

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

    public async Task CreateSubscriptionAsync(int userId, string application, string endpoint, string authKey, string p256dhKey)
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

        try
        {
            await _pushSubscriptionsRepository.CreateSubscriptionAsync(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateSubscriptionAsync)}");
            throw;
        }
    }
}
