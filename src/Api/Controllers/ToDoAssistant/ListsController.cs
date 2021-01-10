using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Api.Config;
using Api.Models.ToDoAssistant.Lists;
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
using PersonalAssistant.Infrastructure.Identity;
using PersonalAssistant.Infrastructure.Sender.Models;

namespace Api.Controllers.ToDoAssistant
{
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
        public async Task<IActionResult> GetAll()
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

            IEnumerable<ListDto> lists = await _listService.GetAllAsync(userId);

            return Ok(lists);
        }

        [HttpGet("options")]
        public async Task<IActionResult> GetAllAsOptions()
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

            IEnumerable<ToDoListOption> options = await _listService.GetAllAsOptionsAsync(userId);

            return Ok(options);
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

            EditListDto list = await _listService.GetAsync(id, userId);
            if (list == null)
            {
                return NotFound();
            }

            return Ok(list);
        }

        [HttpGet("{id}/with-shares")]
        public async Task<IActionResult> GetWithShares(int id)
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

            ListWithShares list = await _listService.GetWithSharesAsync(id, userId);
            if (list == null)
            {
                return NotFound();
            }

            return Ok(list);
        }

        [HttpGet("share-requests")]
        public async Task<IActionResult> GetShareRequests()
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

            IEnumerable<ShareRequest> shareRequests = await _listService.GetShareRequestsAsync(userId);

            return Ok(shareRequests);
        }

        [HttpGet("pending-share-requests-count")]
        public async Task<IActionResult> GetPendingShareRequestsCount()
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

            int pendingShareRequestsCount = await _listService.GetPendingShareRequestsCountAsync(userId);

            return Ok(pendingShareRequestsCount);
        }

        [HttpGet("{id}/shared")]
        public async Task<IActionResult> GetIsShared(int id)
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

            bool isShared = await _listService.IsSharedAsync(id, userId);

            return Ok(isShared);
        }

        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetMembersAsAssigneeOptions(int id)
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

            var assigneeOptions = await _listService.GetMembersAsAssigneeOptionsAsync(id);

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

            UpdateListOriginal original = await _listService.UpdateAsync(dto, _updateValidator);

            // Notify
            var usersToBeNotified = await _userService.GetToBeNotifiedOfListChangeAsync(original.Id, dto.UserId);
            if (usersToBeNotified.Any())
            {
                var currentUser = await _userService.GetAsync(dto.UserId);

                if (dto.Name != original.Name || dto.Icon != original.Icon)
                {
                    var resourceKey = "UpdatedListNotification";

                    if (dto.Name != original.Name && dto.Icon == original.Icon)
                    {
                        resourceKey = "UpdatedListNameNotification";
                    }
                    else if (dto.Name == original.Name && dto.Icon != original.Icon)
                    {
                        resourceKey = "UpdatedListIconNotification";
                    }

                    foreach (var user in usersToBeNotified)
                    {
                        CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                        var message = _localizer[resourceKey, IdentityHelper.GetUserName(User), original.Name];

                        var createNotificationDto = new CreateOrUpdateNotification(user.Id, dto.UserId, original.Id, null, message);
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

            string deletedListName = await _listService.DeleteAsync(id, userId);

            // Notify
            var usersToBeNotified = await _userService.GetToBeNotifiedOfListDeletionAsync(id);
            if (usersToBeNotified.Any())
            {
                var currentUser = await _userService.GetAsync(userId);

                foreach (var user in usersToBeNotified)
                {
                    CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                    var message = _localizer["DeletedListNotification", IdentityHelper.GetUserName(User), deletedListName];

                    var createNotificationDto = new CreateOrUpdateNotification(user.Id, userId, null, null, message);
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

        [HttpGet("can-share-with-user/{email}")]
        public async Task<IActionResult> CanShareListWithUser(string email)
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

            var user = await _userService.GetAsync(email);

            if (user != null)
            {
                canShareVm.UserId = user.Id;
                canShareVm.ImageUri = user.ImageUri;
                canShareVm.CanShare = await _listService.CanShareWithUserAsync(user.Id, userId);
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
                // Notify
                if (await _userService.CheckIfUserCanBeNotifiedOfListChangeAsync(dto.ListId, removedShare.UserId))
                {
                    var currentUser = await _userService.GetAsync(dto.UserId);
                    var user = await _userService.GetAsync(removedShare.UserId);
                    SimpleList list = await _listService.GetAsync(dto.ListId);

                    CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                    var message = _localizer["RemovedShareNotification", IdentityHelper.GetUserName(User), list.Name];

                    var createNotificationDto = new CreateOrUpdateNotification(user.Id, dto.UserId, null, null, message);
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

            bool shareWasAccepted = await _listService.LeaveAsync(id, userId);

            // Notify if joined in the first place
            if (shareWasAccepted)
            {
                var usersToBeNotified = await _userService.GetToBeNotifiedOfListChangeAsync(id, userId);
                if (usersToBeNotified.Any())
                {
                    var currentUser = await _userService.GetAsync(userId);
                    SimpleList list = await _listService.GetAsync(id);

                    foreach (var user in usersToBeNotified)
                    {
                        CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                        var message = _localizer["LeftListNotification", IdentityHelper.GetUserName(User), list.Name];

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

            bool nonPrivateTasksWereUncompleted = await _listService.SetTasksAsNotCompletedAsync(dto.ListId, userId);
            if (nonPrivateTasksWereUncompleted)
            {
                // Notify
                var usersToBeNotified = await _userService.GetToBeNotifiedOfListChangeAsync(dto.ListId, userId);
                if (usersToBeNotified.Any())
                {
                    var currentUser = await _userService.GetAsync(userId);
                    SimpleList list = await _listService.GetAsync(dto.ListId);

                    foreach (var user in usersToBeNotified)
                    {
                        CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                        var message = _localizer["UncompletedAllTasksNotification", IdentityHelper.GetUserName(User), list.Name];

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

            await _listService.SetShareIsAcceptedAsync(dto.ListId, userId, dto.IsAccepted);

            // Notify
            var usersToBeNotified = await _userService.GetToBeNotifiedOfListChangeAsync(dto.ListId, userId);
            if (usersToBeNotified.Any())
            {
                var currentUser = await _userService.GetAsync(userId);
                SimpleList list = await _listService.GetAsync(dto.ListId);
                var localizerKey = dto.IsAccepted ? "JoinedListNotification" : "DeclinedShareRequestNotification";

                foreach (var user in usersToBeNotified)
                {
                    CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                    var message = _localizer[localizerKey, IdentityHelper.GetUserName(User), list.Name];

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
}