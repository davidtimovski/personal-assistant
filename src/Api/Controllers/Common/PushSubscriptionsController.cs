using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.Common.Models;
using PersonalAssistant.Infrastructure.Identity;

namespace Api.Controllers.Common
{
    [Authorize]
    [EnableCors("AllowAllApps")]
    [Route("api/[controller]")]
    public class PushSubscriptionsController : Controller
    {
        private readonly IPushSubscriptionService _pushSubscriptionService;

        public PushSubscriptionsController(
            IPushSubscriptionService pushSubscriptionService)
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

            int userId;
            try
            {
                userId = IdentityHelper.GetUserId(User);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            await _pushSubscriptionService.CreateSubscriptionAsync(userId,
                dto.Application,
                dto.Subscription.Endpoint,
                dto.Subscription.Keys["auth"],
                dto.Subscription.Keys["p256dh"]);

            return StatusCode(201);
        }
    }
}