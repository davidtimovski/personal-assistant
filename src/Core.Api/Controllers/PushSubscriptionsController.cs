﻿using Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Core.Api.Controllers;

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

        await _pushSubscriptionService.CreateSubscriptionAsync(UserId,
            dto.Application,
            dto.Subscription.Endpoint,
            dto.Subscription.Keys["auth"],
            dto.Subscription.Keys["p256dh"]);

        return StatusCode(201);
    }
}