using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using PersonalAssistant.Application.Contracts.ToDoAssistant;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Services.ToDoAssistant
{
    public class TaskService : ITaskService
    {
        private readonly ITasksRepository _tasksRepository;
        private readonly IListService _listService;
        private readonly IMapper _mapper;

        public TaskService(
            ITasksRepository tasksRepository,
            IListService listService,
            IMapper mapper)
        {
            _tasksRepository = tasksRepository;
            _listService = listService;
            _mapper = mapper;
        }

        public async Task<SimpleTask> GetAsync(int id)
        {
            ToDoTask task = await _tasksRepository.GetAsync(id);

            var result = _mapper.Map<SimpleTask>(task);

            return result;
        }

        public async Task<TaskDto> GetAsync(int id, int userId)
        {
            ToDoTask task = await _tasksRepository.GetAsync(id, userId);

            var result = _mapper.Map<TaskDto>(task);

            return result;
        }

        public async Task<TaskForUpdate> GetForUpdateAsync(int id, int userId)
        {
            ToDoTask task = await _tasksRepository.GetForUpdateAsync(id, userId);

            var result = _mapper.Map<TaskForUpdate>(task, opts => { opts.Items["UserId"] = userId; });

            result.IsInSharedList = await _listService.IsSharedAsync(task.ListId, userId);
            result.Recipes = await _tasksRepository.GetRecipesAsync(id, userId);

            return result;
        }

        public Task<bool> ExistsAsync(int id, int userId)
        {
            return _tasksRepository.ExistsAsync(id, userId);
        }

        public Task<bool> ExistsAsync(string name, int listId, int userId)
        {
            return _tasksRepository.ExistsAsync(name.Trim(), listId, userId);
        }

        public Task<bool> ExistsAsync(IEnumerable<string> names, int listId, int userId)
        {
            var upperCaseNames = names.Select(name => name.ToUpperInvariant()).ToList();
            return _tasksRepository.ExistsAsync(upperCaseNames, listId, userId);
        }

        public Task<bool> ExistsAsync(int id, string name, int listId, int userId)
        {
            return _tasksRepository.ExistsAsync(id, name.Trim(), listId, userId);
        }

        public Task<int> CountAsync(int listId)
        {
            return _tasksRepository.CountAsync(listId);
        }

        public async Task<CreatedTask> CreateAsync(CreateTask model, IValidator<CreateTask> validator)
        {
            ValidateAndThrow(model, validator);

            var task = _mapper.Map<ToDoTask>(model);

            task.Name = task.Name.Trim();

            if (!await _listService.IsSharedAsync(task.ListId, model.UserId))
            {
                task.PrivateToUserId = null;
                task.AssignedToUserId = null;
            }

            task.Order = 1;
            task.CreatedDate = task.ModifiedDate = DateTime.UtcNow;

            task.Id = await _tasksRepository.CreateAsync(task, model.UserId);

            var result = _mapper.Map<CreatedTask>(task);

            return result;
        }

        public async Task<IEnumerable<CreatedTask>> BulkCreateAsync(BulkCreate model, IValidator<BulkCreate> validator)
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

            var result = createdTasks.Select(x => _mapper.Map<CreatedTask>(x));

            return result;
        }

        public async Task UpdateAsync(UpdateTask model, IValidator<UpdateTask> validator)
        {
            ValidateAndThrow(model, validator);

            var task = _mapper.Map<ToDoTask>(model);

            task.Name = task.Name.Trim();

            if (model.IsPrivate == true)
            {
                task.AssignedToUserId = null;
            }

            if (!await _listService.IsSharedAsync(task.ListId, model.UserId))
            {
                task.PrivateToUserId = null;
                task.AssignedToUserId = null;
            }

            task.ModifiedDate = DateTime.UtcNow;
            await _tasksRepository.UpdateAsync(task, model.UserId);
        }

        public async Task<SimpleTask> DeleteAsync(int id, int userId)
        {
            if (!await ExistsAsync(id, userId))
            {
                throw new ValidationException("Unauthorized");
            }

            ToDoTask originalTask = await _tasksRepository.DeleteAsync(id, userId);

            var result = _mapper.Map<SimpleTask>(originalTask);

            return result;
        }

        public async Task<SimpleTask> CompleteAsync(CompleteUncomplete model)
        {
            if (!await ExistsAsync(model.Id, model.UserId))
            {
                throw new ValidationException("Unauthorized");
            }

            ToDoTask originalTask = await _tasksRepository.CompleteAsync(model.Id, model.UserId);

            var result = _mapper.Map<SimpleTask>(originalTask);

            return result;
        }

        public async Task<SimpleTask> UncompleteAsync(CompleteUncomplete model)
        {
            if (!await ExistsAsync(model.Id, model.UserId))
            {
                throw new ValidationException("Unauthorized");
            }

            ToDoTask originalTask = await _tasksRepository.UncompleteAsync(model.Id, model.UserId);

            var result = _mapper.Map<SimpleTask>(originalTask);

            return result;
        }

        public async Task ReorderAsync(ReorderTask model)
        {
            if (!await ExistsAsync(model.Id, model.UserId))
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
