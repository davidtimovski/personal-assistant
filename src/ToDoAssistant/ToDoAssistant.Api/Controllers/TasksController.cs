﻿using System.Globalization;
using Api.Common;
using Core.Application.Contracts;
using Core.Application.Contracts.Models.Sender;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using ToDoAssistant.Api.Hubs;
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
    private readonly ILogger<TasksController> _logger;
    private readonly string _url;

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
        ILogger<TasksController> logger,
        IConfiguration configuration) : base(userIdLookup, usersRepository)
    {
        _listActionsHubContext = listActionsHubContext;
        _taskService = taskService;
        _notificationService = notificationService;
        _senderService = senderService;
        _createValidator = createValidator;
        _bulkCreateValidator = bulkCreateValidator;
        _updateValidator = updateValidator;
        _localizer = localizer;
        _logger = logger;
        _url = configuration["Url"];
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        TaskDto taskDto = _taskService.Get(id, UserId);
        if (taskDto is null)
        {
            return NotFound();
        }

        return Ok(taskDto);
    }

    [HttpGet("{id}/update")]
    public IActionResult GetForUpdate(int id)
    {
        TaskForUpdate taskDto = _taskService.GetForUpdate(id, UserId);
        if (taskDto is null)
        {
            return NotFound();
        }

        return Ok(taskDto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTask dto)
    {
        if (dto is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/tasks",
            $"{nameof(TasksController)}.{nameof(Create)}",
            UserId
        );

        dto.UserId = UserId;

        CreatedTaskResult result = await _taskService.CreateAsync(dto, _createValidator, tr);

        try
        {
            if (result.NotifySignalR)
            {
                await _listActionsHubContext.Clients.Group(result.ListId.ToString()).SendAsync("TasksModified", AuthId);
            }

            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["CreatedTaskNotification", result.ActionUserName, result.TaskName, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, dto.UserId, dto.ListId, result.TaskId, message);
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
            _logger.LogError(ex, $"Unexpected error in {nameof(Create)}");
            throw;
        }
        finally
        {
            tr.Finish();
        }

        return StatusCode(201, result.TaskId);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> BulkCreate([FromBody] BulkCreate dto)
    {
        if (dto is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/tasks/bulk",
            $"{nameof(TasksController)}.{nameof(BulkCreate)}",
            UserId
        );

        dto.UserId = UserId;

        BulkCreateResult result = await _taskService.BulkCreateAsync(dto, _bulkCreateValidator, tr);

        try
        {
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

                    var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, dto.UserId, dto.ListId, task.Id, message);
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(BulkCreate)}");
            throw;
        }
        finally
        {
            tr.Finish();
        }

        return StatusCode(201);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateTask dto)
    {
        if (dto is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/tasks",
            $"{nameof(TasksController)}.{nameof(Update)}",
            UserId
        );

        dto.UserId = UserId;

        UpdateTaskResult result = await _taskService.UpdateAsync(dto, _updateValidator, tr);

        try
        {
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

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, dto.UserId, result.ListId, dto.Id, message);
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

            foreach (var recipient in result.RemovedNotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["RemovedTaskNotification", result.ActionUserName, result.OriginalTaskName, result.OldListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, dto.UserId, result.OldListId, null, message);
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

            foreach (var recipient in result.CreatedNotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["CreatedTaskNotification", result.ActionUserName, dto.Name, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, dto.UserId, result.ListId, dto.Id, message);
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

            if (result.AssignedNotificationRecipient != null)
            {
                CultureInfo.CurrentCulture = new CultureInfo(result.AssignedNotificationRecipient.Language, false);
                var message = _localizer["AssignedTaskNotification", result.ActionUserName, result.OriginalTaskName, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(result.AssignedNotificationRecipient.Id, dto.UserId, result.ListId, dto.Id, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto, tr);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/tasks/{id}",
            $"{nameof(TasksController)}.{nameof(Delete)}",
            UserId
        );

        try
        {
            DeleteTaskResult result = await _taskService.DeleteAsync(id, UserId, tr);

            if (result.NotifySignalR)
            {
                await _listActionsHubContext.Clients.Group(result.ListId.ToString()).SendAsync("TaskDeleted", AuthId, id, result.ListId);
            }

            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["RemovedTaskNotification", result.ActionUserName, result.TaskName, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, UserId, result.ListId, null, message);
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

    [HttpPut("complete")]
    public async Task<IActionResult> Complete([FromBody] CompleteUncomplete dto)
    {
        if (dto is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/tasks/complete",
            $"{nameof(TasksController)}.{nameof(Complete)}",
            UserId
        );

        dto.UserId = UserId;

        try
        {
            CompleteUncompleteTaskResult result = await _taskService.CompleteAsync(dto, tr);

            if (result.NotifySignalR)
            {
                await _listActionsHubContext.Clients.Group(result.ListId.ToString()).SendAsync("TaskCompletedChanged", AuthId, dto.Id, result.ListId, true);
            }

            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                string message = _localizer["CompletedTaskNotification", result.ActionUserName, result.TaskName, result.ListName];

                var updateNotificationDto = new CreateOrUpdateNotification(recipient.Id, dto.UserId, result.ListId, dto.Id, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(updateNotificationDto, tr);
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
            _logger.LogError(ex, $"Unexpected error in {nameof(Complete)}");
            throw;
        }
        finally
        {
            tr.Finish();
        }

        return NoContent();
    }

    [HttpPut("uncomplete")]
    public async Task<IActionResult> Uncomplete([FromBody] CompleteUncomplete dto)
    {
        if (dto is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/tasks/uncomplete",
            $"{nameof(TasksController)}.{nameof(Uncomplete)}",
            UserId
        );

        dto.UserId = UserId;

        try
        {
            CompleteUncompleteTaskResult result = await _taskService.UncompleteAsync(dto, tr);

            if (result.NotifySignalR)
            {
                await _listActionsHubContext.Clients.Group(result.ListId.ToString()).SendAsync("TaskCompletedChanged", AuthId, dto.Id, result.ListId, false);
            }

            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                string message = _localizer["UncompletedTaskNotification", result.ActionUserName, result.TaskName, result.ListName];

                var updateNotificationDto = new CreateOrUpdateNotification(recipient.Id, dto.UserId, result.ListId, dto.Id, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(updateNotificationDto, tr);
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
            _logger.LogError(ex, $"Unexpected error in {nameof(Uncomplete)}");
            throw;
        }
        finally
        {
            tr.Finish();
        }

        return NoContent();
    }

    //[HttpPut("reorder")]
    //public async Task<IActionResult> Reorder([FromBody] ReorderTask dto)
    //{
    //    if (dto is null)
    //    {
    //        return BadRequest();
    //    }

    //    dto.UserId = UserId;

    //    ReorderTaskResult result = await _taskService.ReorderAsync(dto);

    //    try
    //    {
    //        if (result.NotifySignalR)
    //        {
    //            await _hubContext.Clients.Group(result.ListId.ToString()).SendAsync("TaskReordered", dto.UserId, dto.Id, result.ListId, dto.OldOrder, dto.NewOrder);
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
        return $"{_url}/notifications/{notificationId}";
    }
}
