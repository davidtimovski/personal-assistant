﻿using System;
using System.Globalization;
using System.Threading.Tasks;
using Api.Config;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications.Models;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models;
using PersonalAssistant.Infrastructure.Identity;
using PersonalAssistant.Infrastructure.Sender.Models;

namespace Api.Controllers.ToDoAssistant
{
    [Authorize]
    [EnableCors("AllowToDoAssistant")]
    [Route("api/[controller]")]
    public class TasksController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IListService _listService;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly ISenderService _senderService;
        private readonly IValidator<CreateTask> _createValidator;
        private readonly IValidator<BulkCreate> _bulkCreateValidator;
        private readonly IValidator<UpdateTask> _updateValidator;
        private readonly IStringLocalizer<TasksController> _localizer;
        private readonly Urls _urls;

        public TasksController(
            ITaskService taskService,
            IListService listService,
            IUserService userService,
            INotificationService notificationService,
            ISenderService senderService,
            IValidator<CreateTask> createValidator,
            IValidator<BulkCreate> bulkCreateValidator,
            IValidator<UpdateTask> updateValidator,
            IStringLocalizer<TasksController> localizer,
            IOptions<Urls> urls)
        {
            _taskService = taskService;
            _listService = listService;
            _userService = userService;
            _notificationService = notificationService;
            _senderService = senderService;
            _createValidator = createValidator;
            _bulkCreateValidator = bulkCreateValidator;
            _updateValidator = updateValidator;
            _localizer = localizer;
            _urls = urls.Value;
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

            TaskDto taskDto = _taskService.Get(id, userId);
            if (taskDto == null)
            {
                return NotFound();
            }

            return Ok(taskDto);
        }

        [HttpGet("{id}/update")]
        public IActionResult GetForUpdate(int id)
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

            TaskForUpdate taskDto = _taskService.GetForUpdate(id, userId);
            if (taskDto == null)
            {
                return NotFound();
            }

            return Ok(taskDto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTask dto)
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

            CreatedTaskResult result = await _taskService.CreateAsync(dto, _createValidator);

            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["CreatedTaskNotification", IdentityHelper.GetUserName(User), result.TaskName, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, dto.UserId, dto.ListId, result.TaskId, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
                var pushNotification = new PushNotification
                {
                    SenderImageUri = result.ActionUserImageUri,
                    UserId = recipient.Id,
                    Application = "To Do Assistant",
                    Message = message,
                    OpenUrl = GetNotificationsPageUrl(notificationId)
                };

                _senderService.Enqueue(pushNotification);
            }

            return StatusCode(201, result.TaskId);
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreate([FromBody] BulkCreate dto)
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

            BulkCreateResult result = await _taskService.BulkCreateAsync(dto, _bulkCreateValidator);
            if (!result.Notify())
            {
                return StatusCode(201);
            }

