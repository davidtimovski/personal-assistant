using System.Globalization;
using Api.Common;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Sentry;
using ToDoAssistant.Api.Hubs;
using ToDoAssistant.Api.Models;
using ToDoAssistant.Api.Models.Tasks.Requests;
using ToDoAssistant.Application.Contracts;
using ToDoAssistant.Application.Contracts.Notifications;
using ToDoAssistant.Application.Contracts.Notifications.Models;
using ToDoAssistant.Application.Contracts.Tasks;
using ToDoAssistant.Application.Contracts.Tasks.Models;

namespace ToDoAssistant.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class TasksController : BaseController
{
    private readonly IHubContext<ListActionsHub> _listActionsHubContext;
    private readonly ITaskService _taskService;
    private readonly INotificationService _notificationService;
    private readonly ISenderService _senderService;
    private readonly IValidator<CreateTask> _createValidator;
    private readonly IValidator<BulkCreate> _bulkCreateValidator;
    private readonly IValidator<UpdateTask> _updateValidator;
    private readonly IStringLocalizer<TasksController> _localizer;
    private readonly AppConfiguration _config;
    private readonly ILogger<TasksController> _logger;

    public TasksController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        IHubContext<ListActionsHub> listActionsHubContext,
        ITaskService taskService,
        INotificationService notificationService,
        ISenderService senderService,
        IValidator<CreateTask> createValidator,
        IValidator<BulkCreate> bulkCreateValidator,
        IValidator<UpdateTask> updateValidator,
        IStringLocalizer<TasksController> localizer,
        IOptions<AppConfiguration> config,
        ILogger<TasksController> logger,
        IStringLocalizer<BaseController> baseLocalizer) : base(userIdLookup, usersRepository, baseLocalizer)
    {
        _listActionsHubContext = listActionsHubContext;
        _taskService = taskService;
        _notificationService = notificationService;
        _senderService = senderService;
        _createValidator = createValidator;
        _bulkCreateValidator = bulkCreateValidator;
        _updateValidator = updateValidator;
        _localizer = localizer;
        _config = config.Value;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var result = _taskService.Get(id, UserId);

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

    [HttpGet("{id}/update")]
    public IActionResult GetForUpdate(int id)
    {
        var result = _taskService.GetForUpdate(id, UserId);

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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/tasks",
            $"{nameof(TasksController)}.{nameof(Create)}",
            UserId
        );

        try
        {
            if (request is null)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return BadRequest();
            }

            var model = new CreateTask
            {
                UserId = UserId,
                ListId = request.ListId,
                Name = request.Name,
                Url = request.Url,
                IsOneTime = request.IsOneTime,
                IsPrivate = request.IsPrivate,
            };
            CreatedTaskResult result = await _taskService.CreateAsync(model, _createValidator, tr, cancellationToken);

            if (result.NotifySignalR)
            {
                await _listActionsHubContext.Clients.Group(result.ListId.ToString()).SendAsync("TasksModified", AuthId);
            }

            var message = _localizer["CreatedTaskNotification", result.ActionUserName, result.TaskName, result.ListName];

            var createResult = await CreateAndEnqueueNotificationsAsync(
                result.NotificationRecipients,
                result.ActionUserImageUri,
                request.ListId,
                result.TaskId,
                message,
                tr,
                cancellationToken);

            if (createResult.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            return StatusCode(201, result.TaskId);
        }
        catch
        {
            tr.Status = SpanStatus.InternalError;
            return StatusCode(500);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> BulkCreate([FromBody] BulkCreateRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/tasks/bulk",
            $"{nameof(TasksController)}.{nameof(BulkCreate)}",
            UserId
        );

        try
        {
            var model = new BulkCreate
            {
                UserId = UserId,
                ListId = request.ListId,
                TasksText = request.TasksText,
                TasksAreOneTime = request.TasksAreOneTime,
                TasksArePrivate = request.TasksArePrivate,
            };
            BulkCreateResult result = await _taskService.BulkCreateAsync(model, _bulkCreateValidator, tr, cancellationToken);

            if (result.NotifySignalR)
            {
                await _listActionsHubContext.Clients.Group(result.ListId.ToString()).SendAsync("TasksModified", AuthId);
            }

            if (!result.Notify())
            {
                return StatusCode(201);
            }

            foreach (BulkCreatedTask task in result.CreatedTasks)
            {
                var message = _localizer["CreatedTaskNotification", result.ActionUserName, task.Name, result.ListName];

                var createResult = await CreateAndEnqueueNotificationsAsync(
                    result.NotificationRecipients,
                    result.ActionUserImageUri,
                    request.ListId,
                    task.Id,
                    message,
                    tr,
                    cancellationToken);

                if (createResult.Failed)
                {
                    tr.Status = SpanStatus.InternalError;
                    return StatusCode(500);
                }
            }

            return StatusCode(201);
        }
        catch
        {
            tr.Status = SpanStatus.InternalError;
            return StatusCode(500);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/tasks",
            $"{nameof(TasksController)}.{nameof(Update)}",
            UserId
        );

        try
        {
            var model = new UpdateTask
            {
                Id = request.Id,
                UserId = UserId,
                ListId = request.ListId,
                Name = request.Name,
                Url = request.Url,
                IsOneTime = request.IsOneTime,
                IsHighPriority = request.IsHighPriority,
                IsPrivate = request.IsPrivate,
                AssignedToUserId = request.AssignedToUserId,
            };
            UpdateTaskResult result = await _taskService.UpdateAsync(model, _updateValidator, tr, cancellationToken);

            if (result.NotifySignalR)
            {
                await _listActionsHubContext.Clients.Group(result.ListId.ToString()).SendAsync("TasksModified", AuthId);
            }

            if (!result.Notify())
            {
                return NoContent();
            }

            foreach (var recipient in result.NotificationRecipients)
            {
                var message = _localizer["UpdatedTaskNotification", result.ActionUserName, result.OriginalTaskName!, result.ListName!];

                var createResult = await CreateAndEnqueueNotificationsAsync(
                    result.NotificationRecipients,
                    result.ActionUserImageUri,
                    request.ListId,
                    request.Id,
                    message,
                    tr,
                    cancellationToken);

                if (createResult.Failed)
                {
                    tr.Status = SpanStatus.InternalError;
                    return StatusCode(500);
                }
            }

            foreach (var recipient in result.RemovedNotificationRecipients)
            {
                var message = _localizer["RemovedTaskNotification", result.ActionUserName, result.OriginalTaskName!, result.OldListName!];

                var createResult = await CreateAndEnqueueNotificationsAsync(
                    result.RemovedNotificationRecipients,
                    result.ActionUserImageUri,
                    result.OldListId,
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

            foreach (var recipient in result.CreatedNotificationRecipients)
            {
                var message = _localizer["CreatedTaskNotification", result.ActionUserName, request.Name, result.ListName!];

                var createResult = await CreateAndEnqueueNotificationsAsync(
                    result.CreatedNotificationRecipients,
                    result.ActionUserImageUri,
                    result.ListId,
                    request.Id,
                    message,
                    tr,
                    cancellationToken);

                if (createResult.Failed)
                {
                    tr.Status = SpanStatus.InternalError;
                    return StatusCode(500);
                }
            }

            if (result.AssignedNotificationRecipient is not null)
            {
                var message = _localizer["AssignedTaskNotification", result.ActionUserName, result.OriginalTaskName!, result.ListName!];

                var createResult = await CreateAndEnqueueNotificationsAsync(
                    new List<NotificationRecipient> { new NotificationRecipient(result.AssignedNotificationRecipient.Id, result.AssignedNotificationRecipient.Language) },
                    result.ActionUserImageUri,
                    result.ListId,
                    request.Id,
                    message,
                    tr,
                    cancellationToken);

                if (createResult.Failed)
                {
                    tr.Status = SpanStatus.InternalError;
                    return StatusCode(500);
                }
            }

            return NoContent();
        }
        catch
        {
            tr.Status = SpanStatus.InternalError;
            return StatusCode(500);
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
            $"{Request.Method} api/tasks/{id}",
            $"{nameof(TasksController)}.{nameof(Delete)}",
            UserId
        );

        try
        {
            DeleteTaskResult result = await _taskService.DeleteAsync(id, UserId, tr, cancellationToken);

            if (result.NotifySignalR)
            {
                await _listActionsHubContext.Clients.Group(result.ListId.ToString()).SendAsync("TaskDeleted", AuthId, id, result.ListId);
            }

            var message = _localizer["RemovedTaskNotification", result.ActionUserName, result.TaskName, result.ListName];

            var createResult = await CreateAndEnqueueNotificationsAsync(
                result.NotificationRecipients,
                result.ActionUserImageUri,
                result.ListId,
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
        catch
        {
            tr.Status = SpanStatus.InternalError;
            return StatusCode(500);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPut("complete")]
    public async Task<IActionResult> Complete([FromBody] CompleteUncompleteRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/tasks/complete",
            $"{nameof(TasksController)}.{nameof(Complete)}",
            UserId
        );

        try
        {
            var model = new CompleteUncomplete
            {
                Id = request.Id,
                UserId = UserId
            };
            CompleteUncompleteTaskResult result = await _taskService.CompleteAsync(model, tr, cancellationToken);

            if (result.NotifySignalR)
            {
                await _listActionsHubContext.Clients.Group(result.ListId.ToString()).SendAsync("TaskCompletedChanged", AuthId, request.Id, result.ListId, true);
            }

            var message = _localizer["CompletedTaskNotification", result.ActionUserName, result.TaskName, result.ListName];

            var createResult = await CreateAndEnqueueNotificationsAsync(
                result.NotificationRecipients,
                result.ActionUserImageUri,
                result.ListId,
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
        catch
        {
            tr.Status = SpanStatus.InternalError;
            return StatusCode(500);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPut("uncomplete")]
    public async Task<IActionResult> Uncomplete([FromBody] CompleteUncompleteRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/tasks/uncomplete",
            $"{nameof(TasksController)}.{nameof(Uncomplete)}",
            UserId
        );

        try
        {
            var model = new CompleteUncomplete
            {
                Id = request.Id,
                UserId = UserId
            };
            CompleteUncompleteTaskResult result = await _taskService.UncompleteAsync(model, tr, cancellationToken);

            if (result.NotifySignalR)
            {
                await _listActionsHubContext.Clients.Group(result.ListId.ToString()).SendAsync("TaskCompletedChanged", AuthId, request.Id, result.ListId, false);
            }

            var message = _localizer["UncompletedTaskNotification", result.ActionUserName, result.TaskName, result.ListName];

            var createResult = await CreateAndEnqueueNotificationsAsync(
                result.NotificationRecipients,
                result.ActionUserImageUri,
                result.ListId,
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
        catch
        {
            tr.Status = SpanStatus.InternalError;
            return StatusCode(500);
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
        int? taskId,
        string message,
        ITransaction tr,
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

            _senderService.Enqueue(toDoAssistantPushNotification);
        }

        return new(true);
    }

    //[HttpPut("reorder")]
    //public async Task<IActionResult> Reorder([FromBody] ReorderTaskRequest request, CancellationToken cancellationToken)
    //{
    //    if (request is null)
    //    {
    //        return BadRequest();
    //    }

    //    ReorderTaskResult result = await _taskService.ReorderAsync(request);

    //    try
    //    {
    //        if (result.NotifySignalR)
    //        {
    //            await _hubContext.Clients.Group(result.ListId.ToString()).SendAsync("TaskReordered", UserId, request.Id, result.ListId, request.OldOrder, request.NewOrder);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, $"Unexpected error in {nameof(Reorder)}");
    //        throw;
    //    }

    //    return NoContent();
    //}
}
