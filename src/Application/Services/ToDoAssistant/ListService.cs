using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.Common;
using Application.Contracts.Common.Models;
using Application.Contracts.ToDoAssistant;
using Application.Contracts.ToDoAssistant.Lists;
using Application.Contracts.ToDoAssistant.Lists.Models;
using Application.Contracts.ToDoAssistant.Notifications;
using Application.Contracts.ToDoAssistant.Tasks;
using Application.Utils;
using AutoMapper;
using Domain.Entities.Common;
using Domain.Entities.ToDoAssistant;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Services.ToDoAssistant;

public class ListService : IListService
{
    private readonly IUserService _userService;
    private readonly IListsRepository _listsRepository;
    private readonly ITasksRepository _tasksRepository;
    private readonly INotificationsRepository _notificationsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ListService> _logger;

    public ListService(
        IUserService userService,
        IListsRepository listsRepository,
        ITasksRepository tasksRepository,
        INotificationsRepository notificationsRepository,
        IMapper mapper,
        ILogger<ListService> logger)
    {
        _userService = userService;
        _listsRepository = listsRepository;
        _tasksRepository = tasksRepository;
        _notificationsRepository = notificationsRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public string[] IconOptions => new string[] { "list", "shopping-cart", "shopping-bag", "home", "birthday", "cheers", "vacation", "passport", "plane", "car", "pickup-truck", "world", "camping", "tree", "motorcycle", "bicycle", "workout", "ski", "snowboard", "swimming", "work", "baby", "dog", "cat", "bird", "fish", "camera", "medicine", "file", "book", "mountain", "facebook", "twitter", "instagram", "tiktok" };

    public IEnumerable<ListDto> GetAll(int userId)
    {
        try
        {
            IEnumerable<ToDoList> lists = _listsRepository.GetAllWithTasksAndSharingDetails(userId);

            var result = lists.Select(x => _mapper.Map<ListDto>(x, opts => { opts.Items["UserId"] = userId; }));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetAll)}");
            throw;
        }
    }

    public IEnumerable<ToDoListOption> GetAllAsOptions(int userId)
    {
        try
        {
            IEnumerable<ToDoList> lists = _listsRepository.GetAllAsOptions(userId);

            var result = lists.Select(x => _mapper.Map<ToDoListOption>(x));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetAllAsOptions)}");
            throw;
        }
    }

    public IEnumerable<Assignee> GetMembersAsAssigneeOptions(int id, int userId)
    {
        try
        {
            if (!_listsRepository.UserOwnsOrShares(id, userId))
            {
                throw new ValidationException("Unauthorized");
            }

            IEnumerable<User> members = _listsRepository.GetMembersAsAssigneeOptions(id);

            var result = members.Select(x => _mapper.Map<Assignee>(x));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetMembersAsAssigneeOptions)}");
            throw;
        }
    }

    public SimpleList Get(int id)
    {
        try
        {
            ToDoList list = _listsRepository.Get(id);

            var result = _mapper.Map<SimpleList>(list);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Get)}");
            throw;
        }
    }

    public EditListDto GetForEdit(int id, int userId)
    {
        try
        {
            ToDoList list = _listsRepository.GetWithShares(id, userId);

            var result = _mapper.Map<EditListDto>(list, opts => { opts.Items["UserId"] = userId; });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetForEdit)}");
            throw;
        }
    }

    public ListWithShares GetWithShares(int id, int userId)
    {
        try
        {
            ToDoList list = _listsRepository.GetWithOwner(id, userId);
            if (list == null)
            {
                return null;
            }

            list.Shares.AddRange(_listsRepository.GetShares(id));

            var result = _mapper.Map<ListWithShares>(list, opts => { opts.Items["UserId"] = userId; });
            result.Shares.RemoveAll(x => x.UserId == userId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetWithShares)}");
            throw;
        }
    }

    public IEnumerable<ShareListRequest> GetShareRequests(int userId)
    {
        try
        {
            IEnumerable<ListShare> shareRequests = _listsRepository.GetShareRequests(userId);

            var result = shareRequests.Select(x => _mapper.Map<ShareListRequest>(x, opts => { opts.Items["UserId"] = userId; }));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetShareRequests)}");
            throw;
        }
    }

    public int GetPendingShareRequestsCount(int userId)
    {
        try
        {
            return _listsRepository.GetPendingShareRequestsCount(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetPendingShareRequestsCount)}");
            throw;
        }
    }

    public bool CanShareWithUser(int shareWithId, int userId)
    {
        try
        {
            if (shareWithId == userId)
            {
                return false;
            }

            return _listsRepository.CanShareWithUser(shareWithId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CanShareWithUser)}");
            throw;
        }
    }

    public bool UserOwnsOrShares(int id, int userId)
    {
        try
        {
            return _listsRepository.UserOwnsOrShares(id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UserOwnsOrShares)}");
            throw;
        }
    }

    public bool UserOwnsOrSharesAsPending(int id, int userId)
    {
        try
        {
            return _listsRepository.UserOwnsOrSharesAsPending(id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UserOwnsOrSharesAsPending)}");
            throw;
        }
    }

    public bool UserOwnsOrSharesAsAdmin(int id, int userId)
    {
        try
        {
            return _listsRepository.UserOwnsOrSharesAsAdmin(id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UserOwnsOrSharesAsAdmin)}");
            throw;
        }
    }

    public bool UserOwnsOrSharesAsAdmin(int id, string name, int userId)
    {
        try
        {
            return _listsRepository.UserOwnsOrSharesAsAdmin(id, name.Trim(), userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UserOwnsOrSharesAsAdmin)}");
            throw;
        }
    }

    public bool IsShared(int id, int userId)
    {
        try
        {
            if (!UserOwnsOrShares(id, userId))
            {
                throw new ValidationException("Unauthorized");
            }

            return _listsRepository.IsShared(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(IsShared)}");
            throw;
        }
    }

    public bool Exists(string name, int userId)
    {
        try
        {
            return _listsRepository.Exists(name.Trim(), userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Exists)}");
            throw;
        }
    }

    public bool Exists(int id, string name, int userId)
    {
        try
        {
            return _listsRepository.Exists(id, name.Trim(), userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Exists)}");
            throw;
        }
    }

    public int Count(int userId)
    {
        try
        {
            return _listsRepository.Count(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Count)}");
            throw;
        }
    }

    public IEnumerable<User> GetUsersToBeNotifiedOfChange(int id, int excludeUserId, bool isPrivate)
    {
        try
        {
            if (isPrivate)
            {
                return new List<User>();
            }

            return _listsRepository.GetUsersToBeNotifiedOfChange(id, excludeUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetUsersToBeNotifiedOfChange)}");
            throw;
        }
    }

    public IEnumerable<User> GetUsersToBeNotifiedOfChange(int id, int excludeUserId, int taskId)
    {
        try
        {
            if (_tasksRepository.IsPrivate(taskId, excludeUserId))
            {
                return new List<User>();
            }

            return _listsRepository.GetUsersToBeNotifiedOfChange(id, excludeUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetUsersToBeNotifiedOfChange)}");
            throw;
        }
    }

    public bool CheckIfUserCanBeNotifiedOfChange(int id, int userId)
    {
        try
        {
            return _listsRepository.CheckIfUserCanBeNotifiedOfChange(id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CheckIfUserCanBeNotifiedOfChange)}");
            throw;
        }
    }

    public async Task<int> CreateAsync(CreateList model, IValidator<CreateList> validator)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        try
        {
            var list = _mapper.Map<ToDoList>(model);

            list.Name = list.Name.Trim();
            list.CreatedDate = list.ModifiedDate = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(model.TasksText))
            {
                list.Tasks = model.TasksText.Split("\n")
                    .Where(task => !string.IsNullOrWhiteSpace(task))
                    .Select(task => new ToDoTask
                        {
                            Name = task.Trim(),
                            IsOneTime = list.IsOneTimeToggleDefault,
                            CreatedDate = list.CreatedDate,
                            ModifiedDate = list.CreatedDate
                        }
                    ).ToList();
            }

            return await _listsRepository.CreateAsync(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateAsync)}");
            throw;
        }
    }

    public async Task CreateSampleAsync(int userId, Dictionary<string, string> translations)
    {
        try
        {
            var now = DateTime.UtcNow;

            var list = new ToDoList
            {
                UserId = userId,
                Name = translations["SampleListName"],
                Icon = "list",
                CreatedDate = now,
                ModifiedDate = now
            };

            list.Tasks = new List<ToDoTask>
            {
                new ToDoTask
                {
                    Name = translations["SampleListTask1"],
                    CreatedDate = list.CreatedDate,
                    ModifiedDate = list.CreatedDate
                },
                new ToDoTask
                {
                    Name = translations["SampleListTask2"],
                    CreatedDate = list.CreatedDate,
                    ModifiedDate = list.CreatedDate
                },
                new ToDoTask
                {
                    Name = translations["SampleListTask3"],
                    CreatedDate = list.CreatedDate,
                    ModifiedDate = list.CreatedDate
                }
            };
            await _listsRepository.CreateAsync(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateSampleAsync)}");
            throw;
        }
    }

    public async Task<UpdateListResult> UpdateAsync(UpdateList model, IValidator<UpdateList> validator)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        try
        {
            var list = _mapper.Map<ToDoList>(model);

            list.Name = list.Name.Trim();
            list.ModifiedDate = DateTime.UtcNow;

            ToDoList original = await _listsRepository.UpdateAsync(list, model.UserId);

            var usersToBeNotified = _listsRepository.GetUsersToBeNotifiedOfChange(model.Id, model.UserId).ToList();
            if (!usersToBeNotified.Any())
            {
                return new UpdateListResult();
            }

            ListNotificationType notificationType;
            if (model.Name != original.Name && model.Icon == original.Icon)
            {
                notificationType = ListNotificationType.NameUpdated;
            }
            else if (model.Name == original.Name && model.Icon != original.Icon)
            {
                notificationType = ListNotificationType.IconUpdated;
            }
            else
            {
                notificationType = ListNotificationType.Other;
            }

            var user = _userService.Get(model.UserId);
            var result = new UpdateListResult
            {
                Type = notificationType,
                OriginalListName = original.Name,
                ActionUserName = user.Name,
                ActionUserImageUri = user.ImageUri,
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateAsync)}");
            throw;
        }
    }

    public async Task UpdateSharedAsync(UpdateSharedList model, IValidator<UpdateSharedList> validator)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        try
        {
            var list = _mapper.Map<ToDoList>(model);

            list.ModifiedDate = DateTime.UtcNow;
            await _listsRepository.UpdateSharedAsync(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateSharedAsync)}");
            throw;
        }
    }

    public async Task<DeleteListResult> DeleteAsync(int id, int userId)
    {
        try
        {
            if (!_listsRepository.UserOwns(id, userId))
            {
                throw new ValidationException("Unauthorized");
            }

            var usersToBeNotified = _listsRepository.GetUsersToBeNotifiedOfDeletion(id).ToList();

            string deletedListName = await _listsRepository.DeleteAsync(id);

            if (!usersToBeNotified.Any())
            {
                return new DeleteListResult();
            }

            var user = _userService.Get(userId);
            var result = new DeleteListResult
            {
                DeletedListName = deletedListName,
                ActionUserName = user.Name,
                ActionUserImageUri = user.ImageUri,
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteAsync)}");
            throw;
        }
    }

    public async Task ShareAsync(ShareList model, IValidator<ShareList> validator)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        try
        {
            var now = DateTime.UtcNow;

            var newShares = new List<ListShare>();
            foreach (ShareUserAndPermission newShare in model.NewShares)
            {
                if (_listsRepository.UserHasBlockedSharing(model.ListId, model.UserId, newShare.UserId))
                {
                    continue;
                }

                newShares.Add(new ListShare
                {
                    ListId = model.ListId,
                    UserId = newShare.UserId,
                    IsAdmin = newShare.IsAdmin,
                    CreatedDate = now,
                    ModifiedDate = now
                });
            }

            var editedShares = model.EditedShares.Select(x => new ListShare
            {
                ListId = model.ListId,
                UserId = x.UserId,
                IsAdmin = x.IsAdmin,
                ModifiedDate = now
            });

            var removedShares = model.RemovedShares.Select(x => new ListShare
            {
                ListId = model.ListId,
                UserId = x.UserId,
                IsAdmin = x.IsAdmin
            });

            await _listsRepository.SaveSharingDetailsAsync(newShares, editedShares, removedShares);

            foreach (ListShare share in removedShares)
            {
                await _notificationsRepository.DeleteForUserAndListAsync(share.UserId, share.ListId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(ShareAsync)}");
            throw;
        }
    }

    public async Task<LeaveListResult> LeaveAsync(int id, int userId)
    {
        try
        {
            ListShare share = await _listsRepository.LeaveAsync(id, userId);

            if (share.IsAccepted == false)
            {
                return new LeaveListResult();
            }

            await _notificationsRepository.DeleteForUserAndListAsync(userId, id);

            var usersToBeNotified = _listsRepository.GetUsersToBeNotifiedOfChange(id, userId).ToList();
            if (!usersToBeNotified.Any())
            {
                return new LeaveListResult();
            }

            ToDoList list = _listsRepository.Get(id);

            var user = _userService.Get(userId);
            var result = new LeaveListResult
            {
                ListName = list.Name,
                ActionUserName = user.Name,
                ActionUserImageUri = user.ImageUri,
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(LeaveAsync)}");
            throw;
        }
    }

    public async Task<int> CopyAsync(CopyList model, IValidator<CopyList> validator)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        try
        {
            var list = _mapper.Map<ToDoList>(model);

            list.Name = list.Name.Trim();
            list.CreatedDate = list.ModifiedDate = DateTime.UtcNow;

            return await _listsRepository.CopyAsync(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CopyAsync)}");
            throw;
        }
    }

    public async Task SetIsArchivedAsync(int id, int userId, bool isArchived)
    {
        try
        {
            if (!UserOwnsOrShares(id, userId))
            {
                throw new ValidationException("Unauthorized");
            }

            await _listsRepository.SetIsArchivedAsync(id, userId, isArchived, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(SetIsArchivedAsync)}");
            throw;
        }
    }

    public async Task<SetTasksAsNotCompletedResult> UncompleteAllAsync(int id, int userId)
    {
        try
        {
            if (!UserOwnsOrShares(id, userId))
            {
                throw new ValidationException("Unauthorized");
            }

            bool nonPrivateTasksWereUncompleted = await _listsRepository.UncompleteAllAsync(id, userId, DateTime.UtcNow);
            if (!nonPrivateTasksWereUncompleted)
            {
                return new SetTasksAsNotCompletedResult();
            }

            var usersToBeNotified = _listsRepository.GetUsersToBeNotifiedOfChange(id, userId).ToList();
            if (!usersToBeNotified.Any())
            {
                return new SetTasksAsNotCompletedResult();
            }

            ToDoList list = _listsRepository.Get(id);

            var user = _userService.Get(userId);
            var result = new SetTasksAsNotCompletedResult
            {
                ListName = list.Name,
                ActionUserName = user.Name,
                ActionUserImageUri = user.ImageUri,
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UncompleteAllAsync)}");
            throw;
        }
    }

    public async Task<SetShareIsAcceptedResult> SetShareIsAcceptedAsync(int id, int userId, bool isAccepted)
    {
        try
        {
            await _listsRepository.SetShareIsAcceptedAsync(id, userId, isAccepted, DateTime.UtcNow);

            var usersToBeNotified = _listsRepository.GetUsersToBeNotifiedOfChange(id, userId).ToList();
            if (!usersToBeNotified.Any())
            {
                return new SetShareIsAcceptedResult();
            }

            ToDoList list = _listsRepository.Get(id);

            var user = _userService.Get(userId);
            var result = new SetShareIsAcceptedResult
            {
                ListName = list.Name,
                ActionUserName = user.Name,
                ActionUserImageUri = user.ImageUri,
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(SetShareIsAcceptedAsync)}");
            throw;
        }
    }

    public async Task ReorderAsync(int id, int userId, short oldOrder, short newOrder)
    {
        try
        {
            if (!UserOwnsOrShares(id, userId))
            {
                throw new ValidationException("Unauthorized");
            }

            await _listsRepository.ReorderAsync(id, userId, oldOrder, newOrder, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(ReorderAsync)}");
            throw;
        }
    }
}