            foreach (BulkCreatedTask task in result.CreatedTasks)
            {
                foreach (var recipient in result.NotificationRecipients)
                {
                    CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                    var message = _localizer["CreatedTaskNotification", IdentityHelper.GetUserName(User), task.Name, result.ListName];

                    var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, dto.UserId, dto.ListId, task.Id, message);
                    var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
                    var pushNotification = new PushNotification
                    {
                        SenderImageUri = result.ActionUserImageUri,
                        UserId = recipient.Id,
                        Application = "To Do Assistant",
                        Message = message,
                        OpenUrl = GetNotificationsPageUrl(notificationId)
                    };

                    _senderService.Enqueue(pushNotification);
                }
            }

            return StatusCode(201);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateTask dto)
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

            UpdateTaskResult result = await _taskService.UpdateAsync(dto, _updateValidator);
            if (!result.Notify())
            {
                return NoContent();
            }

            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["UpdatedTaskNotification", IdentityHelper.GetUserName(User), result.OriginalTaskName, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, dto.UserId, result.ListId, dto.Id, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
                var pushNotification = new PushNotification
                {
                    SenderImageUri = result.ActionUserImageUri,
                    UserId = recipient.Id,
                    Application = "To Do Assistant",
                    Message = message,
                    OpenUrl = GetNotificationsPageUrl(notificationId)
                };

                _senderService.Enqueue(pushNotification);
            }

            foreach (var recipient in result.RemovedNotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["RemovedTaskNotification", IdentityHelper.GetUserName(User), result.OriginalTaskName, result.OldListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, dto.UserId, result.OldListId, null, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
                var pushNotification = new PushNotification
                {
                    SenderImageUri = result.ActionUserImageUri,
                    UserId = recipient.Id,
                    Application = "To Do Assistant",
                    Message = message,
                    OpenUrl = GetNotificationsPageUrl(notificationId)
                };

                _senderService.Enqueue(pushNotification);
            }

            foreach (var recipient in result.CreatedNotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["CreatedTaskNotification", IdentityHelper.GetUserName(User), dto.Name, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, dto.UserId, result.ListId, dto.Id, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
                var pushNotification = new PushNotification
                {
                    SenderImageUri = result.ActionUserImageUri,
                    UserId = recipient.Id,
                    Application = "To Do Assistant",
                    Message = message,
                    OpenUrl = GetNotificationsPageUrl(notificationId)
                };

                _senderService.Enqueue(pushNotification);
            }

            if (result.AssignedNotificationRecipient != null)
            {
                CultureInfo.CurrentCulture = new CultureInfo(result.AssignedNotificationRecipient.Language, false);
                var message = _localizer["AssignedTaskNotification", IdentityHelper.GetUserName(User), result.OriginalTaskName, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(result.AssignedNotificationRecipient.Id, dto.UserId, result.ListId, dto.Id, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
                var pushNotification = new PushNotification
                {
                    SenderImageUri = result.ActionUserImageUri,
                    UserId = result.AssignedNotificationRecipient.Id,
                    Application = "To Do Assistant",
                    Message = message,
                    OpenUrl = GetNotificationsPageUrl(notificationId)
                };

                _senderService.Enqueue(pushNotification);
            }

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

            DeleteTaskResult result = await _taskService.DeleteAsync(id, userId);

            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["RemovedTaskNotification", IdentityHelper.GetUserName(User), result.TaskName, result.ListName];

                var createNotificationDto = new CreateOrUpdateNotification(recipient.Id, userId, result.ListId, null, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
                var pushNotification = new PushNotification
                {
                    SenderImageUri = result.ActionUserImageUri,
                    UserId = recipient.Id,
                    Application = "To Do Assistant",
                    Message = message,
                    OpenUrl = GetNotificationsPageUrl(notificationId)
                };

                _senderService.Enqueue(pushNotification);
            }

            return NoContent();
        }

        [HttpPut("complete")]
        public async Task<IActionResult> Complete([FromBody] CompleteUncomplete dto)
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

            CompleteUncompleteTaskResult result = await _taskService.CompleteAsync(dto);

            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                string message = _localizer["CompletedTaskNotification", IdentityHelper.GetUserName(User), result.TaskName, result.ListName];

                var updateNotificationDto = new CreateOrUpdateNotification(recipient.Id, dto.UserId, result.ListId, dto.Id, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(updateNotificationDto);
                var pushNotification = new PushNotification
                {
                    SenderImageUri = result.ActionUserImageUri,
                    UserId = recipient.Id,
                    Application = "To Do Assistant",
                    Message = message,
                    OpenUrl = GetNotificationsPageUrl(notificationId)
                };

                _senderService.Enqueue(pushNotification);
            }

            return NoContent();
        }

        [HttpPut("uncomplete")]
        public async Task<IActionResult> Uncomplete([FromBody] CompleteUncomplete dto)
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

            CompleteUncompleteTaskResult result = await _taskService.UncompleteAsync(dto);
  
            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                string message = _localizer["UncompletedTaskNotification", IdentityHelper.GetUserName(User), result.TaskName, result.ListName];

                var updateNotificationDto = new CreateOrUpdateNotification(recipient.Id, dto.UserId, result.ListId, dto.Id, message);
                var notificationId = await _notificationService.CreateOrUpdateAsync(updateNotificationDto);
                var pushNotification = new PushNotification
                {
                    SenderImageUri = result.ActionUserImageUri,
                    UserId = recipient.Id,
                    Application = "To Do Assistant",
                    Message = message,
                    OpenUrl = GetNotificationsPageUrl(notificationId)
                };

                _senderService.Enqueue(pushNotification);
            }

            return NoContent();
        }

        [HttpPut("reorder")]
        public async Task<IActionResult> Reorder([FromBody] ReorderTask dto)
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

            await _taskService.ReorderAsync(dto);

            return NoContent();
        }

        private string GetNotificationsPageUrl(int notificationId)
        {
            return $"{_urls.ToDoAssistant}/notifications/{notificationId}";
        }
    }
}