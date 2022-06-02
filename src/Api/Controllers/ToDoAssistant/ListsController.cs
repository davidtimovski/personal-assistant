using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Api.Config;
using Api.Models;
using Api.Models.ToDoAssistant.Lists;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Application.Contracts.Common;
using Application.Contracts.ToDoAssistant.Lists;
using Application.Contracts.ToDoAssistant.Lists.Models;
using Application.Contracts.ToDoAssistant.Notifications;
using Application.Contracts.ToDoAssistant.Notifications.Models;
using Infrastructure.Identity;
using Infrastructure.Sender.Models;

namespace Api.Controllers.ToDoAssistant;

[Authorize]
[EnableCors("AllowToDoAssistant")]
[Route("api/[controller]")]
public class ListsController : Controller
{
    private readonly IListService _listService;
    private readonly INotificationService _notificationService;
    private readonly ISenderService _senderService;
    private readonly IUserService _userService;
    private readonly IValidator<CreateList> _createValidator;
    private readonly IValidator<UpdateList> _updateValidator;
    private readonly IValidator<UpdateSharedList> _updateSharedValidator;
    private readonly IValidator<ShareList> _shareValidator;
    private readonly IValidator<CopyList> _copyValidator;
    private readonly IStringLocalizer<ListsController> _localizer;
    private readonly Urls _urls;

    public ListsController(
        IListService listService,
        INotificationService notificationService,
        ISenderService senderService,
        IUserService userService,
        IValidator<CreateList> createValidator,
        IValidator<UpdateList> updateValidator,
        IValidator<UpdateSharedList> updateSharedValidator,
        IValidator<ShareList> shareValidator,
        IValidator<CopyList> copyValidator,
        IStringLocalizer<ListsController> localizer,
        IOptions<Urls> urls)
    {
        _listService = listService;
        _notificationService = notificationService;
        _senderService = senderService;
        _userService = userService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _updateSharedValidator = updateSharedValidator;
        _shareValidator = shareValidator;
        _copyValidator = copyValidator;
        _localizer = localizer;
        _urls = urls.Value;
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

        IEnumerable<ListDto> lists = _listService.GetAll(userId);

        return Ok(lists);
    }

    [HttpGet("options")]
    public IActionResult GetAllAsOptions()
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

        IEnumerable<ToDoListOption> options = _listService.GetAllAsOptions(userId);

        return Ok(options);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
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

        EditListDto list = _listService.GetForEdit(id, userId);
        if (list == null)
        {
            return NotFound();
        }

