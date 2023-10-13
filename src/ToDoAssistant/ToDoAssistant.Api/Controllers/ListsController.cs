using System.Globalization;
using Api.Common;
using Core.Application.Contracts;
using Core.Application.Contracts.Models.Sender;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using ToDoAssistant.Api.Models;
using ToDoAssistant.Api.Models.Lists.Requests;
using ToDoAssistant.Api.Models.Lists.Responses;
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
    private readonly AppConfiguration _config;
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
        IOptions<AppConfiguration> config,
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
        _config = config.Value;
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

        try
        {
            IEnumerable<ListDto> lists = _listService.GetAll(UserId, tr);

            return Ok(lists);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpGet("options")]
    public IActionResult GetAllAsOptions()
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/options",
            $"{nameof(ListsController)}.{nameof(GetAllAsOptions)}",
            UserId
        );

        try
        {
            IEnumerable<ToDoListOption> options = _listService.GetAllAsOptions(UserId, tr);

            return Ok(options);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/{id}",
            $"{nameof(ListsController)}.{nameof(Get)}",
            UserId
        );

        try
        {
            var list = _listService.GetForEdit(id, UserId, tr);

            return list is null ? NotFound() : Ok(list);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpGet("{id}/with-shares")]
    public IActionResult GetWithShares(int id)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/{id}/with-shares",
            $"{nameof(ListsController)}.{nameof(GetWithShares)}",
            UserId
        );

        try
        {
            var list = _listService.GetWithShares(id, UserId, tr);

            return list is null ? NotFound() : Ok(list);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpGet("share-requests")]
    public IActionResult GetShareRequests()
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/share-requests",
            $"{nameof(ListsController)}.{nameof(GetShareRequests)}",
            UserId
        );

        try
        {
            IEnumerable<Application.Contracts.Lists.Models.ShareListRequest> shareRequests = _listService.GetShareRequests(UserId, tr);

            return Ok(shareRequests);
        }
        finally
        {
            tr.Finish();
        }
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

        try
        {
            var assigneeOptions = _listService.GetMembersAsAssigneeOptions(id, UserId, tr);

            return Ok(assigneeOptions);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateListRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists",
            $"{nameof(ListsController)}.{nameof(Create)}",
            UserId
        );

        try
        {
            var model = new CreateList
            {
                UserId = UserId,
                Name = request.Name,
                Icon = request.Icon,
                IsOneTimeToggleDefault = request.IsOneTimeToggleDefault,
                TasksText = request.TasksText,
            };
            int id = await _listService.CreateAsync(model, _createValidator, tr, cancellationToken);

            return StatusCode(201, id);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateListRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists",
            $"{nameof(ListsController)}.{nameof(Update)}",
            UserId
        );

        try
        {
            var model = new UpdateList
            {
                Id = request.Id,
                UserId = UserId,
                Name = request.Name,
                Icon = request.Icon,
                IsOneTimeToggleDefault = request.IsOneTimeToggleDefault,
                NotificationsEnabled = request.NotificationsEnabled,
            };
            UpdateListResult result = await _listService.UpdateAsync(model, _updateValidator, tr, cancellationToken);
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
                var message = _localizer[resourceKey, result.ActionUserName, result.OriginalListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, request.Id, null, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto, tr, cancellationToken);
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
        finally
        {
            tr.Finish();
        }

        return NoContent();
    }

    [HttpPut("shared")]
    public async Task<IActionResult> UpdateShared([FromBody] UpdateSharedListRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/shared",
            $"{nameof(ListsController)}.{nameof(UpdateShared)}",
            UserId
        );

        try
        {
            var model = new UpdateSharedList
            {
                Id = request.Id,
                UserId = UserId,
                NotificationsEnabled = request.NotificationsEnabled
            };
            await _listService.UpdateSharedAsync(model, _updateSharedValidator, tr, cancellationToken);
        }
        finally
        {
            tr.Finish();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/{id}",
            $"{nameof(ListsController)}.{nameof(Delete)}",
            UserId
        );

        try
        {
            DeleteListResult result = await _listService.DeleteAsync(id, UserId, tr, cancellationToken);

            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["DeletedListNotification", result.ActionUserName, result.DeletedListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, null, null, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto, tr, cancellationToken);
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

        try
        {
            var response = new CanShareResponse
            {
                CanShare = false
            };

            var user = _userService.Get(email);

            if (user != null)
            {
                response.UserId = user.Id;
                response.ImageUri = user.ImageUri;
                response.CanShare = _listService.CanShareWithUser(user.Id, UserId);
            }

            return Ok(response);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPut("share")]
    public async Task<IActionResult> Share([FromBody] Models.Lists.Requests.ShareListRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/share",
            $"{nameof(ListsController)}.{nameof(Share)}",
            UserId
        );

        try
        {
            foreach (Models.Lists.Requests.ShareUserAndPermission removedShare in request.RemovedShares)
            {
                if (!_listService.CheckIfUserCanBeNotifiedOfChange(request.ListId, removedShare.UserId, tr))
                {
                    continue;
                }

                var currentUser = _userService.Get(UserId);
                var user = _userService.Get(removedShare.UserId);
                SimpleList list = _listService.Get(request.ListId);

                CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                var message = _localizer["RemovedShareNotification", currentUser.Name, list.Name];

                var createNotificationDto = new CreateOrUpdateNotification(user.Id, UserId, null, null, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto, tr, cancellationToken);
                var toDoAssistantPushNotification = new ToDoAssistantPushNotification
                {
                    SenderImageUri = currentUser.ImageUri,
                    UserId = user.Id,
                    Message = message,
                    OpenUrl = GetNotificationsPageUrl(notificationId)
                };

                _senderService.Enqueue(toDoAssistantPushNotification);
            }

            var model = new ShareList
            {
                UserId = UserId,
                ListId = request.ListId,
                NewShares = request.NewShares.Select(x => new Application.Contracts.Lists.Models.ShareUserAndPermission { UserId = x.UserId, IsAdmin = x.IsAdmin }).ToList(),
                EditedShares = request.EditedShares.Select(x => new Application.Contracts.Lists.Models.ShareUserAndPermission { UserId = x.UserId, IsAdmin = x.IsAdmin }).ToList(),
                RemovedShares = request.RemovedShares.Select(x => new Application.Contracts.Lists.Models.ShareUserAndPermission { UserId = x.UserId, IsAdmin = x.IsAdmin }).ToList()
            };
            await _listService.ShareAsync(model, _shareValidator, tr, cancellationToken);
        }
        finally
        {
            tr.Finish();
        }

        return NoContent();
    }

    [HttpDelete("{id}/leave")]
    public async Task<IActionResult> Leave(int id, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            "DELETE api/lists/{id}/leave",
            $"{nameof(ListsController)}.{nameof(Leave)}",
            UserId
        );

        LeaveListResult result = await _listService.LeaveAsync(id, UserId, tr, cancellationToken);

        try
        {
            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["LeftListNotification", result.ActionUserName, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, id, null, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto, tr, cancellationToken);
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
        finally
        {
            tr.Finish();
        }

        return NoContent();
    }

    [HttpPost("copy")]
    public async Task<IActionResult> Copy([FromBody] CopyListRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/copy",
            $"{nameof(ListsController)}.{nameof(Copy)}",
            UserId
        );

        try
        {
            var model = new CopyList
            {
                Id = request.Id,
                UserId = UserId,
                Name = request.Name,
                Icon = request.Icon
            };
            int id = await _listService.CopyAsync(model, _copyValidator, tr, cancellationToken);

            return StatusCode(201, id);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPut("is-archived")]
    public async Task<IActionResult> SetIsArchived([FromBody] SetIsArchivedRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/is-archived",
            $"{nameof(ListsController)}.{nameof(SetIsArchived)}",
            UserId
        );

        try
        {
            await _listService.SetIsArchivedAsync(request.ListId, UserId, request.IsArchived, tr, cancellationToken);
        }
        finally
        {
            tr.Finish();
        }

        return NoContent();
    }

    [HttpPut("uncomplete-all")]
    public async Task<IActionResult> UncompleteAll([FromBody] SetTasksAsNotCompletedRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/uncomplete-all",
            $"{nameof(ListsController)}.{nameof(UncompleteAll)}",
            UserId
        );

        SetTasksAsNotCompletedResult result = await _listService.UncompleteAllAsync(request.ListId, UserId, tr, cancellationToken);

        try
        {
            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["UncompletedAllTasksNotification", result.ActionUserName, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, request.ListId, null, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto, tr, cancellationToken);
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
        finally
        {
            tr.Finish();
        }

        return NoContent();
    }

    [HttpPut("share-is-accepted")]
    public async Task<IActionResult> SetShareIsAccepted([FromBody] SetShareIsAcceptedRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/share-is-accepted",
            $"{nameof(ListsController)}.{nameof(SetShareIsAccepted)}",
            UserId
        );

        try
        {
            SetShareIsAcceptedResult result = await _listService.SetShareIsAcceptedAsync(request.ListId, UserId, request.IsAccepted, tr, cancellationToken);
            if (!result.Notify())
            {
                return NoContent();
            }

            var localizerKey = request.IsAccepted ? "JoinedListNotification" : "DeclinedShareRequestNotification";
            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer[localizerKey, result.ActionUserName, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, request.ListId, null, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto, tr, cancellationToken);
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
        finally
        {
            tr.Finish();
        }

        return NoContent();
    }

    //[HttpPut("reorder")]
    //public async Task<IActionResult> Reorder([FromBody] ReorderListRequest request, CancellationToken cancellationToken)
    //{
    //    if (request is null)
    //    {
    //        return BadRequest();
    //    }

    //    await _listService.ReorderAsync(request.Id, UserId, request.OldOrder, request.NewOrder);

    //    return NoContent();
    //}

    private string GetNotificationsPageUrl(int notificationId)
    {
        return $"{_config.Url}/notifications/{notificationId}";
    }
}
