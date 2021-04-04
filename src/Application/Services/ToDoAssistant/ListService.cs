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

        public async Task<IEnumerable<ListDto>> GetAllAsync(int userId)
        {
            IEnumerable<ToDoList> lists = await _listsRepository.GetAllWithTasksAndSharingDetailsAsync(userId);

            var result = lists.Select(x => _mapper.Map<ListDto>(x, opts => { opts.Items["UserId"] = userId; }));

            return result;
        }

        public async Task<IEnumerable<ToDoListOption>> GetAllAsOptionsAsync(int userId)
        {
            IEnumerable<ToDoList> lists = await _listsRepository.GetAllAsOptionsAsync(userId);

            var result = lists.Select(x => _mapper.Map<ToDoListOption>(x));

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

        public async Task<EditListDto> GetAsync(int id, int userId)
        {
            ToDoList list = await _listsRepository.GetAsync(id, userId);

            var result = _mapper.Map<EditListDto>(list, opts => { opts.Items["UserId"] = userId; });

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

        public async Task<IEnumerable<ShareListRequest>> GetShareRequestsAsync(int userId)
        {
            IEnumerable<ListShare> shareRequests = await _listsRepository.GetShareRequestsAsync(userId);

            var result = shareRequests.Select(x => _mapper.Map<ShareListRequest>(x, opts => { opts.Items["UserId"] = userId; }));

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

        public async Task<bool> IsSharedAsync(int id, int userId)
        {
            if (!await UserOwnsOrSharesAsync(id, userId))
            {
                throw new ValidationException("Unauthorized");
            }

            return await _listsRepository.IsSharedAsync(id);
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

        public async Task<int> CreateAsync(CreateList model, IValidator<CreateList> validator)
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
            if (!await _listsRepository.UserOwnsAsync(id, userId))
            {
                throw new ValidationException("Unauthorized");
            }

            return await _listsRepository.DeleteAsync(id);
        }

        public async Task ShareAsync(ShareList model, IValidator<ShareList> validator)
        {
            ValidateAndThrow(model, validator);

            var now = DateTime.Now;

            var newShares = new List<ListShare>();
            foreach (ShareUserAndPermission newShare in model.NewShares)
            {
                if (await _listsRepository.UserHasBlockedSharingAsync(model.UserId, newShare.UserId))
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

        public async Task<bool> LeaveAsync(int id, int userId)
        {
            ListShare share = await _listsRepository.LeaveAsync(id, userId);

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
