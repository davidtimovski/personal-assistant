using System.Globalization;
using Api.Config;
using Api.Models;
using Api.Models.ToDoAssistant.Lists;
using Application.Contracts;
using FluentValidation;
using Infrastructure.Sender.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Contracts.Lists.Models;
using ToDoAssistant.Application.Contracts.Notifications;
using ToDoAssistant.Application.Contracts.Notifications.Models;

namespace Api.Controllers.ToDoAssistant;

[Authorize]
[EnableCors("AllowToDoAssistant")]
[Route("api/[controller]")]
public class ListsController : BaseController
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
    private readonly ILogger<ListsController> _logger;

    public ListsController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
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
        IOptions<Urls> urls,
        ILogger<ListsController> logger) : base(userIdLookup, usersRepository)
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
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        IEnumerable<ListDto> lists = _listService.GetAll(UserId);

        return Ok(lists);
    }

    [HttpGet("options")]
    public IActionResult GetAllAsOptions()
    {
        IEnumerable<ToDoListOption> options = _listService.GetAllAsOptions(UserId);

        return Ok(options);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        EditListDto list = _listService.GetForEdit(id, UserId);
        if (list == null)
        {
            return NotFound();
        }

        return Ok(list);
    }

    [HttpGet("{id}/with-shares")]
    public IActionResult GetWithShares(int id)
    {
        ListWithShares list = _listService.GetWithShares(id, UserId);
        if (list == null)
        {
            return NotFound();
        }

        return Ok(list);
    }

    [HttpGet("share-requests")]
    public IActionResult GetShareRequests()
    {
        IEnumerable<ShareListRequest> shareRequests = _listService.GetShareRequests(UserId);

        return Ok(shareRequests);
    }

    [HttpGet("pending-share-requests-count")]
    public IActionResult GetPendingShareRequestsCount()
    {
        int pendingShareRequestsCount = _listService.GetPendingShareRequestsCount(UserId);

        return Ok(pendingShareRequestsCount);
    }

    [HttpGet("{id}/shared")]
    public IActionResult GetIsShared(int id)
    {
        bool isShared = _listService.IsShared(id, UserId);

        return Ok(isShared);
    }

    [HttpGet("{id}/members")]
    public IActionResult GetMembersAsAssigneeOptions(int id)
    {
        var assigneeOptions = _listService.GetMembersAsAssigneeOptions(id, UserId);

        return Ok(assigneeOptions);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateList dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        dto.UserId = UserId;

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

        dto.UserId = UserId;

        UpdateListResult result = await _listService.UpdateAsync(dto, _updateValidator);
        if (!result.Notify())
        {
            return NoContent();
        }

        try
        {
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
                var message = _localizer[resourceKey, result.ActionUserName, result.OriginalListName];

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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Update)}");
            throw;
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

        dto.UserId = UserId;

        await _listService.UpdateSharedAsync(dto, _updateSharedValidator);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        DeleteListResult result = await _listService.DeleteAsync(id, UserId);

        try
        {
            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["DeletedListNotification", result.ActionUserName, result.DeletedListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, null, null, message);
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Delete)}");
            throw;
        }

        return NoContent();
    }

    [HttpGet("can-share-with-user/{email}")]
    public IActionResult CanShareListWithUser(string email)
    {
        var canShareVm = new CanShareVm
        {
            CanShare = false
        };

        var user = _userService.Get(email);

        if (user != null)
        {
            canShareVm.UserId = user.Id;
            canShareVm.ImageUri = user.ImageUri;
            canShareVm.CanShare = _listService.CanShareWithUser(user.Id, UserId);
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

        dto.UserId = UserId;

        try
        {
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
                var message = _localizer["RemovedShareNotification", currentUser.Name, list.Name];

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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Share)}");
            throw;
        }

        await _listService.ShareAsync(dto, _shareValidator);

        return NoContent();
    }

    [HttpDelete("{id}/leave")]
    public async Task<IActionResult> Leave(int id)
    {
        LeaveListResult result = await _listService.LeaveAsync(id, UserId);

        try
        {
            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["LeftListNotification", result.ActionUserName, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, id, null, message);
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Leave)}");
            throw;
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

        dto.UserId = UserId;

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

        await _listService.SetIsArchivedAsync(dto.ListId, UserId, dto.IsArchived);

        return NoContent();
    }

    [HttpPut("uncomplete-all")]
    public async Task<IActionResult> UncompleteAll([FromBody] SetTasksAsNotCompletedDto dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        SetTasksAsNotCompletedResult result = await _listService.UncompleteAllAsync(dto.ListId, UserId);

        try
        {
            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["UncompletedAllTasksNotification", result.ActionUserName, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, dto.ListId, null, message);
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UncompleteAll)}");
            throw;
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

        SetShareIsAcceptedResult result = await _listService.SetShareIsAcceptedAsync(dto.ListId, UserId, dto.IsAccepted);
        if (!result.Notify())
        {
            return NoContent();
        }

        try
        {
            var localizerKey = dto.IsAccepted ? "JoinedListNotification" : "DeclinedShareRequestNotification";
            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer[localizerKey, result.ActionUserName, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, dto.ListId, null, message);
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(SetShareIsAccepted)}");
            throw;
        }

        return NoContent();
    }

    //[HttpPut("reorder")]
    //public async Task<IActionResult> Reorder([FromBody] ReorderListDto dto)
    //{
    //    if (dto == null)
    //    {
    //        return BadRequest();
    //    }

    //    await _listService.ReorderAsync(dto.Id, UserId, dto.OldOrder, dto.NewOrder);

    //    return NoContent();
    //}

    private string GetNotificationsPageUrl(int notificationId)
    {
        return $"{_urls.ToDoAssistant}/notifications/{notificationId}";
    }
}
