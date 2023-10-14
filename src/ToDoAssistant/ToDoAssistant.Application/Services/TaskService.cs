using AutoMapper;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using Core.Application.Utils;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Sentry;
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
        IUserService userService,
        IListService listService,
        ITasksRepository tasksRepository,
        IListsRepository listsRepository,
        IMapper mapper,
        ILogger<TaskService> logger)
    {
        _userService = userService;
        _listService = listService;
        _tasksRepository = tasksRepository;
        _listsRepository = listsRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public SimpleTask Get(int id)
    {
        try
        {
            ToDoTask task = _tasksRepository.Get(id);

            var result = _mapper.Map<SimpleTask>(task);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Get)}");
            throw;
        }
    }

    public TaskDto? Get(int id, int userId)
    {
        try
        {
            ToDoTask? task = _tasksRepository.Get(id, userId);

            var result = _mapper.Map<TaskDto>(task);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Get)}");
            throw;
        }
    }

    public TaskForUpdate? GetForUpdate(int id, int userId)
    {
        try
        {
            ToDoTask task = _tasksRepository.GetForUpdate(id, userId);
            if (task is null)
            {
                return null;
            }

            var result = _mapper.Map<TaskForUpdate>(task, opts => opts.Items["UserId"] = userId);

            result.IsInSharedList = _listService.IsShared(task.ListId, userId);
            result.Recipes = _tasksRepository.GetRecipes(id, userId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetForUpdate)}");
            throw;
        }
    }

    public bool Exists(int id, int userId)
    {
        try
        {
            return _tasksRepository.Exists(id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Exists)}");
            throw;
        }
    }

    public bool Exists(string name, int listId, int userId)
    {
        try
        {
            return _tasksRepository.Exists(name.Trim(), listId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Exists)}");
            throw;
        }
    }

    public bool Exists(IEnumerable<string> names, int listId, int userId)
    {
        try
        {
            var upperCaseNames = names.Select(name => name.Trim().ToUpperInvariant()).ToList();
            return _tasksRepository.Exists(upperCaseNames, listId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Exists)}");
            throw;
        }
    }

    public bool Exists(int id, string name, int listId, int userId)
    {
        try
        {
            return _tasksRepository.Exists(id, name.Trim(), listId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Exists)}");
            throw;
        }
    }

    public int Count(int listId)
    {
        try
        {
            return _tasksRepository.Count(listId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Count)}");
            throw;
        }
    }

    public async Task<CreatedTaskResult> CreateAsync(CreateTask model, IValidator<CreateTask> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        var metric = metricsSpan.StartChild($"{nameof(TaskService)}.{nameof(CreateAsync)}");

        try
        {
            var task = _mapper.Map<ToDoTask>(model);

            task.Name = task.Name.Trim();
            task.Url = string.IsNullOrEmpty(task.Url) ? null : task.Url.Trim();

            if (!_listService.IsShared(task.ListId, model.UserId))
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
                throw new InvalidOperationException("Cannot create task in non-existing list");
            }

            var notifySignalR = !task.PrivateToUserId.HasValue && list.IsShared;
            var result = new CreatedTaskResult(id, task.ListId, notifySignalR);

            var usersToBeNotified = _listService.GetUsersToBeNotifiedOfChange(model.ListId, model.UserId, model.IsPrivate == true, metric).ToList();
            if (!usersToBeNotified.Any())
            {
                return result;
            }

            var user = _userService.Get(model.UserId);

            result.TaskName = task.Name;
            result.ListName = list.Name;
            result.ActionUserName = user.Name;
            result.ActionUserImageUri = user.ImageUri;
            result.NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language }).ToList();

            return result;
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<BulkCreateResult> BulkCreateAsync(BulkCreate model, IValidator<BulkCreate> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        var metric = metricsSpan.StartChild($"{nameof(TaskService)}.{nameof(BulkCreateAsync)}");

        try
        {
            var now = DateTime.UtcNow;

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

            IEnumerable<ToDoTask> createdTasks = await _tasksRepository.BulkCreateAsync(tasks, model.TasksArePrivate, model.UserId, metric, cancellationToken);

            ToDoList? list = _listsRepository.GetWithShares(model.ListId, model.UserId, metric);
            if (list is null)
            {
                throw new InvalidOperationException("Cannot create tasks in non-existing list");
            }

            var usersToBeNotified = _listService.GetUsersToBeNotifiedOfChange(model.ListId, model.UserId, model.TasksArePrivate, metric).ToList();

            var notifySignalR = !tasks[0].PrivateToUserId.HasValue && list.IsShared;
            var result = new BulkCreateResult(list.Id, notifySignalR);

            if (!usersToBeNotified.Any())
            {
                return result;
            }

            var user = _userService.Get(model.UserId);

            result.ListName = list.Name;
            result.CreatedTasks.AddRange(createdTasks.Select(x => new BulkCreatedTask
            {
                Id = x.Id,
                Name = x.Name
            }));
            result.ActionUserName = user.Name;
            result.ActionUserImageUri = user.ImageUri;
            result.NotificationRecipients.AddRange(usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language }));

            return result;
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(BulkCreateAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<UpdateTaskResult> UpdateAsync(UpdateTask model, IValidator<UpdateTask> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        var metric = metricsSpan.StartChild($"{nameof(TaskService)}.{nameof(UpdateAsync)}");

        try
        {
            var task = _mapper.Map<ToDoTask>(model);

            task.Name = task.Name.Trim();
            task.Url = string.IsNullOrEmpty(task.Url) ? null : task.Url.Trim();

            if (model.IsPrivate == true)
            {
                task.AssignedToUserId = null;
            }

            if (!_listService.IsShared(task.ListId, model.UserId))
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
                throw new InvalidOperationException("Cannot update task in non-existing list");
            }

            var notifySignalR = !task.PrivateToUserId.HasValue && list.IsShared;
            var result = new UpdateTaskResult(originalTask.Name, list.Id, list.Name, notifySignalR);

            if (model.ListId == originalTask.ListId)
            {
                var usersToBeNotified = _listService.GetUsersToBeNotifiedOfChange(model.ListId, model.UserId, model.Id, metric);
                result.NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language }).ToList();
            }
            else
            {
                ToDoList oldList = _listsRepository.Get(originalTask.ListId);

                result.OldListId = originalTask.ListId;
                result.OldListName = oldList.Name;

                var usersToBeNotifiedOfRemoval = _listService.GetUsersToBeNotifiedOfChange(oldList.Id, model.UserId, model.Id, metric);
                result.RemovedNotificationRecipients = usersToBeNotifiedOfRemoval.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language }).ToList();

                var usersToBeNotifiedOfCreation = _listService.GetUsersToBeNotifiedOfChange(model.ListId, model.UserId, model.Id, metric);
                result.CreatedNotificationRecipients = usersToBeNotifiedOfCreation.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language }).ToList();
            }

            if (model.AssignedToUserId.HasValue
                && model.AssignedToUserId.Value != originalTask.AssignedToUserId
                && model.AssignedToUserId.Value != model.UserId
                && _listService.CheckIfUserCanBeNotifiedOfChange(model.ListId, model.AssignedToUserId.Value, metric))
            {
                var assignedUser = _userService.Get(model.AssignedToUserId.Value);
                result.AssignedNotificationRecipient = new NotificationRecipient { Id = assignedUser.Id, Language = assignedUser.Language };
            }

            if (result.Notify())
            {
                var user = _userService.Get(model.UserId);
                result.ActionUserName = user.Name;
                result.ActionUserImageUri = user.ImageUri;
            }

            return result;
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateAsync)}");
            throw;
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
                return new DeleteTaskResult(false);
            }

            if (!Exists(id, userId))
            {
                metric.Status = SpanStatus.PermissionDenied;
                throw new ValidationException("Unauthorized");
            }

            await _tasksRepository.DeleteAsync(id, userId, metric, cancellationToken);

            ToDoList? list = _listsRepository.GetWithShares(task.ListId, userId, metric);
            if (list is null)
            {
                throw new InvalidOperationException("Cannot delete task from non-existing list");
            }

            var usersToBeNotified = _listService.GetUsersToBeNotifiedOfChange(task.ListId, userId, task.PrivateToUserId == userId, metric).ToList();

            var notifySignalR = !task.PrivateToUserId.HasValue && list.IsShared;
            var result = new DeleteTaskResult(task.ListId, notifySignalR);

            if (!usersToBeNotified.Any())
            {
                return result;
            }

            var user = _userService.Get(userId);

            result.TaskName = task.Name;
            result.ListName = list.Name;
            result.ActionUserName = user.Name;
            result.ActionUserImageUri = user.ImageUri;
            result.NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language }).ToList();

            return result;
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteAsync)}");
            throw;
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
            if (!Exists(model.Id, model.UserId))
            {
                metric.Status = SpanStatus.PermissionDenied;
                throw new ValidationException("Unauthorized");
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
                throw new InvalidOperationException("Cannot complete task in non-existing list");
            }

            var usersToBeNotified = _listService.GetUsersToBeNotifiedOfChange(task.ListId, model.UserId, model.Id, metric).ToList();

            var notifySignalR = !task.PrivateToUserId.HasValue && list.IsShared;
            var result = new CompleteUncompleteTaskResult(task.ListId, notifySignalR: notifySignalR);

            if (!usersToBeNotified.Any())
            {
                return result;
            }

            var user = _userService.Get(model.UserId);

            result.TaskName = task.Name;
            result.ListName = list.Name;
            result.ActionUserName = user.Name;
            result.ActionUserImageUri = user.ImageUri;
            result.NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language }).ToList();

            return result;
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CompleteAsync)}");
            throw;
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
            if (!Exists(model.Id, model.UserId))
            {
                metric.Status = SpanStatus.PermissionDenied;
                throw new ValidationException("Unauthorized");
            }

            ToDoTask task = _tasksRepository.Get(model.Id);
            if (!task.IsCompleted)
            {
                return new CompleteUncompleteTaskResult(false);
            }

            await _tasksRepository.UncompleteAsync(model.Id, model.UserId, metric, cancellationToken);

            ToDoList? list = _listsRepository.GetWithShares(task.ListId, model.UserId, metric);
            if (list is null)
            {
                throw new InvalidOperationException("Cannot uncomplete task in non-existing list");
            }

            var usersToBeNotified = _listService.GetUsersToBeNotifiedOfChange(task.ListId, model.UserId, model.Id, metric).ToList();

            var notifySignalR = !task.PrivateToUserId.HasValue && list.IsShared;
            var result = new CompleteUncompleteTaskResult(task.ListId, notifySignalR);

            if (!usersToBeNotified.Any())
            {
                return result;
            }

            var user = _userService.Get(model.UserId);

            result.TaskName = task.Name;
            result.ListName = list.Name;
            result.ActionUserName = user.Name;
            result.ActionUserImageUri = user.ImageUri;
            result.NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language }).ToList();

            return result;
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UncompleteAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<ReorderTaskResult> ReorderAsync(ReorderTask model, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(TaskService)}.{nameof(ReorderAsync)}");

        try
        {
            if (!Exists(model.Id, model.UserId))
            {
                metric.Status = SpanStatus.PermissionDenied;
                throw new ValidationException("Unauthorized");
            }

            await _tasksRepository.ReorderAsync(model.Id, model.UserId, model.OldOrder, model.NewOrder, DateTime.UtcNow, cancellationToken);

            ToDoTask task = _tasksRepository.Get(model.Id);
            ToDoList? list = _listsRepository.GetWithShares(task.ListId, model.UserId, metric);
            if (list is null)
            {
                throw new InvalidOperationException("Cannot reorder task in non-existing list");
            }

            var notifySignalR = !task.PrivateToUserId.HasValue && list.IsShared;
            return new ReorderTaskResult(list.Id, notifySignalR);
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(ReorderAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }
}
