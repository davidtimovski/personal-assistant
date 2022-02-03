using System.Threading.Tasks;
using Domain.Entities.Common;

namespace Application.Contracts.Common;

public interface IPushSubscriptionsRepository
{
    Task CreateSubscriptionAsync(PushSubscription subscription);
}