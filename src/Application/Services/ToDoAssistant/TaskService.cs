using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.Common.Models;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Services.ToDoAssistant
{
    public class TaskService : ITaskService
    {
        private readonly IUserService _userService;
        private readonly IListService _listService;
        private readonly ITasksRepository _tasksRepository;
        private readonly IListsRepository _listsRepository;
        private readonly IMapper _mapper;

        public TaskService(
            IUserService userService,
            IListService listService,
            ITasksRepository tasksRepository,
            IListsRepository listsRepository,
            IMapper mapper)
        {
            _userService = userService;
            _listService = listService;
            _tasksRepository = tasksRepository;
            _listsRepository = listsRepository;
            _mapper = mapper;
        }

        public SimpleTask Get(int id)
        {
            ToDoTask task = _tasksRepository.Get(id);

            var result = _mapper.Map<SimpleTask>(task);

            return result;
        }

        public TaskDto Get(int id, int userId)
        {
            ToDoTask task = _tasksRepository.Get(id, userId);

            var result = _mapper.Map<TaskDto>(task);

            return result;
        }

        public TaskForUpdate GetForUpdate(int id, int userId)
        {
            ToDoTask task = _tasksRepository.GetForUpdate(id, userId);

            var result = _mapper.Map<TaskForUpdate>(task, opts => { opts.Items["UserId"] = userId; });

            result.IsInSharedList = _listService.IsShared(task.ListId, userId);
            result.Recipes = _tasksRepository.GetRecipes(id, userId);

            return result;
        }

        public bool Exists(int id, int userId)
        {
            return _tasksRepository.Exists(id, userId);
        }

        public bool Exists(string name, int listId, int userId)
        {
            return _tasksRepository.Exists(name.Trim(), listId, userId);
        }

        public bool Exists(IEnumerable<string> names, int listId, int userId)
        {
            var upperCaseNames = names.Select(name => name.Trim().ToUpperInvariant()).ToList();
            return _tasksRepository.Exists(upperCaseNames, listId, userId);
        }

        public bool Exists(int id, string name, int listId, int userId)
        {
            return _tasksRepository.Exists(id, name.Trim(), listId, userId);
        }

        public int Count(int listId)
        {
            return _tasksRepository.Count(listId);
        }

        public async Task<CreatedTaskResult> CreateAsync(CreateTask model, IValidator<CreateTask> validator)
        {
            ValidateAndThrow(model, validator);

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

            var usersToBeNotified = _listService.GetUsersToBeNotifiedOfChange(model.ListId, model.UserId, model.IsPrivate == true);
            if (!usersToBeNotified.Any())
            {
                return new CreatedTaskResult { TaskId = id };
            }

            ToDoList list = _listsRepository.Get(model.ListId);

            var result = new CreatedTaskResult
            {
                TaskId = id,
                TaskName = task.Name,
                ListName = list.Name,
                ActionUserImageUri = _userService.GetImageUri(model.UserId),
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
            };

            return result;
        }

        public async Task<BulkCreateResult> BulkCreateAsync(BulkCreate model, IValidator<BulkCreate> validator)
        {
            ValidateAndThrow(model, validator);

            var task = _mapper.Map<ToDoTask>(model);

            var now = DateTime.UtcNow;

            var tasks = model.TasksText.Split("\n")
                .Where(task => !string.IsNullOrWhiteSpace(task))
                .Select(task => new ToDoTask
                {
                    ListId = model.ListId,
                    Name = task.Trim(),
                    IsOneTime = model.TasksAreOneTime,
                    PrivateToUserId = model.TasksArePrivate ? model.UserId : (int?)null,
                    AssignedToUserId = null,
                    CreatedDate = now,
                    ModifiedDate = now
                }
            ).ToList();

            IEnumerable<ToDoTask> createdTasks = await _tasksRepository.BulkCreateAsync(tasks, model.TasksArePrivate, model.UserId);

            var usersToBeNotified = _listService.GetUsersToBeNotifiedOfChange(model.ListId, model.UserId, model.TasksArePrivate);
            if (!usersToBeNotified.Any())
            {
                return new BulkCreateResult();
            }

            ToDoList list = _listsRepository.Get(model.ListId);

            var result = new BulkCreateResult
            {
                ListName = list.Name,
                CreatedTasks = createdTasks.Select(x => new BulkCreatedTask
                {
                    Id = x.Id,
                    Name = x.Name
                }),
                ActionUserImageUri = _userService.GetImageUri(model.UserId),
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
            };

            return result;
        }

        public async Task<UpdateTaskResult> UpdateAsync(UpdateTask model, IValidator<UpdateTask> validator)
        {
            ValidateAndThrow(model, validator);

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

            ToDoList list = _listsRepository.Get(model.ListId);

            var result = new UpdateTaskResult
            {
                OriginalTaskName = originalTask.Name,
                ListId = model.ListId,
                ListName = list.Name
            };

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
                result.ActionUserImageUri = _userService.GetImageUri(model.UserId);
            }

            return result;
        }

        public async Task<DeleteTaskResult> DeleteAsync(int id, int userId)
        {
            ToDoTask task = _tasksRepository.Get(id);
            if (task == null)
            {
                return new DeleteTaskResult();
            }

            if (!Exists(id, userId))
            {
                throw new ValidationException("Unauthorized");
            }

            await _tasksRepository.DeleteAsync(id, userId);

            var usersToBeNotified = _listService.GetUsersToBeNotifiedOfChange(task.ListId, userId, task.PrivateToUserId == userId);
            if (!usersToBeNotified.Any())
            {
                return new DeleteTaskResult();
            }

            ToDoList list = _listsRepository.Get(task.ListId);

            var result = new DeleteTaskResult
            {
                TaskName = task.Name,
                ListId = task.ListId,
                ListName = list.Name,
                ActionUserImageUri = _userService.GetImageUri(userId),
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
            };

            return result;
        }

        public async Task<CompleteUncompleteTaskResult> CompleteAsync(CompleteUncomplete model)
        {
            if (!Exists(model.Id, model.UserId))
            {
                throw new ValidationException("Unauthorized");
            }

            ToDoTask task = _tasksRepository.Get(model.Id);
            if (task.IsCompleted)
            {
                return new CompleteUncompleteTaskResult();
            }

            await _tasksRepository.CompleteAsync(model.Id, model.UserId);

            var usersToBeNotified = _listService.GetUsersToBeNotifiedOfChange(task.ListId, model.UserId, model.Id);
            if (!usersToBeNotified.Any())
            {
                return new CompleteUncompleteTaskResult();
            }

            ToDoList list = _listsRepository.Get(task.ListId);

            var result = new CompleteUncompleteTaskResult
            {
                TaskName = task.Name,
                ListId = task.ListId,
                ListName = list.Name,
                ActionUserImageUri = _userService.GetImageUri(model.UserId),
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
            };

            return result;
        }

        public async Task<CompleteUncompleteTaskResult> UncompleteAsync(CompleteUncomplete model)
        {
            if (!Exists(model.Id, model.UserId))
            {
                throw new ValidationException("Unauthorized");
            }

            ToDoTask task = _tasksRepository.Get(model.Id);
            if (!task.IsCompleted)
            {
                return null;
            }

            await _tasksRepository.UncompleteAsync(model.Id, model.UserId);

            var usersToBeNotified = _listService.GetUsersToBeNotifiedOfChange(task.ListId, model.UserId, model.Id);
            if (!usersToBeNotified.Any())
            {
                return new CompleteUncompleteTaskResult();
            }

            ToDoList list = _listsRepository.Get(task.ListId);

            var result = new CompleteUncompleteTaskResult
            {
                TaskName = task.Name,
                ListId = task.ListId,
                ListName = list.Name,
                ActionUserImageUri = _userService.GetImageUri(model.UserId),
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
            };

            return result;
        }

        public async Task ReorderAsync(ReorderTask model)
        {
            if (!Exists(model.Id, model.UserId))
            {
                throw new ValidationException("Unauthorized");
            }

            await _tasksRepository.ReorderAsync(model.Id, model.UserId, model.OldOrder, model.NewOrder, DateTime.UtcNow);
        }

        private void ValidateAndThrow<T>(T model, IValidator<T> validator)
        {
            ValidationResult result = validator.Validate(model);
            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }
    }
}
