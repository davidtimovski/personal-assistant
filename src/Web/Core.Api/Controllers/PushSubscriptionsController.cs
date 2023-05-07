using Api.Common;
using Core.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class PushSubscriptionsController : BaseController
{
    private readonly IPushSubscriptionService _pushSubscriptionService;

    public PushSubscriptionsController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        IPushSubscriptionService pushSubscriptionService) : base(userIdLookup, usersRepository)
    {
        _pushSubscriptionService = pushSubscriptionService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubscription([FromBody] PushNotificationsSubscription dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            "POST api/pushsubscriptions",
            $"{nameof(PushSubscriptionsController)}.{nameof(CreateSubscription)}",
            UserId
        );

        await _pushSubscriptionService.CreateSubscriptionAsync(UserId,
            dto.Application,
            dto.Subscription.Endpoint,
            dto.Subscription.Keys["auth"],
            dto.Subscription.Keys["p256dh"],
            tr);

        tr.Finish();

        return StatusCode(201);
    }
}
