using AutoMapper;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using Core.Application.Utils;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Sentry;
using ToDoAssistant.Application.Contracts;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Contracts.Lists.Models;
using ToDoAssistant.Application.Contracts.Notifications;
using ToDoAssistant.Application.Contracts.Tasks;
using ToDoAssistant.Application.Entities;
using User = Core.Application.Entities.User;

namespace ToDoAssistant.Application.Services;

public class ListService : IListService
{
    private readonly IUserService _userService;
    private readonly IListsRepository _listsRepository;
    private readonly ITasksRepository _tasksRepository;
    private readonly INotificationsRepository _notificationsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ListService> _logger;

    public ListService(
        IUserService? userService,
        IListsRepository? listsRepository,
        ITasksRepository? tasksRepository,
        INotificationsRepository? notificationsRepository,
        IMapper? mapper,
        ILogger<ListService>? logger)
    {
        _userService = ArgValidator.NotNull(userService);
        _listsRepository = ArgValidator.NotNull(listsRepository);
        _tasksRepository = ArgValidator.NotNull(tasksRepository);
        _notificationsRepository = ArgValidator.NotNull(notificationsRepository);
        _mapper = ArgValidator.NotNull(mapper);
        _logger = ArgValidator.NotNull(logger);
    }

    public static HashSet<string> IconOptions => new HashSet<string> { "list", "shopping-cart", "shopping-bag", "home", "birthday", "cheers", "vacation", "passport", "plane", "car", "pickup-truck", "world", "camping", "tree", "motorcycle", "bicycle", "workout", "ski", "snowboard", "swimming", "work", "baby", "dog", "cat", "bird", "fish", "camera", "medicine", "file", "book", "mountain", "facebook", "twitter", "instagram", "tiktok" };

    public Result<IReadOnlyList<ListDto>> GetAll(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(GetAll)}");

        try
        {
            IReadOnlyList<ToDoList> lists = _listsRepository.GetAllWithTasksAndSharingDetails(userId, metric);

            var result = lists.Select(x => _mapper.Map<ListDto>(x, opts => opts.Items["UserId"] = userId)).ToList();

            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetAll)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public Result<IReadOnlyList<ToDoListOption>> GetAllAsOptions(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(GetAllAsOptions)}");

        try
        {
            IReadOnlyList<ToDoList> lists = _listsRepository.GetAllAsOptions(userId, metric);

            var result = lists.Select(x => _mapper.Map<ToDoListOption>(x)).ToList();

            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetAllAsOptions)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public Result<IReadOnlyList<Assignee>?> GetMembersAsAssigneeOptions(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(GetMembersAsAssigneeOptions)}");

