using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Application.Contracts.Common;
using Application.Contracts.Common.Models;
using Application.Contracts.ToDoAssistant.Lists;
using Application.Contracts.ToDoAssistant.Lists.Models;
using Application.Contracts.ToDoAssistant.Notifications;
using Application.Contracts.ToDoAssistant.Tasks;
using Domain.Entities.Common;
using Domain.Entities.ToDoAssistant;

namespace Application.Services.ToDoAssistant;

public class ListService : IListService
{
    private readonly IUserService _userService;
    private readonly IListsRepository _listsRepository;
    private readonly ITasksRepository _tasksRepository;
    private readonly INotificationsRepository _notificationsRepository;
    private readonly IMapper _mapper;

    public ListService(
        IUserService userService,
        IListsRepository listsRepository,
        ITasksRepository tasksRepository,
        INotificationsRepository notificationsRepository,
        IMapper mapper)
    {
        _userService = userService;
        _listsRepository = listsRepository;
        _tasksRepository = tasksRepository;
        _notificationsRepository = notificationsRepository;
        _mapper = mapper;
    }

    public IEnumerable<ListDto> GetAll(int userId)
    {
        IEnumerable<ToDoList> lists = _listsRepository.GetAllWithTasksAndSharingDetails(userId);

        var result = lists.Select(x => _mapper.Map<ListDto>(x, opts => { opts.Items["UserId"] = userId; }));

        return result;
    }

    public IEnumerable<ToDoListOption> GetAllAsOptions(int userId)
    {
        IEnumerable<ToDoList> lists = _listsRepository.GetAllAsOptions(userId);

        var result = lists.Select(x => _mapper.Map<ToDoListOption>(x));

        return result;
    }

    public IEnumerable<AssigneeOption> GetMembersAsAssigneeOptions(int id, int userId)
    {
        if (!_listsRepository.UserOwnsOrShares(id, userId))
        {
            throw new ValidationException("Unauthorized");
        }

        IEnumerable<User> members = _listsRepository.GetMembersAsAssigneeOptions(id);

        var result = members.Select(x => _mapper.Map<AssigneeOption>(x));

        return result;
    }

    public SimpleList Get(int id)
    {
        ToDoList list = _listsRepository.Get(id);

        var result = _mapper.Map<SimpleList>(list);

        return result;
    }

    public EditListDto GetForEdit(int id, int userId)
    {
        ToDoList list = _listsRepository.GetWithShares(id, userId);

        var result = _mapper.Map<EditListDto>(list, opts => { opts.Items["UserId"] = userId; });

        return result;
    }

    public ListWithShares GetWithShares(int id, int userId)
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

    public IEnumerable<ShareListRequest> GetShareRequests(int userId)
    {
        IEnumerable<ListShare> shareRequests = _listsRepository.GetShareRequests(userId);

        var result = shareRequests.Select(x => _mapper.Map<ShareListRequest>(x, opts => { opts.Items["UserId"] = userId; }));

        return result;
    }

    public int GetPendingShareRequestsCount(int userId)
    {
        return _listsRepository.GetPendingShareRequestsCount(userId);
    }

    public bool CanShareWithUser(int shareWithId, int userId)
    {
        if (shareWithId == userId)
        {
            return false;
        }

        return _listsRepository.CanShareWithUser(shareWithId, userId);
    }

    public bool UserOwnsOrShares(int id, int userId)
    {
        return _listsRepository.UserOwnsOrShares(id, userId);
    }

    public bool UserOwnsOrSharesAsPending(int id, int userId)
    {
        return _listsRepository.UserOwnsOrSharesAsPending(id, userId);
    }

    public bool UserOwnsOrSharesAsAdmin(int id, int userId)
    {
        return _listsRepository.UserOwnsOrSharesAsAdmin(id, userId);
    }

    public bool UserOwnsOrSharesAsAdmin(int id, string name, int userId)
    {
        return _listsRepository.UserOwnsOrSharesAsAdmin(id, name.Trim(), userId);
    }

    public bool IsShared(int id, int userId)
    {
        if (!UserOwnsOrShares(id, userId))
        {
            throw new ValidationException("Unauthorized");
        }

        return _listsRepository.IsShared(id);
    }

    public bool Exists(string name, int userId)
    {
        return _listsRepository.Exists(name.Trim(), userId);
    }

    public bool Exists(int id, string name, int userId)
    {
        return _listsRepository.Exists(id, name.Trim(), userId);
    }

    public int Count(int userId)
    {
        return _listsRepository.Count(userId);
    }

    public IEnumerable<User> GetUsersToBeNotifiedOfChange(int id, int excludeUserId, bool isPrivate)
    {
        if (isPrivate)
        {
            return new List<User>();
        }

        return _listsRepository.GetUsersToBeNotifiedOfChange(id, excludeUserId);
    }

