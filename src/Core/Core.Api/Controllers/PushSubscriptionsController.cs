using Api.Common;
using Core.Api.Models.PushNotifications.Requests;
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
    public async Task<IActionResult> CreateSubscription([FromBody] PushNotificationsSubscriptionRequest request)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/pushsubscriptions",
            $"{nameof(PushSubscriptionsController)}.{nameof(CreateSubscription)}",
            UserId
        );

        await _pushSubscriptionService.CreateSubscriptionAsync(UserId,
            request.Application,
            request.Subscription.Endpoint,
            request.Subscription.Keys["auth"],
            request.Subscription.Keys["p256dh"],
            tr);

        tr.Finish();

        return StatusCode(201);
    }
}