        try
        {
            var ownsOrShares = _listsRepository.UserOwnsOrShares(id, userId);
            if (!ownsOrShares)
            {
                metric.Status = SpanStatus.PermissionDenied;
                return new(null);
            }

            IReadOnlyList<User> members = _listsRepository.GetMembersAsAssigneeOptions(id, metric);

            var result = members.Select(x => _mapper.Map<Assignee>(x)).ToList();

            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetMembersAsAssigneeOptions)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public Result<SimpleList> Get(int id)
    {
        try
        {
            ToDoList list = _listsRepository.Get(id);

            var result = _mapper.Map<SimpleList>(list);

            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Get)}");
            return new();
        }
    }

    public Result<EditListDto?> GetForEdit(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(GetForEdit)}");

        try
        {
            ToDoList? list = _listsRepository.GetWithShares(id, userId, metric);

            var result = _mapper.Map<EditListDto>(list, opts => opts.Items["UserId"] = userId);

            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetForEdit)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public Result<ListWithShares?> GetWithShares(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(GetWithShares)}");

        try
        {
            ToDoList? list = _listsRepository.GetWithOwner(id, userId, metric);
            if (list is null)
            {
                return new(null);
            }

            list.Shares.AddRange(_listsRepository.GetShares(id, metric));

            var result = _mapper.Map<ListWithShares>(list, opts => opts.Items["UserId"] = userId);
            result.Shares.RemoveAll(x => x.UserId == userId);

            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetWithShares)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public Result<IReadOnlyList<ShareListRequest>> GetShareRequests(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(GetShareRequests)}");

        try
        {
            IReadOnlyList<ListShare> shareRequests = _listsRepository.GetShareRequests(userId, metric);

            var result = shareRequests.Select(x => _mapper.Map<ShareListRequest>(x, opts => opts.Items["UserId"] = userId)).ToList();

            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetShareRequests)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public Result<int> GetPendingShareRequestsCount(int userId)
    {
        try
        {
            var count = _listsRepository.GetPendingShareRequestsCount(userId);
            return new(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetPendingShareRequestsCount)}");
            return new();
        }
    }

    public Result<bool> CanShareWithUser(int shareWithId, int userId)
    {
        try
        {
            if (shareWithId == userId)
            {
                return new(false);
            }

            var canShare = _listsRepository.CanShareWithUser(shareWithId, userId);
            return new(canShare);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CanShareWithUser)}");
            return new();
        }
    }

    public Result<bool> UserOwnsOrShares(int id, int userId)
    {
        try
        {
            var ownsOrShares = _listsRepository.UserOwnsOrShares(id, userId);
            return new(ownsOrShares);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UserOwnsOrShares)}");
            return new();
        }
    }

    public Result<bool> UserOwnsOrSharesAsPending(int id, int userId)
    {
        try
        {
            var ownsOrShares = _listsRepository.UserOwnsOrSharesAsPending(id, userId);
            return new(ownsOrShares);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UserOwnsOrSharesAsPending)}");
            return new();
        }
    }

    public Result<bool> UserOwnsOrSharesAsAdmin(int id, int userId)
    {
        try
        {
            var ownsOrShares = _listsRepository.UserOwnsOrSharesAsAdmin(id, userId);
            return new(ownsOrShares);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UserOwnsOrSharesAsAdmin)}");
            return new();
        }
    }

    public Result<bool> UserOwnsOrSharesAsAdmin(int id, string name, int userId)
    {
        try
        {
            var ownsOrShares = _listsRepository.UserOwnsOrSharesAsAdmin(id, name.Trim(), userId);
            return new(ownsOrShares);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UserOwnsOrSharesAsAdmin)}");
            return new();
        }
    }

    public Result<bool?> IsShared(int id, int userId)
    {
        try
        {
            var ownsOrShares = _listsRepository.UserOwnsOrShares(id, userId);
            if (!ownsOrShares)
            {
                return new(null);
            }

            var shared = _listsRepository.IsShared(id);
            return new(shared);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(IsShared)}");
            return new();
        }
    }

    public Result<bool> Exists(string name, int userId)
    {
        try
        {
            var exists = _listsRepository.Exists(name.Trim(), userId);
            return new(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Exists)}");
            return new();
        }
    }

    public Result<bool> Exists(int id, string name, int userId)
    {
        try
        {
            var exists = _listsRepository.Exists(id, name.Trim(), userId);
            return new(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Exists)}");
            return new();
        }
    }

    public Result<int> Count(int userId)
    {
        try
        {
            var count = _listsRepository.Count(userId);
            return new(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Count)}");
            return new();
        }
    }

    public Result<IReadOnlyList<User>> GetUsersToBeNotifiedOfChange(int id, int excludeUserId, bool isPrivate, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(GetUsersToBeNotifiedOfChange)}");

        try
        {
            if (isPrivate)
            {
                return new(new List<User>());
            }

            var result = _listsRepository.GetUsersToBeNotifiedOfChange(id, excludeUserId, metric);
            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetUsersToBeNotifiedOfChange)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public Result<IReadOnlyList<User>> GetUsersToBeNotifiedOfChange(int id, int excludeUserId, int taskId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(GetUsersToBeNotifiedOfChange)}");

        try
        {
            if (_tasksRepository.IsPrivate(taskId, excludeUserId))
            {
                return new(new List<User>());
            }

            var result = _listsRepository.GetUsersToBeNotifiedOfChange(id, excludeUserId, metric);
            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetUsersToBeNotifiedOfChange)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public Result<bool> CheckIfUserCanBeNotifiedOfChange(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(CheckIfUserCanBeNotifiedOfChange)}");

        try
        {
            var canBeNotified = _listsRepository.CheckIfUserCanBeNotifiedOfChange(id, userId, metric);
            return new(canBeNotified);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CheckIfUserCanBeNotifiedOfChange)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<ValidatedResult<int>> CreateAsync(CreateList model, IValidator<CreateList> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(CreateAsync)}");

        try
        {
            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
            {
                return new(validationResult.Errors);
            }

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

            var id = await _listsRepository.CreateAsync(list, metric, cancellationToken);

            return new(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<Result> CreateSampleAsync(int userId, Dictionary<string, string> translations, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(CreateSampleAsync)}");

        try
        {
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
            await _listsRepository.CreateAsync(list, metric, cancellationToken);

            return new Result(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateSampleAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<UpdateListResult> UpdateAsync(UpdateList model, IValidator<UpdateList> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(UpdateAsync)}");

        try
        {
            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
            {
                return new(validationResult.Errors);
            }

            var list = _mapper.Map<ToDoList>(model);

            list.Name = list.Name.Trim();
            list.ModifiedDate = DateTime.UtcNow;

            ToDoList original = await _listsRepository.UpdateAsync(list, model.UserId, metric, cancellationToken);

            var usersToBeNotified = _listsRepository.GetUsersToBeNotifiedOfChange(model.Id, model.UserId, metric);
            if (!usersToBeNotified.Any())
            {
                return new UpdateListResult(ResultStatus.Successful);
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

            var userResult = _userService.Get(model.UserId);
            if (userResult.Failed)
            {
                throw new Exception("User retrieval failed");
            }

            var result = new UpdateListResult(ResultStatus.Successful)
            {
                Type = notificationType,
                OriginalListName = original.Name,
                ActionUserName = userResult.Data!.Name,
                ActionUserImageUri = userResult.Data.ImageUri,
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient(x.Id, x.Language)).ToList()
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateAsync)}");
            return new(ResultStatus.Error);
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<ValidatedResult> UpdateSharedAsync(UpdateSharedList model, IValidator<UpdateSharedList> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(UpdateSharedAsync)}");

        try
        {
            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
            {
                return new(validationResult.Errors);
            }

            var list = _mapper.Map<ToDoList>(model);

            list.ModifiedDate = DateTime.UtcNow;
            await _listsRepository.UpdateSharedAsync(list, metric, cancellationToken);

            return new(ResultStatus.Successful);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateSharedAsync)}");
            return new(ResultStatus.Error);
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<DeleteListResult> DeleteAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(DeleteAsync)}");

        try
        {
            if (!_listsRepository.UserOwns(id, userId))
            {
                metric.Status = SpanStatus.PermissionDenied;
                return new();
            }

            var usersToBeNotified = _listsRepository.GetUsersToBeNotifiedOfDeletion(id, metric);

            string deletedListName = await _listsRepository.DeleteAsync(id, metric, cancellationToken);

            if (!usersToBeNotified.Any())
            {
                return new DeleteListResult(true);
            }

            var userResult = _userService.Get(userId);
            if (userResult.Failed)
            {
                throw new Exception("User retrieval failed");
            }

            var result = new DeleteListResult(true)
            {
                DeletedListName = deletedListName,
                ActionUserName = userResult.Data!.Name,
                ActionUserImageUri = userResult.Data.ImageUri,
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient(x.Id, x.Language)).ToList()
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<ValidatedResult> ShareAsync(ShareList model, IValidator<ShareList> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(ShareAsync)}");

        try
        {
            var now = DateTime.UtcNow;

            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
            {
                return new(validationResult.Errors);
            }

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
            }).ToList();

            var removedShares = model.RemovedShares.Select(x => new ListShare
            {
                ListId = model.ListId,
                UserId = x.UserId,
                IsAdmin = x.IsAdmin
            }).ToList();

            await _listsRepository.SaveSharingDetailsAsync(newShares, editedShares, removedShares, metric, cancellationToken);

            foreach (ListShare share in removedShares)
            {
                await _notificationsRepository.DeleteForUserAndListAsync(share.UserId, share.ListId, metric, cancellationToken);
            }

            return new(ResultStatus.Successful);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(ShareAsync)}");
            return new(ResultStatus.Error);
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<LeaveListResult> LeaveAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(LeaveAsync)}");

        try
        {
            ListShare share = await _listsRepository.LeaveAsync(id, userId, metric, cancellationToken);

            if (share.IsAccepted == false)
            {
                return new LeaveListResult(true);
            }

            await _notificationsRepository.DeleteForUserAndListAsync(userId, id, metric, cancellationToken);

            var usersToBeNotified = _listsRepository.GetUsersToBeNotifiedOfChange(id, userId, metric);
            if (!usersToBeNotified.Any())
            {
                return new LeaveListResult(true);
            }

            ToDoList list = _listsRepository.Get(id);

            var userResult = _userService.Get(userId);
            if (userResult.Failed)
            {
                throw new Exception("User retrieval failed");
            }

            var result = new LeaveListResult(true)
            {
                ListName = list.Name,
                ActionUserName = userResult.Data!.Name,
                ActionUserImageUri = userResult.Data.ImageUri,
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient(x.Id, x.Language)).ToList()
            };

            return result
