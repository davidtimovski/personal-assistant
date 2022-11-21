using Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoAssistant.Application.Contracts.Notifications;

namespace ToDoAssistant.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class NotificationsController : BaseController
{
    private readonly INotificationService _notificationService;

    public NotificationsController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        INotificationService notificationService) : base(userIdLookup, usersRepository)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var notificationDtos = _notificationService.GetAllAndFlagUnseen(UserId);

        return Ok(notificationDtos);
    }

    [HttpGet("unseen-notifications-count")]
    public IActionResult GetUnseenNotificationsCount()
    {
        int unseenNotificationsCount = _notificationService.GetUnseenNotificationsCount(UserId);

        return Ok(unseenNotificationsCount);
    }
}
