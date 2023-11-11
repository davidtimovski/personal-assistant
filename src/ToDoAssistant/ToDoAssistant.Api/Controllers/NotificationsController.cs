using Core.Application.Contracts;
using Core.Application.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ToDoAssistant.Application.Contracts.Notifications;

namespace ToDoAssistant.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class NotificationsController : BaseController
{
    private readonly INotificationService _notificationService;

    public NotificationsController(
        IUserIdLookup? userIdLookup,
        IUsersRepository? usersRepository,
        INotificationService? notificationService,
        IStringLocalizer<BaseController>? baseLocalizer) : base(userIdLookup, usersRepository, baseLocalizer)
    {
        _notificationService = ArgValidator.NotNull(notificationService);
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var result = _notificationService.GetAllAndFlagUnseen(UserId);

        if (result.Failed)
        {
            return StatusCode(500);
        }

        return Ok(result.Data);
    }

    [HttpGet("unseen-notifications-count")]
    public IActionResult GetUnseenNotificationsCount()
    {
        var result = _notificationService.GetUnseenNotificationsCount(UserId);

        if (result.Failed)
        {
            return StatusCode(500);
        }

        return Ok(result.Data);
    }
}
