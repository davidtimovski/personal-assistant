using Application.Contracts.ToDoAssistant.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.ToDoAssistant;

[Authorize]
[EnableCors("AllowToDoAssistant")]
[Route("api/[controller]")]
public class NotificationsController : BaseController
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var notificationDtos = _notificationService.GetAllAndFlagUnseen(CurrentUserId);

        return Ok(notificationDtos);
    }

    [HttpGet("unseen-notifications-count")]
    public IActionResult GetUnseenNotificationsCount()
    {
        int unseenNotificationsCount = _notificationService.GetUnseenNotificationsCount(CurrentUserId);

        return Ok(unseenNotificationsCount);
    }
}