;        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(LeaveAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<ValidatedResult<int>> CopyAsync(CopyList model, IValidator<CopyList> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(CopyAsync)}");

        try
        {
            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
            {
                return new(validationResult.Errors);
            }

            var list = _mapper.Map<ToDoList>(model);

            list.Name = list.Name.Trim();
            list.CreatedDate = list.ModifiedDate = DateTime.UtcNow;

            var id = await _listsRepository.CopyAsync(list, metric, cancellationToken);
            return new(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CopyAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<Result> SetIsArchivedAsync(int id, int userId, bool isArchived, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(SetIsArchivedAsync)}");

        try
        {
            var ownsOrShares = _listsRepository.UserOwnsOrShares(id, userId);
            if (!ownsOrShares)
            {
                metric.Status = SpanStatus.PermissionDenied;
                return new();
            }

            await _listsRepository.SetIsArchivedAsync(id, userId, isArchived, DateTime.UtcNow, metric, cancellationToken);

            return new Result(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(SetIsArchivedAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<SetTasksAsNotCompletedResult> UncompleteAllAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(UncompleteAllAsync)}");

        try
        {
            var ownsOrShares = _listsRepository.UserOwnsOrShares(id, userId);
            if (!ownsOrShares)
            {
                metric.Status = SpanStatus.PermissionDenied;
                return new();
            }

            bool nonPrivateTasksWereUncompleted = await _listsRepository.UncompleteAllAsync(id, userId, DateTime.UtcNow, metric, cancellationToken);
            if (!nonPrivateTasksWereUncompleted)
            {
                return new SetTasksAsNotCompletedResult(true);
            }

            var usersToBeNotified = _listsRepository.GetUsersToBeNotifiedOfChange(id, userId, metric);
            if (!usersToBeNotified.Any())
            {
                return new SetTasksAsNotCompletedResult(true);
            }

            ToDoList list = _listsRepository.Get(id);

            var userResult = _userService.Get(userId);
            if (userResult.Failed)
            {
                throw new Exception("User retrieval failed");
            }

            var result = new SetTasksAsNotCompletedResult(true)
            {
                ListName = list.Name,
                ActionUserName = userResult.Data!.Name,
                ActionUserImageUri = userResult.Data.ImageUri,
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient(x.Id, x.Language)).ToList()
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UncompleteAllAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<SetShareIsAcceptedResult> SetShareIsAcceptedAsync(int id, int userId, bool isAccepted, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(ListService)}.{nameof(SetShareIsAcceptedAsync)}");

        try
        {
            await _listsRepository.SetShareIsAcceptedAsync(id, userId, isAccepted, DateTime.UtcNow, metric, cancellationToken);

            var usersToBeNotified = _listsRepository.GetUsersToBeNotifiedOfChange(id, userId, metric);
            if (!usersToBeNotified.Any())
            {
                return new SetShareIsAcceptedResult(true);
            }

            ToDoList list = _listsRepository.Get(id);

            var userResult = _userService.Get(userId);
            if (userResult.Failed)
            {
                throw new Exception("User retrieval failed");
            }

            var result = new SetShareIsAcceptedResult(true)
            {
                ListName = list.Name,
                ActionUserName = userResult.Data!.Name,
                ActionUserImageUri = userResult.Data.ImageUri,
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient(x.Id, x.Language)).ToList()
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(SetShareIsAcceptedAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<Result> ReorderAsync(int id, int userId, short oldOrder, short newOrder, CancellationToken cancellationToken)
    {
        try
        {
            var ownsOrShares = _listsRepository.UserOwnsOrShares(id, userId);
            if (!ownsOrShares)
            {
                return new();
            }

            await _listsRepository.ReorderAsync(id, userId, oldOrder, newOrder, DateTime.UtcNow, cancellationToken);

            return new Result(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(ReorderAsync)}");
            return new();
        }
    }
}
