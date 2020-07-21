using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using PersonalAssistant.Application.Contracts;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.Common.Models;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using PersonalAssistant.Domain.Entities;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Application.Services.Common
{
    public class UserService : IUserService
    {
        private readonly ICdnService _cdnService;
        private readonly IUsersRepository _usersRepository;
        private readonly ITasksRepository _tasksRepository;
        private readonly IMapper _mapper;

        public UserService(
            ICdnService cdnService,
            IUsersRepository usersRepository,
            ITasksRepository tasksRepository,
            IMapper mapper)
        {
            _cdnService = cdnService;
            _usersRepository = usersRepository;
            _tasksRepository = tasksRepository;
            _mapper = mapper;
        }

        public Task<User> GetAsync(int id)
        {
            return _usersRepository.GetAsync(id);
        }

        public Task<User> GetAsync(string email)
        {
            return _usersRepository.GetAsync(email.Trim());
        }

        public Task<IEnumerable<User>> GetToBeNotifiedOfListChangeAsync(int listId, int excludeUserId)
        {
            return _usersRepository.GetToBeNotifiedOfListChangeAsync(listId, excludeUserId);
        }

        public async Task<IEnumerable<User>> GetToBeNotifiedOfListChangeAsync(int listId, int excludeUserId, bool isPrivate)
        {
            if (isPrivate)
            {
                return new List<User>();
            }

            return await _usersRepository.GetToBeNotifiedOfListChangeAsync(listId, excludeUserId);
        }

        public async Task<IEnumerable<User>> GetToBeNotifiedOfListChangeAsync(int listId, int excludeUserId, int taskId)
        {
            if (await _tasksRepository.IsPrivateAsync(taskId, excludeUserId))
            {
                return new List<User>();
            }

            return await _usersRepository.GetToBeNotifiedOfListChangeAsync(listId, excludeUserId);
        }

        public Task<bool> CheckIfUserCanBeNotifiedOfListChangeAsync(int listId, int userId)
        {
            return _usersRepository.CheckIfUserCanBeNotifiedOfListChangeAsync(listId, userId);
        }

        public Task<IEnumerable<User>> GetToBeNotifiedOfListDeletionAsync(int listId)
        {
            return _usersRepository.GetToBeNotifiedOfListDeletionAsync(listId);
        }

        public Task<IEnumerable<User>> GetToBeNotifiedOfRecipeSentAsync(int recipeId)
        {
            return _usersRepository.GetToBeNotifiedOfRecipeSentAsync(recipeId);
        }

        public Task<bool> ExistsAsync(int id)
        {
            return _usersRepository.ExistsAsync(id);
        }

        public Task<string> GetLanguageAsync(int id)
        {
            return _usersRepository.GetLanguageAsync(id);
        }

        public async Task<string> GetImageUriAsync(int id)
        {
            string imageUri = await _usersRepository.GetImageUriAsync(id);
            return _cdnService.ImageUriToThumbnail(imageUri);
        }

        public async Task<ToDoAssistantPreferences> GetToDoAssistantPreferencesAsync(int id)
        {
            User user = await _usersRepository.GetAsync(id);
            return _mapper.Map<ToDoAssistantPreferences>(user);
        }

        public async Task<CookingAssistantPreferences> GetCookingAssistantPreferencesAsync(int id)
        {
            User user = await _usersRepository.GetAsync(id);
            return _mapper.Map<CookingAssistantPreferences>(user);
        }

        public async Task UpdateToDoNotificationsEnabledAsync(int id, bool enabled)
        {
            await _usersRepository.UpdateToDoNotificationsEnabledAsync(id, enabled);
        }

        public async Task UpdateCookingNotificationsEnabledAsync(int id, bool enabled)
        {
            await _usersRepository.UpdateCookingNotificationsEnabledAsync(id, enabled);
        }

        public async Task UpdateImperialSystemAsync(int id, bool imperialSystem)
        {
            await _usersRepository.UpdateImperialSystemAsync(id, imperialSystem);
        }
    }
}
