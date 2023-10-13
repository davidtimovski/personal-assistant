﻿using System.Globalization;
using Api.Common;
using Core.Application.Contracts;
using Core.Application.Contracts.Models.Sender;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using ToDoAssistant.Api.Hubs;
using ToDoAssistant.Api.Models;
using ToDoAssistant.Api.Models.Tasks.Requests;
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
        ILogger<TasksController> logger) : base(userIdLookup, usersRepository)
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
        TaskDto? taskDto = _taskService.Get(id, UserId);
        if (taskDto is null)
        {
            return NotFound();
        }

        return Ok(taskDto);
    }

    [HttpGet("{id}/update")]
    public IActionResult GetForUpdate(int id)
    {
        TaskForUpdate? taskDto = _taskService.GetForUpdate(id, UserId);
        if (taskDto is null)
        {
            return NotFound();
        }

        return Ok(taskDto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/tasks",
            $"{nameof(TasksController)}.{nameof(Create)}",
            UserId
        );

        try
        {
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

            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["CreatedTaskNotification", result.ActionUserName, result.TaskName, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, request.ListId, result.TaskId, message);
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

            return StatusCode(201, result.TaskId);
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
                foreach (var recipient in result.NotificationRecipients)
                {
                    CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                    var message = _localizer["CreatedTaskNotification", result.ActionUserName, task.Name, result.ListName];

                    var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, request.ListId, task.Id, message);
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
        }
        finally
        {
            tr.Finish();
        }

        return StatusCode(201);
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
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["UpdatedTaskNotification", result.ActionUserName, result.OriginalTaskName, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, result.ListId, request.Id, message);
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

            foreach (var recipient in result.RemovedNotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["RemovedTaskNotification", result.ActionUserName, result.OriginalTaskName, result.OldListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, result.OldListId, null, message);
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

            foreach (var recipient in result.CreatedNotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["CreatedTaskNotification", result.ActionUserName, request.Name, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, result.ListId, request.Id, message);
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

            if (result.AssignedNotificationRecipient != null)
            {
                CultureInfo.CurrentCulture = new CultureInfo(result.AssignedNotificationRecipient.Language, false);
                var message = _localizer["AssignedTaskNotification", result.ActionUserName, result.OriginalTaskName, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(result.AssignedNotificationRecipient.Id, UserId, result.ListId, request.Id, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto, tr, cancellationToken);
                var toDoAssistantPushNotification = new ToDoAssistantPushNotification
                {
                    SenderImageUri = result.ActionUserImageUri,
                    UserId = result.AssignedNotificationRecipient.Id,
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

            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["RemovedTaskNotification", result.ActionUserName, result.TaskName, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, result.ListId, null, message);
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

            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                string message = _localizer["CompletedTaskNotification", result.ActionUserName, result.TaskName, result.ListName];

                var updateNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, result.ListId, request.Id, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(updateNotificationDto, tr, cancellationToken);
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

            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                string message = _localizer["UncompletedTaskNotification", result.ActionUserName, result.TaskName, result.ListName];

                var updateNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, result.ListId, request.Id, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(updateNotificationDto, tr, cancellationToken);
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

    private string GetNotificationsPageUrl(int notificationId)
    {
        return $"{_config.Url}/notifications/{notificationId}";
    }
}
