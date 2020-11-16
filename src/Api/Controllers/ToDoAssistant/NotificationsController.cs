using System;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Introspection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications;
using PersonalAssistant.Infrastructure.Identity;

namespace Api.Controllers.ToDoAssistant
{
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [EnableCors("AllowToDoAssistant")]
    [Route("api/[controller]")]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            int userId;
            try
            {
                userId = IdentityHelper.GetUserId(User);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            var notificationDtos = await _notificationService.GetAllAndFlagUnseenAsync(userId);

            return Ok(notificationDtos);
        }

        [HttpGet("unseen-notifications-count")]
        public async Task<IActionResult> GetUnseenNotificationsCount()
        {
            int userId;
            try
            {
                userId = IdentityHelper.GetUserId(User);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            int unseenNotificationsCount = await _notificationService.GetUnseenNotificationsCountAsync(userId);

            return Ok(unseenNotificationsCount);
        }
    }
}