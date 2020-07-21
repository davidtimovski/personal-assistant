﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Api.Config;
using Api.Models.ToDoAssistant.Tasks;
using AspNet.Security.OAuth.Introspection;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications.Models;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models;
using PersonalAssistant.Domain.Entities.Common;
using PersonalAssistant.Infrastructure.Identity;
using PersonalAssistant.Infrastructure.Sender.Models;

namespace Api.Controllers.ToDoAssistant
{
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
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
        public async Task<IActionResult> Get(int id)
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

            TaskDto taskDto = await _taskService.GetAsync(id, userId);
            if (taskDto == null)
            {
                return NotFound();
            }

            return Ok(taskDto);
        }

        [HttpGet("{id}/update")]
        public async Task<IActionResult> GetForUpdate(int id)
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

            TaskForUpdate taskDto = await _taskService.GetForUpdateAsync(id, userId);
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

            CreatedTask createdTask = await _taskService.CreateAsync(dto, _createValidator);

            // Notify
            var usersToBeNotified = await _userService.GetToBeNotifiedOfListChangeAsync(dto.ListId, dto.UserId, dto.IsPrivate.HasValue && dto.IsPrivate.Value);
            if (usersToBeNotified.Any())
            {
                var currentUser = await _userService.GetAsync(dto.UserId);
                SimpleList list = await _listService.GetAsync(dto.ListId);

                foreach (var user in usersToBeNotified)
                {
                    CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                    var message = _localizer["CreatedTaskNotification", IdentityHelper.GetUserName(User), createdTask.Name, list.Name];

                    var createNotificationDto = new CreateOrUpdateNotification(user.Id, dto.UserId, list.Id, createdTask.Id, message);
                    var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
                    var pushNotification = new PushNotification
                    {
                        SenderImageUri = currentUser.ImageUri,
                        UserId = user.Id,
                        Application = "To Do Assistant",
                        Message = message,
                        OpenUrl = GetNotificationsPageUrl(notificationId)
                    };

                    _senderService.Enqueue(pushNotification);
                }
            }

            return StatusCode(201, createdTask);
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

            IEnumerable<CreatedTask> createdTasks = await _taskService.BulkCreateAsync(dto, _bulkCreateValidator);

            // Notify
            var usersToBeNotified = await _userService.GetToBeNotifiedOfListChangeAsync(dto.ListId, dto.UserId, dto.TasksArePrivate);
            if (usersToBeNotified.Any())
            {
                var currentUser = await _userService.GetAsync(dto.UserId);
                SimpleList list = await _listService.GetAsync(dto.ListId);

                foreach (CreatedTask task in createdTasks)
                {
                    foreach (var user in usersToBeNotified)
                    {
                        CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                        var message = _localizer["CreatedTaskNotification", IdentityHelper.GetUserName(User), task.Name, list.Name];

                        var createNotificationDto = new CreateOrUpdateNotification(user.Id, dto.UserId, list.Id, task.Id, message);
                        var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
                        var pushNotification = new PushNotification
                        {
                            SenderImageUri = currentUser.ImageUri,
                            UserId = user.Id,
                            Application = "To Do Assistant",
                            Message = message,
                            OpenUrl = GetNotificationsPageUrl(notificationId)
                        };

                        _senderService.Enqueue(pushNotification);
                    }
                }
            }

            return StatusCode(201, createdTasks);
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

            SimpleTask originalTask = await _taskService.GetAsync(dto.Id);

            await _taskService.UpdateAsync(dto, _updateValidator);

            // Notify
            if (dto.ListId == originalTask.ListId)
            {
                var usersToBeNotified = await _userService.GetToBeNotifiedOfListChangeAsync(dto.ListId, dto.UserId, dto.Id);
                if (usersToBeNotified.Any())
                {
                    var currentUser = await _userService.GetAsync(dto.UserId);
                    SimpleList list = await _listService.GetAsync(dto.ListId);

                    foreach (User user in usersToBeNotified)
                    {
                        CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                        var message = _localizer["UpdatedTaskNotification", IdentityHelper.GetUserName(User), originalTask.Name, list.Name];

                        var createNotificationDto = new CreateOrUpdateNotification(user.Id, dto.UserId, list.Id, dto.Id, message);
                        var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
                        var pushNotification = new PushNotification
                        {
                            SenderImageUri = currentUser.ImageUri,
                            UserId = user.Id,
                            Application = "To Do Assistant",
                            Message = message,
                            OpenUrl = GetNotificationsPageUrl(notificationId)
                        };

                        _senderService.Enqueue(pushNotification);
                    }
                }
            }
            else
            {
                SimpleList oldList = await _listService.GetAsync(originalTask.ListId);
                SimpleList newList = await _listService.GetAsync(dto.ListId);
                var currentUserName = IdentityHelper.GetUserName(User);

                var usersToBeNotifiedOfRemoval = await _userService.GetToBeNotifiedOfListChangeAsync(oldList.Id, dto.UserId, dto.Id);
                if (usersToBeNotifiedOfRemoval.Any())
                {
                    var currentUser = await _userService.GetAsync(dto.UserId);

                    foreach (var user in usersToBeNotifiedOfRemoval)
                    {
                        CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                        var message = _localizer["RemovedTaskNotification", currentUserName, originalTask.Name, oldList.Name];

                        var createNotificationDto = new CreateOrUpdateNotification(user.Id, dto.UserId, oldList.Id, null, message);
                        var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
                        var pushNotification = new PushNotification
                        {
                            SenderImageUri = currentUser.ImageUri,
                            UserId = user.Id,
                            Application = "To Do Assistant",
                            Message = message,
                            OpenUrl = GetNotificationsPageUrl(notificationId)
                        };

                        _senderService.Enqueue(pushNotification);
                    }
                }

                var usersToBeNotifiedOfCreation = await _userService.GetToBeNotifiedOfListChangeAsync(newList.Id, dto.UserId, dto.Id);
                if (usersToBeNotifiedOfCreation.Any())
                {
                    var currentUser = await _userService.GetAsync(dto.UserId);

                    foreach (var user in usersToBeNotifiedOfCreation)
                    {
                        CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                        var message = _localizer["CreatedTaskNotification", currentUserName, dto.Name, newList.Name];

                        var createNotificationDto = new CreateOrUpdateNotification(user.Id, dto.UserId, newList.Id, dto.Id, message);
                        var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
                        var pushNotification = new PushNotification
                        {
                            SenderImageUri = currentUser.ImageUri,
                            UserId = user.Id,
                            Application = "To Do Assistant",
                            Message = message,
                            OpenUrl = GetNotificationsPageUrl(notificationId)
                        };

                        _senderService.Enqueue(pushNotification);
                    }
                }
            }

            // Notify if assigned to another user
            if (dto.AssignedToUserId.HasValue
                && dto.AssignedToUserId.Value != originalTask.AssignedToUserId
                && dto.AssignedToUserId.Value != dto.UserId)
            {
                if (await _userService.CheckIfUserCanBeNotifiedOfListChangeAsync(dto.ListId, dto.AssignedToUserId.Value))
                {
                    var assignedUser = await _userService.GetAsync(dto.AssignedToUserId.Value);
                    SimpleList list = await _listService.GetAsync(dto.ListId);

                    CultureInfo.CurrentCulture = new CultureInfo(assignedUser.Language, false);
                    var message = _localizer["AssignedTaskNotification", IdentityHelper.GetUserName(User), originalTask.Name, list.Name];

                    var createNotificationDto = new CreateOrUpdateNotification(assignedUser.Id, dto.UserId, list.Id, dto.Id, message);
                    var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
                    var pushNotification = new PushNotification
                    {
                        UserId = assignedUser.Id,
                        Application = "To Do Assistant",
                        Message = message,
                        OpenUrl = GetNotificationsPageUrl(notificationId)
                    };

                    _senderService.Enqueue(pushNotification);
                }
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

            SimpleTask deletedTask = await _taskService.DeleteAsync(id, userId);

            // Notify
            var usersToBeNotified = await _userService.GetToBeNotifiedOfListChangeAsync(deletedTask.ListId, userId, deletedTask.PrivateToUserId == userId);
            if (usersToBeNotified.Any())
            {
                var currentUser = await _userService.GetAsync(userId);
                SimpleList list = await _listService.GetAsync(deletedTask.ListId);

                foreach (var user in usersToBeNotified)
                {
                    CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                    var message = _localizer["RemovedTaskNotification", IdentityHelper.GetUserName(User), deletedTask.Name, list.Name];

                    var createNotificationDto = new CreateOrUpdateNotification(user.Id, userId, list.Id, null, message);
                    var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
                    var pushNotification = new PushNotification
                    {
                        SenderImageUri = currentUser.ImageUri,
                        UserId = user.Id,
                        Application = "To Do Assistant",
                        Message = message,
                        OpenUrl = GetNotificationsPageUrl(notificationId)
                    };

                    _senderService.Enqueue(pushNotification);
                }
            }

            return NoContent();
        }

        [HttpPut("is-completed")]
        public async Task<IActionResult> SetIsCompleted([FromBody] SetIsCompletedOnTaskDto dto)
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

            SimpleTask task = await _taskService.SetIsCompletedAsync(dto.Id, dto.IsCompleted, userId);

            // Notify
            var usersToBeNotified = await _userService.GetToBeNotifiedOfListChangeAsync(task.ListId, userId, task.Id);
            if (usersToBeNotified.Any())
            {
                var currentUser = await _userService.GetAsync(userId);
                SimpleList list = await _listService.GetAsync(task.ListId);

                foreach (var user in usersToBeNotified)
                {
                    CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                    string message;

                    if (dto.IsCompleted)
                    {
                        message = _localizer["CompletedTaskNotification", IdentityHelper.GetUserName(User), task.Name, list.Name];
                    }
                    else
                    {
                        message = _localizer["UncompletedTaskNotification", IdentityHelper.GetUserName(User), task.Name, list.Name];
                    }

                    var createNotificationDto = new CreateOrUpdateNotification(user.Id, userId, list.Id, task.Id, message);
                    var notificationId = await _notificationService.CreateOrUpdateAsync(createNotificationDto);
                    var pushNotification = new PushNotification
                    {
                        SenderImageUri = currentUser.ImageUri,
                        UserId = user.Id,
                        Application = "To Do Assistant",
                        Message = message,
                        OpenUrl = GetNotificationsPageUrl(notificationId)
                    };

                    _senderService.Enqueue(pushNotification);
                }
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