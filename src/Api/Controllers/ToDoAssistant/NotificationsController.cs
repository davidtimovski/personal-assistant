using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Application.Contracts.ToDoAssistant.Notifications;
using Infrastructure.Identity;

namespace Api.Controllers.ToDoAssistant;

[Authorize]
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
    public IActionResult GetAll()
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

        var notificationDtos = _notificationService.GetAllAndFlagUnseen(userId);

        return Ok(notificationDtos);
    }

    [HttpGet("unseen-notifications-count")]
    public IActionResult GetUnseenNotificationsCount()
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

        int unseenNotificationsCount = _notificationService.GetUnseenNotificationsCount(userId);

        return Ok(unseenNotificationsCount);
    }
}
