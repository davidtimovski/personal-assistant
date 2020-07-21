using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.ToDoAssistant;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications;
using PersonalAssistant.Domain.Entities;
using PersonalAssistant.Domain.Entities.Common;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Services.ToDoAssistant
{
    public class ListService : IListService
    {
        private readonly IUserService _userService;
        private readonly IListsRepository _listsRepository;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IMapper _mapper;

        public ListService(
            IUserService userService,
            IListsRepository listsRepository,
            INotificationsRepository notificationsRepository,
            IMapper mapper)
        {
            _userService = userService;
            _listsRepository = listsRepository;
            _notificationsRepository = notificationsRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ToDoListOption>> GetAllAsOptionsAsync(int userId)
        {
            IEnumerable<ToDoList> lists = await _listsRepository.GetAllAsOptionsAsync(userId);

            var result = lists.Select(x => _mapper.Map<ToDoListOption>(x));

            return result;
        }

        public async Task<IEnumerable<ListWithSharingDetails>> GetAllWithTasksAndSharingDetailsAsync(int userId)
        {
            IEnumerable<ToDoList> lists = await _listsRepository.GetAllWithTasksAndSharingDetailsAsync(userId);

            var result = lists.Select(x => _mapper.Map<ListWithSharingDetails>(x, opts => { opts.Items["UserId"] = userId; }));

            return result.OrderBy(x => x.Order);
        }

        public async Task<IEnumerable<ArchivedList>> GetAllArchivedAsync(int userId)
        {
            IEnumerable<ToDoList> lists = await _listsRepository.GetAllArchivedAsync(userId);
            lists = lists.OrderByDescending(x => x.ModifiedDate);

            var result = lists.Select(x => _mapper.Map<ArchivedList>(x, opts => { opts.Items["UserId"] = userId; }));

            return result;
        }

        public async Task<IEnumerable<AssigneeOption>> GetMembersAsAssigneeOptionsAsync(int id)
        {
            IEnumerable<User> members = await _listsRepository.GetMembersAsAssigneeOptionsAsync(id);

            var result = members.Select(x => _mapper.Map<AssigneeOption>(x));

            return result;
        }

        public async Task<SimpleList> GetAsync(int id)
        {
            ToDoList list = await _listsRepository.GetAsync(id);

            var result = _mapper.Map<SimpleList>(list);

            return result;
        }

        public async Task<ListDto> GetAsync(int id, int userId)
        {
            ToDoList list = await _listsRepository.GetAsync(id, userId);

            var result = _mapper.Map<ListDto>(list, opts => { opts.Items["UserId"] = userId; });

            return result;
        }

        public async Task<ListWithTasks> GetWithTasksAsync(int id, int userId)
        {
            ToDoList list = await _listsRepository.GetWithTasksAsync(id, userId);

            var result = _mapper.Map<ListWithTasks>(list, opts => { opts.Items["UserId"] = userId; });

            return result;
        }

        public async Task<ListWithShares> GetWithSharesAsync(int id, int userId)
        {
            ToDoList list = await _listsRepository.GetWithOwnerAsync(id, userId);
            if (list == null)
            {
                return null;
            }

            list.Shares.AddRange(await _listsRepository.GetSharesAsync(id));
            list.Shares.RemoveAll(x => x.UserId == userId);

            var result = _mapper.Map<ListWithShares>(list, opts => { opts.Items["UserId"] = userId; });

            return result;
        }

        public async Task<IEnumerable<ShareRequest>> GetShareRequestsAsync(int userId)
        {
            IEnumerable<Share> shareRequests = await _listsRepository.GetShareRequestsAsync(userId);

            var result = shareRequests.Select(x => _mapper.Map<ShareRequest>(x, opts => { opts.Items["UserId"] = userId; }));

            return result;
        }

        public Task<int> GetPendingShareRequestsCountAsync(int userId)
        {
            return _listsRepository.GetPendingShareRequestsCountAsync(userId);
        }

        public async Task<bool> CanShareWithUserAsync(int shareWithId, int userId)
        {
            if (shareWithId == userId)
            {
                return false;
            }

            return await _listsRepository.CanShareWithUserAsync(shareWithId, userId);
        }

        public Task<bool> UserOwnsOrSharesAsync(int id, int userId)
        {
            return _listsRepository.UserOwnsOrSharesAsync(id, userId);
        }

        public Task<bool> UserOwnsOrSharesAsPendingAsync(int id, int userId)
        {
            return _listsRepository.UserOwnsOrSharesAsPendingAsync(id, userId);
        }

        public Task<bool> UserOwnsOrSharesAsAdminAsync(int id, int userId)
        {
            return _listsRepository.UserOwnsOrSharesAsAdminAsync(id, userId);
        }

        public Task<bool> UserOwnsOrSharesAsAdminAsync(int id, string name, int userId)
        {
            return _listsRepository.UserOwnsOrSharesAsAdminAsync(id, name.Trim(), userId);
        }

        public Task<bool> UserOwnsAsync(int id, int userId)
        {
            return _listsRepository.UserOwnsAsync(id, userId);
        }

        public async Task<bool> IsSharedAsync(int id, int userId)
        {
            if (!await UserOwnsOrSharesAsync(id, userId))
            {
                throw new ValidationException("Unauthorized");
            }

            return await _listsRepository.IsSharedAsync(id);
        }

        public Task<bool> UserHasBlockedSharingAsync(int userId, int sharedWithId)
        {
            return _listsRepository.UserHasBlockedSharingAsync(userId, sharedWithId);
        }

        public Task<bool> ExistsAsync(string name, int userId)
        {
            return _listsRepository.ExistsAsync(name.Trim(), userId);
        }

        public Task<bool> ExistsAsync(int id, string name, int userId)
        {
            return _listsRepository.ExistsAsync(id, name.Trim(), userId);
        }

        public Task<int> CountAsync(int userId)
        {
            return _listsRepository.CountAsync(userId);
        }

        public async Task<CreatedList> CreateAsync(CreateList model, IValidator<CreateList> validator)
        {
            ValidateAndThrow(model, validator);

            var list = _mapper.Map<ToDoList>(model);

            list.Name = list.Name.Trim();
            list.CreatedDate = list.ModifiedDate = DateTime.Now;

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

            await _listsRepository.CreateAsync(list);

            var createdList = _mapper.Map<CreatedList>(list, opts => { opts.Items["UserId"] = model.UserId; });

            return createdList;
        }

        public async Task CreateSampleAsync(int userId, Dictionary<string, string> translations)
        {
            var list = new ToDoList
            {
                UserId = userId,
                Name = translations["SampleListName"],
                Icon = "list"
            };
            list.CreatedDate = list.ModifiedDate = DateTime.Now;

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

        public async Task<UpdateListOriginal> UpdateAsync(UpdateList model, IValidator<UpdateList> validator)
        {
            ValidateAndThrow(model, validator);

            var list = _mapper.Map<ToDoList>(model);

            list.Name = list.Name.Trim();
            list.ModifiedDate = DateTime.Now;
            ToDoList original = await _listsRepository.UpdateAsync(list);

            var result = _mapper.Map<UpdateListOriginal>(original);

            return result;
        }

        public async Task UpdateSharedAsync(UpdateSharedList model, IValidator<UpdateSharedList> validator)
        {
            ValidateAndThrow(model, validator);

            var list = _mapper.Map<ToDoList>(model);

            list.ModifiedDate = DateTime.Now;
            await _listsRepository.UpdateSharedAsync(list);
        }

        public async Task<string> DeleteAsync(int id, int userId)
        {
            if (!await UserOwnsAsync(id, userId))
            {
                throw new ValidationException("Unauthorized");
            }

            return await _listsRepository.DeleteAsync(id);
        }

        public async Task ShareAsync(ShareList model, IValidator<ShareList> validator)
        {
            ValidateAndThrow(model, validator);

            var now = DateTime.Now;

            var newShares = new List<Share>();
            foreach (ShareUserAndPermission newShare in model.NewShares)
            {
                if (await UserHasBlockedSharingAsync(model.UserId, newShare.UserId))
                {
                    continue;
                }

                newShares.Add(new Share
                {
                    UserId = newShare.UserId,
                    IsAdmin = newShare.IsAdmin,
                    CreatedDate = now,
                    ModifiedDate = now
                });
            }

            var editedShares = new List<Share>();
            foreach (ShareUserAndPermission editedShare in model.EditedShares)
            {
                editedShares.Add(new Share
                {
                    UserId = editedShare.UserId,
                    IsAdmin = editedShare.IsAdmin,
                    ModifiedDate = now
                });
            }

            var removedShares = new List<Share>();
            foreach (ShareUserAndPermission removedShare in model.RemovedShares)
            {
                removedShares.Add(new Share
                {
                    UserId = removedShare.UserId,
                    IsAdmin = removedShare.IsAdmin
                });
            }

            await _listsRepository.SaveSharingDetailsAsync(newShares, editedShares, removedShares);

            foreach (Share share in removedShares)
            {
                await _notificationsRepository.DeleteForUserAndListAsync(share.UserId, share.ListId);
            }
        }

        public async Task<bool> LeaveAsync(int id, int userId)
        {
            Share share = await _listsRepository.LeaveAsync(id, userId);

            await _notificationsRepository.DeleteForUserAndListAsync(userId, id);

            return share.IsAccepted.Value != false;
        }

        public async Task<int> CopyAsync(CopyList model, IValidator<CopyList> validator)
        {
            ValidateAndThrow(model, validator);

            var list = _mapper.Map<ToDoList>(model);

            list.Name = list.Name.Trim();
            list.CreatedDate = list.ModifiedDate = DateTime.Now;

            return await _listsRepository.CopyAsync(list);
        }

        public async Task SetIsArchivedAsync(int id, int userId, bool isArchived)
        {
            if (!await UserOwnsOrSharesAsync(id, userId))
            {
                throw new ValidationException("Unauthorized");
            }

            await _listsRepository.SetIsArchivedAsync(id, userId, isArchived, DateTime.Now);
        }

        public async Task<bool> SetTasksAsNotCompletedAsync(int id, int userId)
        {
            if (!await UserOwnsOrSharesAsync(id, userId))
            {
                throw new ValidationException("Unauthorized");
            }

            return await _listsRepository.SetTasksAsNotCompletedAsync(id, userId, DateTime.Now);
        }

        public async Task SetShareIsAcceptedAsync(int id, int userId, bool isAccepted)
        {
            await _listsRepository.SetShareIsAcceptedAsync(id, userId, isAccepted, DateTime.Now);
        }

        public async Task ReorderAsync(int id, int userId, short oldOrder, short newOrder)
        {
            if (!await UserOwnsOrSharesAsync(id, userId))
            {
                throw new ValidationException("Unauthorized");
            }

            await _listsRepository.ReorderAsync(id, userId, oldOrder, newOrder, DateTime.Now);
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
