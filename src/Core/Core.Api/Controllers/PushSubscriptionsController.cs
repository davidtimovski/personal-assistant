using Api.Common;
using Core.Api.Models.PushNotifications.Requests;
using Core.Application.Contracts;
using Core.Application.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class PushSubscriptionsController : BaseController
{
    private readonly IPushSubscriptionService _pushSubscriptionService;

    public PushSubscriptionsController(
        IUserIdLookup? userIdLookup,
        IUsersRepository? usersRepository,
        IPushSubscriptionService? pushSubscriptionService) : base(userIdLookup, usersRepository)
    {
        _pushSubscriptionService = ArgValidator.NotNull(pushSubscriptionService);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubscription([FromBody] PushNotificationsSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/pushsubscriptions",
            $"{nameof(PushSubscriptionsController)}.{nameof(CreateSubscription)}",
            UserId
        );

        try
        {
            if (request is null)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return BadRequest();
            }

            var result = await _pushSubscriptionService.CreateSubscriptionAsync(UserId,
                request.Application,
                request.Subscription.Endpoint,
                request.Subscription.Keys["auth"],
                request.Subscription.Keys["p256dh"],
                tr,
                cancellationToken);

            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            return StatusCode(201);
        }
        finally
        {
            tr.Finish();
        }
    }
}
