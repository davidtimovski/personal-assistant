using System.Globalization;
using Api.Common;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using Core.Application.Utils;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using ToDoAssistant.Api.Models;
using ToDoAssistant.Api.Models.Lists.Requests;
using ToDoAssistant.Api.Models.Lists.Responses;
using ToDoAssistant.Application.Contracts;
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
        IUserIdLookup? userIdLookup,
        IUsersRepository? usersRepository,
        IListService? listService,
        INotificationService? notificationService,
        ISenderService? senderService,
        IUserService? userService,
        IValidator<CreateList>? createValidator,
        IValidator<UpdateList>? updateValidator,
        IValidator<UpdateSharedList>? updateSharedValidator,
        IValidator<ShareList>? shareValidator,
        IValidator<CopyList>? copyValidator,
        IStringLocalizer<ListsController>? localizer,
        IOptions<AppConfiguration>? config,
        ILogger<ListsController>? logger,
        IStringLocalizer<BaseController>? baseLocalizer) : base(userIdLookup, usersRepository, baseLocalizer)
    {
        _listService = ArgValidator.NotNull(listService);
        _notificationService = ArgValidator.NotNull(notificationService);
        _senderService = ArgValidator.NotNull(senderService);
        _userService = ArgValidator.NotNull(userService);
        _createValidator = ArgValidator.NotNull(createValidator);
        _updateValidator = ArgValidator.NotNull(updateValidator);
        _updateSharedValidator = ArgValidator.NotNull(updateSharedValidator);
        _shareValidator = ArgValidator.NotNull(shareValidator);
        _copyValidator = ArgValidator.NotNull(copyValidator);
        _localizer = ArgValidator.NotNull(localizer);
        _config = ArgValidator.NotNull(config).Value;
        _logger = ArgValidator.NotNull(logger);
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
            var result = _listService.GetAll(UserId, tr);

            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            return Ok(result.Data);
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
            var result = _listService.GetAllAsOptions(UserId, tr);

            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            return Ok(result.Data);
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
            var result = _listService.GetForEdit(id, UserId, tr);

            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            if (result.Data is null)
            {
                tr.Status = SpanStatus.NotFound;
                return NotFound();
            }

            return Ok(result.Data);
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
            var result = _listService.GetWithShares(id, UserId, tr);

            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            if (result.Data is null)
            {
                tr.Status = SpanStatus.NotFound;
                return NotFound();
            }

            return Ok(result.Data);
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
            var result = _listService.GetShareRequests(UserId, tr);

            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            return Ok(result.Data);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpGet("pending-share-requests-count")]
    public IActionResult GetPendingShareRequestsCount()
    {
        var result = _listService.GetPendingShareRequestsCount(UserId);

        if (result.Failed)
        {
            return StatusCode(500);
        }

        return Ok(result.Data);
    }

    [HttpGet("{id}/shared")]
    public IActionResult GetIsShared(int id)
    {
        var result = _listService.IsShared(id, UserId);

        if (result.Failed)
        {
            return StatusCode(500);
        }

        if (result.Data is null)
        {
            return NotFound();
        }

        return Ok(result.Data);
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
            var result = _listService.GetMembersAsAssigneeOptions(id, UserId, tr);

            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            if (result.Data is null)
            {
                tr.Status = SpanStatus.NotFound;
                return NotFound();
            }

            return Ok(result.Data);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateListRequest request, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists",
            $"{nameof(ListsController)}.{nameof(Create)}",
            UserId
        );

        try
        {
            if (request is null)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return BadRequest();
            }

            var model = new CreateList
            {
                UserId = UserId,
                Name = request.Name,
                Icon = request.Icon,
                IsOneTimeToggleDefault = request.IsOneTimeToggleDefault,
                TasksText = request.TasksText,
            };
            var result = await _listService.CreateAsync(model, _createValidator, tr, cancellationToken);

            if (result.Status == ResultStatus.Error)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            if (result.Status == ResultStatus.Invalid)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return UnprocessableEntityResult(result.ValidationErrors!);
            }

            return StatusCode(201, result.Data);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateListRequest request, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists",
            $"{nameof(ListsController)}.{nameof(Update)}",
            UserId
        );

        try
        {
            if (request is null)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return BadRequest();
            }

            var model = new UpdateList
            {
                Id = request.Id,
                UserId = UserId,
                Name = request.Name,
                Icon = request.Icon,
                IsOneTimeToggleDefault = request.IsOneTimeToggleDefault,
                NotificationsEnabled = request.NotificationsEnabled,
            };
            var result = await _listService.UpdateAsync(model, _updateValidator, tr, cancellationToken);

            if (result.Status == ResultStatus.Error)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            if (result.Status == ResultStatus.Invalid)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return UnprocessableEntityResult(result.ValidationErrors!);
            }

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

            var message = _localizer[resourceKey, result.ActionUserName, result.OriginalListName];

            var createResult = await CreateAndEnqueueNotificationsAsync(
                result.NotificationRecipients,
                result.ActionUserImageUri,
                request.Id,
                message,
                tr,
                cancellationToken);

            if (createResult.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            return NoContent();
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPut("shared")]
    public async Task<IActionResult> UpdateShared([FromBody] UpdateSharedListRequest request, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/shared",
            $"{nameof(ListsController)}.{nameof(UpdateShared)}",
            UserId
        );

        try
        {
            if (request is null)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return BadRequest();
            }

            var model = new UpdateSharedList
            {
                Id = request.Id,
                UserId = UserId,
                NotificationsEnabled = request.NotificationsEnabled
            };
            var result = await _listService.UpdateSharedAsync(model, _updateSharedValidator, tr, cancellationToken);

            if (result.Status == ResultStatus.Invalid)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return UnprocessableEntityResult(result.ValidationErrors!);
            }

            return NoContent();
        }
        finally
        {
            tr.Finish();
        }
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
            var result = await _listService.DeleteAsync(id, UserId, tr, cancellationToken);

            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            var message = _localizer["DeletedListNotification", result.ActionUserName, result.DeletedListName];

            var createResult = await CreateAndEnqueueNotificationsAsync(
                result.NotificationRecipients,
                result.ActionUserImageUri,
                null,
                message,
                tr,
                cancellationToken);

            if (createResult.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            return NoContent();
        }
        finally
        {
            tr.Finish();
        }
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
            var userResult = _userService.Get(email);
            if (userResult.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            if (userResult.Data is null)
            {
                return Ok(new CanShareResponse(false));
            }

            var canShareResult = _listService.CanShareWithUser(userResult.Data.Id, UserId);
            if (canShareResult.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            var response = new CanShareResponse
            (
                canShare: canShareResult.Data,
                userId: userResult.Data.Id,
                imageUri: userResult.Data.ImageUri
            );

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
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/share",
            $"{nameof(ListsController)}.{nameof(Share)}",
            UserId
        );

        try
        {
            if (request is null)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return BadRequest();
            }

            foreach (Models.Lists.Requests.ShareUserAndPermission removedShare in request.RemovedShares)
            {
                var canBeNotifiedResult = _listService.CheckIfUserCanBeNotifiedOfChange(request.ListId, removedShare.UserId, tr);

                if (canBeNotifiedResult.Failed)
                {
                    tr.Status = SpanStatus.InternalError;
                    return StatusCode(500);
                }

                if (!canBeNotifiedResult.Data)
                {
                    continue;
                }

                var currentUser = _userService.Get(UserId);

                var currentUserResult = _userService.Get(UserId);
                if (currentUserResult.Failed)
                {
                    return StatusCode(500);
                }

                var userResult = _userService.Get(removedShare.UserId);
                if (userResult.Failed)
                {
                    return StatusCode(500);
                }

                var listResult = _listService.Get(request.ListId);

                if (listResult.Failed)
                {
                    tr.Status = SpanStatus.InternalError;
                    return StatusCode(500);
                }

                var message = _localizer["RemovedShareNotification", currentUserResult.Data!.Name, listResult.Data!.Name];

                var createResult = await CreateAndEnqueueNotificationsAsync(
                    new List<NotificationRecipient> { new NotificationRecipient(userResult.Data!.Id, userResult.Data.Language) },
                    currentUserResult.Data.ImageUri,
                    null,
                    message,
                    tr,
                    cancellationToken);

                if (createResult.Failed)
                {
                    tr.Status = SpanStatus.InternalError;
                    return StatusCode(500);
                }
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

            return NoContent();
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpDelete("{id}/leave")]
    public async Task<IActionResult> Leave(int id, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            "DELETE api/lists/{id}/leave",
            $"{nameof(ListsController)}.{nameof(Leave)}",
            UserId
        );

        try
        {
            var result = await _listService.LeaveAsync(id, UserId, tr, cancellationToken);

            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            var message = _localizer["LeftListNotification", result.ActionUserName, result.ListName];

            var createResult = await CreateAndEnqueueNotificationsAsync(
                result.NotificationRecipients,
                result.ActionUserImageUri,
                id,
                message,
                tr,
                cancellationToken);

            if (createResult.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            return NoContent();
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPost("copy")]
    public async Task<IActionResult> Copy([FromBody] CopyListRequest request, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/copy",
            $"{nameof(ListsController)}.{nameof(Copy)}",
            UserId
        );

        try
        {
            if (request is null)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return BadRequest();
            }

            var model = new CopyList
            {
                Id = request.Id,
                UserId = UserId,
                Name = request.Name,
                Icon = request.Icon
            };
            var result = await _listService.CopyAsync(model, _copyValidator, tr, cancellationToken);

            if (result.Status == ResultStatus.Error)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            if (result.Status == ResultStatus.Invalid)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return UnprocessableEntityResult(result.ValidationErrors!);
            }

            return StatusCode(201, result.Data);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPut("is-archived")]
    public async Task<IActionResult> SetIsArchived([FromBody] SetIsArchivedRequest request, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/is-archived",
            $"{nameof(ListsController)}.{nameof(SetIsArchived)}",
            UserId
        );

        try
        {
            if (request is null)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return BadRequest();
            }

            var result = await _listService.SetIsArchivedAsync(request.ListId, UserId, request.IsArchived, tr, cancellationToken);
            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            return NoContent();
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPut("uncomplete-all")]
    public async Task<IActionResult> UncompleteAll([FromBody] SetTasksAsNotCompletedRequest request, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/uncomplete-all",
            $"{nameof(ListsController)}.{nameof(UncompleteAll)}",
            UserId
        );

        try
        {
            if (request is null)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return BadRequest();
            }

            var result = await _listService.UncompleteAllAsync(request.ListId, UserId, tr, cancellationToken);

            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            if (result is null)
            {
                tr.Status = SpanStatus.NotFound;
                return NotFound();
            }

            var message = _localizer["UncompletedAllTasksNotification", result.ActionUserName, result.ListName];

            var createResult = await CreateAndEnqueueNotificationsAsync(
                result.NotificationRecipients,
                result.ActionUserImageUri,
                request.ListId,
                message,
                tr,
                cancellationToken);

            if (createResult.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            return NoContent();
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPut("share-is-accepted")]
    public async Task<IActionResult> SetShareIsAccepted([FromBody] SetShareIsAcceptedRequest request, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/lists/share-is-accepted",
            $"{nameof(ListsController)}.{nameof(SetShareIsAccepted)}",
            UserId
        );

        try
        {
            if (request is null)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return BadRequest();
            }

            var result = await _listService.SetShareIsAcceptedAsync(request.ListId, UserId, request.IsAccepted, tr, cancellationToken);

            if (result.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            if (!result.Notify())
            {
                return NoContent();
            }

            var localizerKey = request.IsAccepted ? "JoinedListNotification" : "DeclinedShareRequestNotification";

            var message = _localizer[localizerKey, result.ActionUserName, result.ListName];

            var createResult = await CreateAndEnqueueNotificationsAsync(
                result.NotificationRecipients,
                result.ActionUserImageUri,
                request.ListId,
                message,
                tr,
                cancellationToken);

            if (createResult.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            return NoContent();
        }
        finally
        {
            tr.Finish();
        }
    }

    private async Task<Result> CreateAndEnqueueNotificationsAsync(
        IReadOnlyList<NotificationRecipient> recipients,
        string actionUserImageUri,
        int? listId,
        string message,
        ITransactionTracer tr,
        CancellationToken cancellationToken)
    {
        foreach (var recipient in recipients)
        {
            CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
            var model = new CreateOrUpdateNotification(recipient.Id, UserId, listId, null, message);
            var result = await _notificationService.CreateOrUpdateAsync(model, tr, cancellationToken);

            if (result.Failed)
            {
                return new();
            }

            var toDoAssistantPushNotification = new ToDoAssistantPushNotification
            {
                SenderImageUri = actionUserImageUri,
                UserId = recipient.Id,
                Message = message,
                OpenUrl = $"{_config.Url}/notifications/{result.Data}"
            };

            await _senderService.EnqueueAsync(toDoAssistantPushNotification);
        }

        return new(true);
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
}
