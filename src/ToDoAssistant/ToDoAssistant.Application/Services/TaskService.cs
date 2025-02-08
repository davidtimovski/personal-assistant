using AutoMapper;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using Core.Application.Utils;
using FluentValidation;
using Microsoft.Extensions.Logging;
using ToDoAssistant.Application.Contracts.Lists;
using ToDoAssistant.Application.Contracts.Tasks;
using ToDoAssistant.Application.Contracts.Tasks.Models;
using ToDoAssistant.Application.Entities;

namespace ToDoAssistant.Application.Services;

public class TaskService : ITaskService
{
    private readonly IUserService _userService;
    private readonly IListService _listService;
    private readonly ITasksRepository _tasksRepository;
    private readonly IListsRepository _listsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        IUserService? userService,
        IListService? listService,
        ITasksRepository? tasksRepository,
        IListsRepository? listsRepository,
        IMapper? mapper,
        ILogger<TaskService>? logger)
    {
        _userService = ArgValidator.NotNull(userService);
        _listService = ArgValidator.NotNull(listService);
        _tasksRepository = ArgValidator.NotNull(tasksRepository);
        _listsRepository = ArgValidator.NotNull(listsRepository);
        _mapper = ArgValidator.NotNull(mapper);
        _logger = ArgValidator.NotNull(logger);
    }

    public Result<SimpleTask> Get(int id)
    {
        try
        {
            ToDoTask task = _tasksRepository.Get(id);

            var result = _mapper.Map<SimpleTask>(task);

            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Get)}");
            return new();
        }
    }

    public Result<TaskDto?> Get(int id, int userId)
    {
        try
        {
            ToDoTask? task = _tasksRepository.Get(id, userId);

            var result = _mapper.Map<TaskDto>(task);

            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Get)}");
            return new();
        }
    }

    public Result<TaskForUpdate?> GetForUpdate(int id, int userId)
    {
        try
        {
            var task = _tasksRepository.GetForUpdate(id, userId);
            if (task is null)
            {
                return new(null);
            }

            var result = _mapper.Map<TaskForUpdate>(task, opts => opts.Items["UserId"] = userId);

            var isSharedResult = _listService.IsShared(task.ListId, userId);
            if (isSharedResult.Failed)
            {
                return new();
            }

            if (isSharedResult.Data is null)
            {
                return new(null);
            }

            result.IsInSharedList = isSharedResult.Data.Value;
            result.Recipes = _tasksRepository.GetRecipes(id, userId);

            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetForUpdate)}");
            return new();
        }
    }

    public Result<bool> Exists(int id, int userId)
    {
        try
        {
            var result = _tasksRepository.Exists(id, userId);
            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Exists)}");
            return new();
        }
    }

    public Result<bool> Exists(string name, int listId, int userId)
    {
        try
        {
            var result = _tasksRepository.Exists(name.Trim(), listId, userId);
            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Exists)}");
            return new();
        }
    }

    public Result<bool> Exists(List<string> names, int listId, int userId)
    {
        try
        {
            var upperCaseNames = names.Select(name => name.Trim().ToUpperInvariant()).ToList();
            var result = _tasksRepository.Exists(upperCaseNames, listId, userId);
            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Exists)}");
            return new();
        }
    }

    public Result<bool> Exists(int id, string name, int listId, int userId)
    {
        try
        {
            var result = _tasksRepository.Exists(id, name.Trim(), listId, userId);
            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Exists)}");
            return new();
        }
    }

    public Result<int> Count(int listId)
    {
        try
        {
            var count = _tasksRepository.Count(listId);
            return new(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Count)}");
            return new();
        }
    }

    public async Task<CreatedTaskResult> CreateAsync(CreateTask model, IValidator<CreateTask> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(TaskService)}.{nameof(CreateAsync)}");

        try
        {
            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
            {
                return new(validationResult.Errors);
            }

            var task = _mapper.Map<ToDoTask>(model);

            task.Name = task.Name.Trim();
            task.Url = string.IsNullOrEmpty(task.Url) ? null : task.Url.Trim();

            var isSharedResult = _listService.IsShared(task.ListId, model.UserId);
            if (isSharedResult.Failed || isSharedResult.Data is null)
            {
                return new(ResultStatus.Error);
            }

            if (!isSharedResult.Data.Value)
            {
                task.PrivateToUserId = null;
                task.AssignedToUserId = null;
            }

            task.Order = 1;
            task.CreatedDate = task.ModifiedDate = DateTime.UtcNow;

            var id = await _tasksRepository.CreateAsync(task, model.UserId, metric, cancellationToken);

            ToDoList? list = _listsRepository.GetWithShares(model.ListId, model.UserId, metric);
            if (list is null)
            {
                return new(ResultStatus.Error);
            }

            var notifySignalR = !task.PrivateToUserId.HasValue && list.IsShared;

            var usersToBeNotifiedResult = _listService.GetUsersToBeNotifiedOfChange(model.ListId, model.UserId, model.IsPrivate == true, metric);
            if (usersToBeNotifiedResult.Failed)
            {
                return new CreatedTaskResult(ResultStatus.Successful) { TaskId = id, ListId = task.ListId, NotifySignalR = notifySignalR };
            }

            if (!usersToBeNotifiedResult.Data!.Any())
            {
                return new(ResultStatus.Successful);
            }

            var userResult = _userService.Get(model.UserId);
            if (userResult.Failed)
            {
                throw new Exception("User retrieval failed");
            }

            var result = new CreatedTaskResult(ResultStatus.Successful)
            {
                TaskId = id,
                ListId = task.ListId,
                NotifySignalR = notifySignalR,
                TaskName = task.Name,
                ListName = list.Name,
                ActionUserName = userResult.Data!.Name,
                ActionUserImageUri = userResult.Data.ImageUri,
                NotificationRecipients = usersToBeNotifiedResult.Data!.Select(x => new NotificationRecipient(x.Id, x.Language)).ToList()
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateAsync)}");
            return new(ResultStatus.Error);
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<BulkCreateResult> BulkCreateAsync(BulkCreate model, IValidator<BulkCreate> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(TaskService)}.{nameof(BulkCreateAsync)}");

        try
        {
            var now = DateTime.UtcNow;

            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
            {
                return new(validationResult.Errors);
            }

            var tasks = model.TasksText.Split("\n")
                .Where(task => !string.IsNullOrWhiteSpace(task))
                .Select(task => new ToDoTask
                {
                    ListId = model.ListId,
                    Name = task.Trim(),
                    IsOneTime = model.TasksAreOneTime,
                    PrivateToUserId = model.TasksArePrivate ? model.UserId : null,
                    AssignedToUserId = null,
                    CreatedDate = now,
                    ModifiedDate = now
                }
                ).ToList();

            var createdTasks = await _tasksRepository.BulkCreateAsync(tasks, model.TasksArePrivate, model.UserId, metric, cancellationToken);

            ToDoList? list = _listsRepository.GetWithShares(model.ListId, model.UserId, metric);
            if (list is null)
            {
                return new(ResultStatus.Error);
            }

            var usersToBeNotifiedResult = _listService.GetUsersToBeNotifiedOfChange(model.ListId, model.UserId, model.TasksArePrivate, metric);
            if (usersToBeNotifiedResult.Failed)
            {
                return new(ResultStatus.Error);
            }

            var notifySignalR = !tasks[0].PrivateToUserId.HasValue && list.IsShared;
            if (!usersToBeNotifiedResult.Data!.Any())
            {
                return new BulkCreateResult(ResultStatus.Successful) { ListId = list.Id, NotifySignalR = notifySignalR };
            }

            var userResult = _userService.Get(model.UserId);
            if (userResult.Failed)
            {
                throw new Exception("User retrieval failed");
            }

            var result = new BulkCreateResult(ResultStatus.Successful)
            {
                ListId = list.Id,
                NotifySignalR = notifySignalR,
                ListName = list.Name,
                CreatedTasks = createdTasks.Select(x => new BulkCreatedTask
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToList(),
                ActionUserName = userResult.Data!.Name,
                ActionUserImageUri = userResult.Data.ImageUri,
                NotificationRecipients = usersToBeNotifiedResult.Data!.Select(x => new NotificationRecipient(x.Id, x.Language)).ToList()
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(BulkCreateAsync)}");
            return new(ResultStatus.Error);
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<UpdateTaskResult> UpdateAsync(UpdateTask model, IValidator<UpdateTask> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(TaskService)}.{nameof(UpdateAsync)}");

        try
        {
            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
            {
                return new(validationResult.Errors);
            }

            var task = _mapper.Map<ToDoTask>(model);

            task.Name = task.Name.Trim();
            task.Url = string.IsNullOrEmpty(task.Url) ? null : task.Url.Trim();

            if (model.IsPrivate == true)
            {
                task.AssignedToUserId = null;
            }

            var isSharedResult = _listService.IsShared(task.ListId, model.UserId);
            if (isSharedResult.Failed || isSharedResult.Data is null)
            {
                return new(ResultStatus.Error);
            }

            if (!isSharedResult.Data.Value)
            {
                task.PrivateToUserId = null;
                task.AssignedToUserId = null;
            }

            ToDoTask originalTask = _tasksRepository.Get(model.Id);

            task.ModifiedDate = DateTime.UtcNow;
            await _tasksRepository.UpdateAsync(task, model.UserId, metric, cancellationToken);

            ToDoList? list = _listsRepository.GetWithShares(model.ListId, model.UserId, metric);
            if (list is null)
            {
                return new(ResultStatus.Error);
            }

            var notifySignalR = !task.PrivateToUserId.HasValue && list.IsShared;
            var result = new UpdateTaskResult(ResultStatus.Successful) { OriginalTaskName = originalTask.Name, ListId = list.Id, ListName = list.Name, NotifySignalR = notifySignalR };

            if (model.ListId == originalTask.ListId)
            {
                var usersToBeNotifiedResult = _listService.GetUsersToBeNotifiedOfChange(model.ListId, model.UserId, model.Id, metric);
                if (usersToBeNotifiedResult.Failed)
                {
                    return new(ResultStatus.Error);
                }

                result.NotificationRecipients = usersToBeNotifiedResult.Data!.Select(x => new NotificationRecipient(x.Id, x.Language)).ToList();
            }
            else
            {
                ToDoList? oldList = _listsRepository.Get(originalTask.ListId);
                if (oldList is null)
                {
                    throw new Exception("Original list could not be found");
                }

                result.OldListId = originalTask.ListId;
                result.OldListName = oldList.Name;

                var usersToBeNotifiedOfRemovalResult = _listService.GetUsersToBeNotifiedOfChange(oldList.Id, model.UserId, model.Id, metric);
                if (usersToBeNotifiedOfRemovalResult.Failed)
                {
                    return new(ResultStatus.Error);
                }
                result.RemovedNotificationRecipients = usersToBeNotifiedOfRemovalResult.Data!.Select(x => new NotificationRecipient(x.Id, x.Language)).ToList();

                var usersToBeNotifiedOfCreationResult = _listService.GetUsersToBeNotifiedOfChange(model.ListId, model.UserId, model.Id, metric);
                if (usersToBeNotifiedOfCreationResult.Failed)
                {
                    return new(ResultStatus.Error);
                }
                result.CreatedNotificationRecipients = usersToBeNotifiedOfCreationResult.Data!.Select(x => new NotificationRecipient(x.Id, x.Language)).ToList();
            }

            if (model.AssignedToUserId.HasValue
                && model.AssignedToUserId.Value != originalTask.AssignedToUserId
                && model.AssignedToUserId.Value != model.UserId)
            {
                var canBeNotifiedResult = _listService.CheckIfUserCanBeNotifiedOfChange(model.ListId, model.AssignedToUserId.Value, metric);
                if (canBeNotifiedResult.Failed)
                {
                    return new(ResultStatus.Error);
                }

                if (canBeNotifiedResult.Data)
                {
                    var assignedUserResult = _userService.Get(model.AssignedToUserId.Value);
                    if (assignedUserResult.Failed)
                    {
                        throw new Exception("User retrieval failed");
                    }

                    result.AssignedNotificationRecipient = new NotificationRecipient(assignedUserResult.Data!.Id, assignedUserResult.Data.Language);
                }
            }

            if (result.Notify())
            {
                var userResult = _userService.Get(model.UserId);
                if (userResult.Failed)
                {
                    throw new Exception("User retrieval failed");
                }

                result.ActionUserName = userResult.Data!.Name;
                result.ActionUserImageUri = userResult.Data.ImageUri;
            }

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

    public async Task<DeleteTaskResult> DeleteAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(TaskService)}.{nameof(DeleteAsync)}");

        try
        {
            ToDoTask task = _tasksRepository.Get(id);
            if (task is null)
            {
                return new DeleteTaskResult(true);
            }

            var exists = _tasksRepository.Exists(id, userId);
            if (!exists)
            {
                metric.Status = SpanStatus.PermissionDenied;
                return new();
            }

            await _tasksRepository.DeleteAsync(id, userId, metric, cancellationToken);

            ToDoList? list = _listsRepository.GetWithShares(task.ListId, userId, metric);
            if (list is null)
            {
                return new();
            }

            var usersToBeNotifiedResult = _listService.GetUsersToBeNotifiedOfChange(task.ListId, userId, task.PrivateToUserId == userId, metric);
            if (usersToBeNotifiedResult.Failed)
            {
                return new();
            }

            var notifySignalR = !task.PrivateToUserId.HasValue && list.IsShared;
            if (!usersToBeNotifiedResult.Data!.Any())
            {
                return new DeleteTaskResult(true) { ListId = task.ListId, NotifySignalR = notifySignalR };
            }

            var userResult = _userService.Get(userId);
            if (userResult.Failed)
            {
                throw new Exception("User retrieval failed");
            }

            var result = new DeleteTaskResult(true)
            {
                ListId = task.ListId,
                NotifySignalR = notifySignalR,
                TaskName = task.Name,
                ListName = list.Name,
                ActionUserName = userResult.Data!.Name,
                ActionUserImageUri = userResult.Data.ImageUri,
                NotificationRecipients = usersToBeNotifiedResult.Data!.Select(x => new NotificationRecipient(x.Id, x.Language)).ToList()
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

    public async Task<CompleteUncompleteTaskResult> CompleteAsync(CompleteUncomplete model, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(TaskService)}.{nameof(CompleteAsync)}");

        try
        {
            var exists = _tasksRepository.Exists(model.Id, model.UserId);
            if (!exists)
            {
                metric.Status = SpanStatus.PermissionDenied;
                return new();
            }

            ToDoTask task = _tasksRepository.Get(model.Id);
            if (task.IsCompleted)
            {
                return new CompleteUncompleteTaskResult(false);
            }

            await _tasksRepository.CompleteAsync(model.Id, model.UserId, metric, cancellationToken);

            ToDoList? list = _listsRepository.GetWithShares(task.ListId, model.UserId, metric);
            if (list is null)
            {
                return new();
            }

            var usersToBeNotifiedResult = _listService.GetUsersToBeNotifiedOfChange(task.ListId, model.UserId, model.Id, metric);
            if (usersToBeNotifiedResult.Failed)
            {
                return new();
            }

            var notifySignalR = !task.PrivateToUserId.HasValue && list.IsShared;
            if (!usersToBeNotifiedResult.Data!.Any())
            {
                return new CompleteUncompleteTaskResult(true) { ListId = task.ListId, NotifySignalR = notifySignalR };
            }

            var userResult = _userService.Get(model.UserId);
            if (userResult.Failed)
            {
                throw new Exception("User retrieval failed");
            }

            var result = new CompleteUncompleteTaskResult(true)
            {
                ListId = task.ListId,
                NotifySignalR = notifySignalR,
                TaskName = task.Name,
                ListName = list.Name,
                ActionUserName = userResult.Data!.Name,
                ActionUserImageUri = userResult.Data.ImageUri,
                NotificationRecipients = usersToBeNotifiedResult.Data!.Select(x => new NotificationRecipient(x.Id, x.Language)).ToList()
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CompleteAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<CompleteUncompleteTaskResult> UncompleteAsync(CompleteUncomplete model, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(TaskService)}.{nameof(UncompleteAsync)}");

        try
        {
            var exists = _tasksRepository.Exists(model.Id, model.UserId);
            if (!exists)
            {
                metric.Status = SpanStatus.PermissionDenied;
                return new();
            }

            ToDoTask task = _tasksRepository.Get(model.Id);
            if (!task.IsCompleted)
            {
                return new CompleteUncompleteTaskResult(true);
            }

            await _tasksRepository.UncompleteAsync(model.Id, model.UserId, metric, cancellationToken);

            ToDoList? list = _listsRepository.GetWithShares(task.ListId, model.UserId, metric);
            if (list is null)
            {
                return new();
            }

            var usersToBeNotifiedResult = _listService.GetUsersToBeNotifiedOfChange(task.ListId, model.UserId, model.Id, metric);
            if (usersToBeNotifiedResult.Failed)
            {
                return new();
            }

            var notifySignalR = !task.PrivateToUserId.HasValue && list.IsShared;
            if (!usersToBeNotifiedResult.Data!.Any())
            {
                return new CompleteUncompleteTaskResult(true) { ListId = task.ListId, NotifySignalR = notifySignalR };
            }

            var userResult = _userService.Get(model.UserId);
            if (userResult.Failed)
            {
                throw new Exception("User retrieval failed");
            }

            var result = new CompleteUncompleteTaskResult(true)
            {
                ListId = task.ListId,
                NotifySignalR = notifySignalR,
                TaskName = task.Name,
                ListName = list.Name,
                ActionUserName = userResult.Data!.Name,
                ActionUserImageUri = userResult.Data.ImageUri,
                NotificationRecipients = usersToBeNotifiedResult.Data!.Select(x => new NotificationRecipient(x.Id, x.Language)).ToList()
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UncompleteAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    //public async Task<Result<ReorderTaskResult>> ReorderAsync(ReorderTask model, ISpan metricsSpan, CancellationToken cancellationToken)
    //{
    //    var metric = metricsSpan.StartChild($"{nameof(TaskService)}.{nameof(ReorderAsync)}");

    //    try
    //    {
    //        var exists = _tasksRepository.Exists(model.Id, model.UserId);
    //        if (!exists)
    //        {
    //            metric.Status = SpanStatus.PermissionDenied;
    //            return new();
    //        }

    //        await _tasksRepository.ReorderAsync(model.Id, model.UserId, model.OldOrder, model.NewOrder, DateTime.UtcNow, cancellationToken);

    //        ToDoTask task = _tasksRepository.Get(model.Id);
    //        ToDoList? list = _listsRepository.GetWithShares(task.ListId, model.UserId, metric);
    //        if (list is null)
    //        {
    //            return new();
    //        }

    //        var notifySignalR = !task.PrivateToUserId.HasValue && list.IsShared;
    //        return new(new ReorderTaskResult(list.Id, notifySignalR));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, $"Unexpected error in {nameof(ReorderAsync)}");
    //        return new();
    //    }
    //    finally
    //    {
    //        metric.Finish();
    //    }
    //}
}
