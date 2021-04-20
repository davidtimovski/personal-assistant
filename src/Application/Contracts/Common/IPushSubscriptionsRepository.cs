using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Application.Contracts.Common
{
    public interface IPushSubscriptionsRepository
    {
        Task CreateSubscriptionAsync(PushSubscription subscription);
    }
}
