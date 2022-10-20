﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.Common;
using Application.Contracts.Common.Models;
using Application.Contracts.ToDoAssistant.Lists;
using Application.Contracts.ToDoAssistant.Tasks;
using Application.Contracts.ToDoAssistant.Tasks.Models;
using Application.Utils;
using AutoMapper;
using Domain.Entities.ToDoAssistant;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Services.ToDoAssistant;

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

    public TaskDto Get(int id, int userId)
    {
        try
        {
            ToDoTask task = _tasksRepository.Get(id, userId);

            var result = _mapper.Map<TaskDto>(task);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Get)}");
            throw;
        }
    }

    public TaskForUpdate GetForUpdate(int id, int userId)
    {
        try
        {
            ToDoTask task = _tasksRepository.GetForUpdate(id, userId);
            if (task == null)
            {
                return null;
            }

            var result = _mapper.Map<TaskForUpdate>(task, opts => { opts.Items["UserId"] = userId; });

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

    public async Task<CreatedTaskResult> CreateAsync(CreateTask model, IValidator<CreateTask> validator)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        try
        {
            var task = _mapper.Map<ToDoTask>(model);

            task.Name = task.Name.Trim();

            if (!_listService.IsShared(task.ListId, model.UserId))
            {
                task.PrivateToUserId = null;
                task.AssignedToUserId = null;
            }

            task.Order = 1;
            task.CreatedDate = task.ModifiedDate = DateTime.UtcNow;

            var id = await _tasksRepository.CreateAsync(task, model.UserId);

            ToDoList list = _listsRepository.GetWithShares(model.ListId, model.UserId);

            var notifySignalR = !task.PrivateToUserId.HasValue && list.IsShared;
            var result = new CreatedTaskResult(id, task.ListId, notifySignalR);

            var usersToBeNotified = _listService.GetUsersToBeNotifiedOfChange(model.ListId, model.UserId, model.IsPrivate == true).ToList();
            if (!usersToBeNotified.Any())
            {
                return result;
            }

            var user = _userService.Get(model.UserId);

            result.TaskName = task.Name;
            result.ListName = list.Name;
            result.ActionUserName = user.Name;
            result.ActionUserImageUri = user.ImageUri;
            result.NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateAsync)}");
            throw;
        }
    }

    public async Task<BulkCreateResult> BulkCreateAsync(BulkCreate model, IValidator<BulkCreate> validator)
    {
        ValidationUtil.ValidOrThrow(model, validator);

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

            IEnumerable<ToDoTask> createdTasks = await _tasksRepository.BulkCreateAsync(tasks, model.TasksArePrivate, model.UserId);

            ToDoList list = _listsRepository.GetWithShares(model.ListId, model.UserId);

            var usersToBeNotified = _listService.GetUsersToBeNotifiedOfChange(model.ListId, model.UserId, model.TasksArePrivate).ToList();

            var notifySignalR = !tasks[0].PrivateToUserId.HasValue && list.IsShared;
            var result = new BulkCreateResult(list.Id, notifySignalR);

            if (!usersToBeNotified.Any())
            {
                return result;
            }

            var user = _userService.Get(model.UserId);

            result.ListName = list.Name;
            result.CreatedTasks = createdTasks.Select(x => new BulkCreatedTask
            {
                Id = x.Id,
                Name = x.Name
            });
            result.ActionUserName = user.Name;
            result.ActionUserImageUri = user.ImageUri;
            result.NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(BulkCreateAsync)}");
            throw;
        }
    }

    public async Task<UpdateTaskResult> UpdateAsync(UpdateTask model, IValidator<UpdateTask> validator)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        try
        {
            var task = _mapper.Map<ToDoTask>(model);

            task.Name = task.Name.Trim();

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
            await _tasksRepository.UpdateAsync(task, model.UserId);

            ToDoList list = _listsRepository.GetWithShares(model.ListId, model.UserId);

            var notifySignalR = !task.PrivateToUserId.HasValue && list.IsShared;
            var result = new UpdateTaskResult(originalTask.Name, list.Id, list.Name, notifySignalR);

            if (model.ListId == originalTask.ListId)
            {
                var usersToBeNotified = _listService.GetUsersToBeNotifiedOfChange(model.ListId, model.UserId, model.Id);
                result.NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language });
            }
            else
            {
                ToDoList oldList = _listsRepository.Get(originalTask.ListId);

                result.OldListId = originalTask.ListId;
                result.OldListName = oldList.Name;

                var usersToBeNotifiedOfRemoval = _listService.GetUsersToBeNotifiedOfChange(oldList.Id, model.UserId, model.Id);
                result.RemovedNotificationRecipients = usersToBeNotifiedOfRemoval.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language });

                var usersToBeNotifiedOfCreation = _listService.GetUsersToBeNotifiedOfChange(model.ListId, model.UserId, model.Id);
                result.CreatedNotificationRecipients = usersToBeNotifiedOfCreation.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language });
            }

            if (model.AssignedToUserId.HasValue
                && model.AssignedToUserId.Value != originalTask.AssignedToUserId
                && model.AssignedToUserId.Value != model.UserId
                && _listService.CheckIfUserCanBeNotifiedOfChange(model.ListId, model.AssignedToUserId.Value))
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
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateAsync)}");
            throw;
        }
    }

    public async Task<DeleteTaskResult> DeleteAsync(int id, int userId)
    {
        try
        {
            ToDoTask task = _tasksRepository.Get(id);
            if (task == null)
            {
                return new DeleteTaskResult(false);
            }

            if (!Exists(id, userId))
            {
                throw new ValidationException("Unauthorized");
            }

            await _tasksRepository.DeleteAsync(id, userId);

            ToDoList list = _listsRepository.GetWithShares(task.ListId, userId);

            var usersToBeNotified = _listService.GetUsersToBeNotifiedOfChange(task.ListId, userId, task.PrivateToUserId == userId).ToList();

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
            result.NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteAsync)}");
            throw;
        }
    }

    public async Task<CompleteUncompleteTaskResult> CompleteAsync(CompleteUncomplete model)
    {
        try
        {
            if (!Exists(model.Id, model.UserId))
            {
                throw new ValidationException("Unauthorized");
            }

            ToDoTask task = _tasksRepository.Get(model.Id);
            if (task.IsCompleted)
            {
                return new CompleteUncompleteTaskResult(false);
            }

            await _tasksRepository.CompleteAsync(model.Id, model.UserId);

            ToDoList list = _listsRepository.GetWithShares(task.ListId, model.UserId);

            var usersToBeNotified = _listService.GetUsersToBeNotifiedOfChange(task.ListId, model.UserId, model.Id).ToList();

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
            result.NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CompleteAsync)}");
            throw;
        }
    }

    public async Task<CompleteUncompleteTaskResult> UncompleteAsync(CompleteUncomplete model)
    {
        try
        {
            if (!Exists(model.Id, model.UserId))
            {
                throw new ValidationException("Unauthorized");
            }

            ToDoTask task = _tasksRepository.Get(model.Id);
            if (!task.IsCompleted)
            {
                return new CompleteUncompleteTaskResult(false);
            }

            await _tasksRepository.UncompleteAsync(model.Id, model.UserId);

            ToDoList list = _listsRepository.GetWithShares(task.ListId, model.UserId);

            var usersToBeNotified = _listService.GetUsersToBeNotifiedOfChange(task.ListId, model.UserId, model.Id).ToList();

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
            result.NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language });

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UncompleteAsync)}");
            throw;
        }
    }

    public async Task<ReorderTaskResult> ReorderAsync(ReorderTask model)
    {
        try
        {
            if (!Exists(model.Id, model.UserId))
            {
                throw new ValidationException("Unauthorized");
            }

            await _tasksRepository.ReorderAsync(model.Id, model.UserId, model.OldOrder, model.NewOrder, DateTime.UtcNow);

            ToDoTask task = _tasksRepository.Get(model.Id);
            ToDoList list = _listsRepository.GetWithShares(task.ListId, model.UserId);

            var notifySignalR = !task.PrivateToUserId.HasValue && list.IsShared;
            return new ReorderTaskResult(list.Id, notifySignalR);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(ReorderAsync)}");
            throw;
        }
    }
}
