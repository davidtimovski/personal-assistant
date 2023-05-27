﻿using System.Globalization;
using Api.Common;
using Core.Application.Contracts;
using Core.Application.Contracts.Models.Sender;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ToDoAssistant.Api.Models;
using ToDoAssistant.Api.Models.Lists;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Contracts.Lists.Models;
using ToDoAssistant.Application.Contracts.Notifications;
using ToDoAssistant.Application.Contracts.Notifications.Models;

namespace ToDoAssistant.Api.Controllers;

[Authorize]
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
    private readonly string _url;
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
        IConfiguration configuration,
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
        _url = configuration["Url"];
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists",
            $"{nameof(ListsController)}.{nameof(GetAll)}",
            UserId
        );

        IEnumerable<ListDto> lists = _listService.GetAll(UserId, tr);

        tr.Finish();

        return Ok(lists);
    }

    [HttpGet("options")]
    public IActionResult GetAllAsOptions()
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/options",
            $"{nameof(ListsController)}.{nameof(GetAllAsOptions)}",
            UserId
        );

        IEnumerable<ToDoListOption> options = _listService.GetAllAsOptions(UserId, tr);

        tr.Finish();

        return Ok(options);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/{id}",
            $"{nameof(ListsController)}.{nameof(Get)}",
            UserId
        );

        EditListDto list = _listService.GetForEdit(id, UserId, tr);

        tr.Finish();

        return list == null ? NotFound() : Ok(list);
    }

    [HttpGet("{id}/with-shares")]
    public IActionResult GetWithShares(int id)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/{id}/with-shares",
            $"{nameof(ListsController)}.{nameof(GetWithShares)}",
            UserId
        );

        ListWithShares list = _listService.GetWithShares(id, UserId, tr);

        tr.Finish();

        return list == null ? NotFound() : Ok(list);
    }

    [HttpGet("share-requests")]
    public IActionResult GetShareRequests()
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/share-requests",
            $"{nameof(ListsController)}.{nameof(GetShareRequests)}",
            UserId
        );

        IEnumerable<ShareListRequest> shareRequests = _listService.GetShareRequests(UserId, tr);

        tr.Finish();

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
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/{id}/members",
            $"{nameof(ListsController)}.{nameof(GetMembersAsAssigneeOptions)}",
            UserId
        );

        var assigneeOptions = _listService.GetMembersAsAssigneeOptions(id, UserId, tr);

        tr.Finish();

        return Ok(assigneeOptions);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateList dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists",
            $"{nameof(ListsController)}.{nameof(Create)}",
            UserId
        );

        dto.UserId = UserId;

        int id = await _listService.CreateAsync(dto, _createValidator, tr);

        tr.Finish();

        return StatusCode(201, id);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateList dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists",
            $"{nameof(ListsController)}.{nameof(Update)}",
            UserId
        );

        dto.UserId = UserId;

        UpdateListResult result = await _listService.UpdateAsync(dto, _updateValidator, tr);
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
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto, tr);
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
        finally
        {
            tr.Finish();
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

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/shared",
            $"{nameof(ListsController)}.{nameof(UpdateShared)}",
            UserId
        );

        dto.UserId = UserId;

        await _listService.UpdateSharedAsync(dto, _updateSharedValidator, tr);

        tr.Finish();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/{id}",
            $"{nameof(ListsController)}.{nameof(Delete)}",
            UserId
        );

        DeleteListResult result = await _listService.DeleteAsync(id, UserId, tr);

        try
        {
            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["DeletedListNotification", result.ActionUserName, result.DeletedListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, null, null, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto, tr);
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
        finally
        {
            tr.Finish();
        }

        return NoContent();
    }

    [HttpGet("can-share-with-user/{email}")]
    public IActionResult CanShareListWithUser(string email)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/can-share-with-user/{email}",
            $"{nameof(ListsController)}.{nameof(CanShareListWithUser)}",
            UserId
        );

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

        tr.Finish();

        return Ok(canShareVm);
    }

    [HttpPut("share")]
    public async Task<IActionResult> Share([FromBody] ShareList dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/share",
            $"{nameof(ListsController)}.{nameof(Share)}",
            UserId
        );

        dto.UserId = UserId;

        try
        {
            foreach (ShareUserAndPermission removedShare in dto.RemovedShares)
            {
                if (!_listService.CheckIfUserCanBeNotifiedOfChange(dto.ListId, removedShare.UserId, tr))
                {
                    continue;
                }

                var currentUser = _userService.Get(dto.UserId);
                var user = _userService.Get(removedShare.UserId);
                SimpleList list = _listService.Get(dto.ListId);

                CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                var message = _localizer["RemovedShareNotification", currentUser.Name, list.Name];

                var createNotificationDto = new CreateOrUpdateNotification(user.Id, dto.UserId, null, null, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto, tr);
                var toDoAssistantPushNotification = new ToDoAssistantPushNotification
                {
                    SenderImageUri = currentUser.ImageUri,
                    UserId = user.Id,
                    Message = message,
                    OpenUrl = GetNotificationsPageUrl(notificationId)
                };

                _senderService.Enqueue(toDoAssistantPushNotification);
            }

            await _listService.ShareAsync(dto, _shareValidator, tr);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Share)}");
            throw;
        }
        finally
        {
            tr.Finish();
        }

        return NoContent();
    }

    [HttpDelete("{id}/leave")]
    public async Task<IActionResult> Leave(int id)
    {
        var tr = Metrics.StartTransactionWithUser(
            "DELET api/lists/{id}/leave",
            $"{nameof(ListsController)}.{nameof(Leave)}",
            UserId
        );

        LeaveListResult result = await _listService.LeaveAsync(id, UserId, tr);

        try
        {
            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["LeftListNotification", result.ActionUserName, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, id, null, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto, tr);
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
        finally
        {
            tr.Finish();
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

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/copy",
            $"{nameof(ListsController)}.{nameof(Copy)}",
            UserId
        );

        dto.UserId = UserId;

        int id = await _listService.CopyAsync(dto, _copyValidator, tr);

        tr.Finish();

        return StatusCode(201, id);
    }

    [HttpPut("is-archived")]
    public async Task<IActionResult> SetIsArchived([FromBody] SetIsArchivedDto dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/is-archived",
            $"{nameof(ListsController)}.{nameof(SetIsArchived)}",
            UserId
        );

        await _listService.SetIsArchivedAsync(dto.ListId, UserId, dto.IsArchived, tr);

        tr.Finish();

        return NoContent();
    }

    [HttpPut("uncomplete-all")]
    public async Task<IActionResult> UncompleteAll([FromBody] SetTasksAsNotCompletedDto dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/uncomplete-all",
            $"{nameof(ListsController)}.{nameof(UncompleteAll)}",
            UserId
        );

        SetTasksAsNotCompletedResult result = await _listService.UncompleteAllAsync(dto.ListId, UserId, tr);

        try
        {
            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["UncompletedAllTasksNotification", result.ActionUserName, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, dto.ListId, null, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto, tr);
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
        finally
        {
            tr.Finish();
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

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/share-is-accepted",
            $"{nameof(ListsController)}.{nameof(SetShareIsAccepted)}",
            UserId
        );

        SetShareIsAcceptedResult result = await _listService.SetShareIsAcceptedAsync(dto.ListId, UserId, dto.IsAccepted, tr);
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
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto, tr);
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
        finally
        {
            tr.Finish();
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
        return $"{_url}/notifications/{notificationId}";
    }
}