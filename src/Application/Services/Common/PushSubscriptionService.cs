using System;
using System.Threading.Tasks;
using Application.Contracts;
using Application.Contracts.Common;
using Domain.Entities.Common;

namespace Application.Services.Common
{
    public class PushSubscriptionService : IPushSubscriptionService
    {
        private readonly IPushSubscriptionsRepository _pushSubscriptionsRepository;

        public PushSubscriptionService(IPushSubscriptionsRepository pushSubscriptionsRepository)
        {
            _pushSubscriptionsRepository = pushSubscriptionsRepository;
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

            await _pushSubscriptionsRepository.CreateSubscriptionAsync(subscription);
        }
    }
}