    public IEnumerable<User> GetUsersToBeNotifiedOfChange(int id, int excludeUserId, int taskId)
    {
        if (_tasksRepository.IsPrivate(taskId, excludeUserId))
        {
            return new List<User>();
        }

        return _listsRepository.GetUsersToBeNotifiedOfChange(id, excludeUserId);
    }

    public bool CheckIfUserCanBeNotifiedOfChange(int id, int userId)
    {
        return _listsRepository.CheckIfUserCanBeNotifiedOfChange(id, userId);
    }

    public async Task<int> CreateAsync(CreateList model, IValidator<CreateList> validator)
    {
        ValidateAndThrow(model, validator);

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

    public async Task CreateSampleAsync(int userId, Dictionary<string, string> translations)
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

    public async Task<UpdateListResult> UpdateAsync(UpdateList model, IValidator<UpdateList> validator)
    {
        ValidateAndThrow(model, validator);

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

        var result = new UpdateListResult
        {
            Type = notificationType,
            OriginalListName = original.Name,
            ActionUserImageUri = _userService.GetImageUri(model.UserId),
            NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
        };

        return result;
    }

    public async Task UpdateSharedAsync(UpdateSharedList model, IValidator<UpdateSharedList> validator)
    {
        ValidateAndThrow(model, validator);

        var list = _mapper.Map<ToDoList>(model);

        list.ModifiedDate = DateTime.UtcNow;
        await _listsRepository.UpdateSharedAsync(list);
    }

    public async Task<DeleteListResult> DeleteAsync(int id, int userId)
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

        var result = new DeleteListResult
        {
            DeletedListName = deletedListName,
            ActionUserImageUri = _userService.GetImageUri(userId),
            NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
        };

        return result;
    }

    public async Task ShareAsync(ShareList model, IValidator<ShareList> validator)
    {
        ValidateAndThrow(model, validator);

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

    public async Task<LeaveListResult> LeaveAsync(int id, int userId)
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

        var result = new LeaveListResult
        {
            ListName = list.Name,
            ActionUserImageUri = _userService.GetImageUri(userId),
            NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
        };

        return result;
    }

    public async Task<int> CopyAsync(CopyList model, IValidator<CopyList> validator)
    {
        ValidateAndThrow(model, validator);

        var list = _mapper.Map<ToDoList>(model);

        list.Name = list.Name.Trim();
        list.CreatedDate = list.ModifiedDate = DateTime.UtcNow;

        return await _listsRepository.CopyAsync(list);
    }

    public async Task SetIsArchivedAsync(int id, int userId, bool isArchived)
    {
        if (!UserOwnsOrShares(id, userId))
        {
            throw new ValidationException("Unauthorized");
        }

        await _listsRepository.SetIsArchivedAsync(id, userId, isArchived, DateTime.UtcNow);
    }

    public async Task<SetTasksAsNotCompletedResult> SetTasksAsNotCompletedAsync(int id, int userId)
    {
        if (!UserOwnsOrShares(id, userId))
        {
            throw new ValidationException("Unauthorized");
        }

        bool nonPrivateTasksWereUncompleted = await _listsRepository.SetTasksAsNotCompletedAsync(id, userId, DateTime.UtcNow);
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

        var result = new SetTasksAsNotCompletedResult
        {
            ListName = list.Name,
            ActionUserImageUri = _userService.GetImageUri(userId),
            NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
        };

        return result;
    }

    public async Task<SetShareIsAcceptedResult> SetShareIsAcceptedAsync(int id, int userId, bool isAccepted)
    {
        await _listsRepository.SetShareIsAcceptedAsync(id, userId, isAccepted, DateTime.UtcNow);

        var usersToBeNotified = _listsRepository.GetUsersToBeNotifiedOfChange(id, userId).ToList();
        if (!usersToBeNotified.Any())
        {
            return new SetShareIsAcceptedResult();
        }

        ToDoList list = _listsRepository.Get(id);

        var result = new SetShareIsAcceptedResult
        {
            ListName = list.Name,
            ActionUserImageUri = _userService.GetImageUri(userId),
            NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
        };

        return result;
    }

    public async Task ReorderAsync(int id, int userId, short oldOrder, short newOrder)
    {
        if (!UserOwnsOrShares(id, userId))
        {
            throw new ValidationException("Unauthorized");
        }

        await _listsRepository.ReorderAsync(id, userId, oldOrder, newOrder, DateTime.UtcNow);
    }

    private static void ValidateAndThrow<T>(T model, IValidator<T> validator)
    {
        ValidationResult result = validator.Validate(model);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}