        return Ok(list);
    }

    [HttpGet("{id}/with-shares")]
    public IActionResult GetWithShares(int id)
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

        ListWithShares list = _listService.GetWithShares(id, userId);
        if (list == null)
        {
            return NotFound();
        }

        return Ok(list);
    }

    [HttpGet("share-requests")]
    public IActionResult GetShareRequests()
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

        IEnumerable<ShareListRequest> shareRequests = _listService.GetShareRequests(userId);

        return Ok(shareRequests);
    }

    [HttpGet("pending-share-requests-count")]
    public IActionResult GetPendingShareRequestsCount()
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

        int pendingShareRequestsCount = _listService.GetPendingShareRequestsCount(userId);

        return Ok(pendingShareRequestsCount);
    }

    [HttpGet("{id}/shared")]
    public IActionResult GetIsShared(int id)
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

        bool isShared = _listService.IsShared(id, userId);

        return Ok(isShared);
    }

    [HttpGet("{id}/members")]
    public IActionResult GetMembersAsAssigneeOptions(int id)
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

        var assigneeOptions = _listService.GetMembersAsAssigneeOptions(id, userId);

        return Ok(assigneeOptions);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateList dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        try
        {
            dto.UserId = IdentityHelper.GetUserId(User);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }

        int id = await _listService.CreateAsync(dto, _createValidator);

        return StatusCode(201, id);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateList dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        try
        {
            dto.UserId = IdentityHelper.GetUserId(User);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }

        UpdateListResult result = await _listService.UpdateAsync(dto, _updateValidator);
        if (!result.Notify())
        {
            return NoContent();
        }

        string resourceKey;
        switch (result.Type)
        {
            case ListNotificationType.NameUpdated:
                resourceKey = "UpdatedListNameNotification";
                break;
            case ListNotificationType.IconUpdated:
                resourceKey = "UpdatedListIconNotification";
                break;
            default:
                resourceKey = "UpdatedListNotification";
                break;
        }

        foreach (var recipient in result.NotificationRecipients)
        {
            CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
            var message = _localizer[resourceKey, IdentityHelper.GetUserName(User), result.OriginalListName];

            var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, dto.UserId, dto.Id, null, message);
            var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
            var toDoAssistantPushNotification = new ToDoAssistantPushNotification
            {
                SenderImageUri = result.ActionUserImageUri,
                UserId = recipient.Id,
                Message = message,
                OpenUrl = GetNotificationsPageUrl(notificationId)
            };

            _senderService.Enqueue(toDoAssistantPushNotification);
        }

        return NoContent();
    }

    [HttpPut("shared")]
    public async Task<IActionResult> UpdateShared([FromBody] UpdateSharedList dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        try
        {
            dto.UserId = IdentityHelper.GetUserId(User);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }

        await _listService.UpdateSharedAsync(dto, _updateSharedValidator);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
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

        DeleteListResult result = await _listService.DeleteAsync(id, userId);

        foreach (var recipient in result.NotificationRecipients)
        {
            CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
            var message = _localizer["DeletedListNotification", IdentityHelper.GetUserName(User), result.DeletedListName];

            var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, userId, null, null, message);
            var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
            var toDoAssistantPushNotification = new ToDoAssistantPushNotification
            {
                SenderImageUri = result.ActionUserImageUri,
                UserId = recipient.Id,
                Message = message,
                OpenUrl = GetNotificationsPageUrl(notificationId)
            };

            _senderService.Enqueue(toDoAssistantPushNotification);
        }

        return NoContent();
    }

    [HttpGet("can-share-with-user/{email}")]
    public IActionResult CanShareListWithUser(string email)
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

        var canShareVm = new CanShareVm
        {
            CanShare = false
        };

        var user = _userService.Get(email);

        if (user != null)
        {
            canShareVm.UserId = user.Id;
            canShareVm.ImageUri = user.ImageUri;
            canShareVm.CanShare = _listService.CanShareWithUser(user.Id, userId);
        }

        return Ok(canShareVm);
    }

    [HttpPut("share")]
    public async Task<IActionResult> Share([FromBody] ShareList dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        try
        {
            dto.UserId = IdentityHelper.GetUserId(User);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }

        foreach (ShareUserAndPermission removedShare in dto.RemovedShares)
        {
            if (!_listService.CheckIfUserCanBeNotifiedOfChange(dto.ListId, removedShare.UserId))
            {
                continue;
            }

            var currentUser = _userService.Get(dto.UserId);
            var user = _userService.Get(removedShare.UserId);
            SimpleList list = _listService.Get(dto.ListId);

            CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
            var message = _localizer["RemovedShareNotification", IdentityHelper.GetUserName(User), list.Name];

            var createNotificationDto = new CreateOrUpdateNotification(user.Id, dto.UserId, null, null, message);
            var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
            var toDoAssistantPushNotification = new ToDoAssistantPushNotification
            {
                SenderImageUri = currentUser.ImageUri,
                UserId = user.Id,
                Message = message,
                OpenUrl = GetNotificationsPageUrl(notificationId)
            };

            _senderService.Enqueue(toDoAssistantPushNotification);
        }

        await _listService.ShareAsync(dto, _shareValidator);

        return NoContent();
    }

    [HttpDelete("{id}/leave")]
    public async Task<IActionResult> Leave(int id)
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

        LeaveListResult result = await _listService.LeaveAsync(id, userId);

        foreach (var recipient in result.NotificationRecipients)
        {
            CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
            var message = _localizer["LeftListNotification", IdentityHelper.GetUserName(User), result.ListName];

            var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, userId, id, null, message);
            var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
            var toDoAssistantPushNotification = new ToDoAssistantPushNotification
            {
                SenderImageUri = result.ActionUserImageUri,
                UserId = recipient.Id,
                Message = message,
                OpenUrl = GetNotificationsPageUrl(notificationId)
            };

            _senderService.Enqueue(toDoAssistantPushNotification);
        }

        return NoContent();
    }

    [HttpPost("copy")]
    public async Task<IActionResult> Copy([FromBody] CopyList dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        try
        {
            dto.UserId = IdentityHelper.GetUserId(User);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }

        int id = await _listService.CopyAsync(dto, _copyValidator);

        return StatusCode(201, id);
    }

    [HttpPut("is-archived")]
    public async Task<IActionResult> SetIsArchived([FromBody] SetIsArchivedDto dto)
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

        await _listService.SetIsArchivedAsync(dto.ListId, userId, dto.IsArchived);

        return NoContent();
    }

    [HttpPut("set-tasks-as-not-completed")]
    public async Task<IActionResult> SetTasksAsNotCompleted([FromBody] SetTasksAsNotCompletedDto dto)
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

        SetTasksAsNotCompletedResult result = await _listService.SetTasksAsNotCompletedAsync(dto.ListId, userId);

        foreach (var recipient in result.NotificationRecipients)
        {
            CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
            var message = _localizer["UncompletedAllTasksNotification", IdentityHelper.GetUserName(User), result.ListName];

            var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, userId, dto.ListId, null, message);
            var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
            var toDoAssistantPushNotification = new ToDoAssistantPushNotification
            {
                SenderImageUri = result.ActionUserImageUri,
                UserId = recipient.Id,
                Message = message,
                OpenUrl = GetNotificationsPageUrl(notificationId)
            };

            _senderService.Enqueue(toDoAssistantPushNotification);
        }

        return NoContent();
    }

    [HttpPut("share-is-accepted")]
    public async Task<IActionResult> SetShareIsAccepted([FromBody] SetShareIsAcceptedDto dto)
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

        SetShareIsAcceptedResult result = await _listService.SetShareIsAcceptedAsync(dto.ListId, userId, dto.IsAccepted);
        if (!result.Notify())
        {
            return NoContent();
        }

        var localizerKey = dto.IsAccepted ? "JoinedListNotification" : "DeclinedShareRequestNotification";
        foreach (var recipient in result.NotificationRecipients)
        {
            CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
            var message = _localizer[localizerKey, IdentityHelper.GetUserName(User), result.ListName];

            var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, userId, dto.ListId, null, message);
            var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
            var toDoAssistantPushNotification = new ToDoAssistantPushNotification
            {
                SenderImageUri = result.ActionUserImageUri,
                UserId = recipient.Id,
                Message = message,
                OpenUrl = GetNotificationsPageUrl(notificationId)
            };

            _senderService.Enqueue(toDoAssistantPushNotification);
        }

        return NoContent();
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder([FromBody] ReorderListDto dto)
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

        await _listService.ReorderAsync(dto.Id, userId, dto.OldOrder, dto.NewOrder);

        return NoContent();
    }

    private string GetNotificationsPageUrl(int notificationId)
    {
        return $"{_urls.ToDoAssistant}/notifications/{notificationId}";
    }
}
