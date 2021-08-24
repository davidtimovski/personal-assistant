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

        public User Get(int id)
        {
            return _usersRepository.Get(id);
        }

        public User Get(string email)
        {
            return _usersRepository.Get(email.Trim());
        }

        public bool Exists(int id)
        {
            return _usersRepository.Exists(id);
        }

        public string GetLanguage(int id)
        {
            return _usersRepository.GetLanguage(id);
        }

        public string GetImageUri(int id)
        {
            string imageUri = _usersRepository.GetImageUri(id);
            return _cdnService.ImageUriToThumbnail(imageUri);
        }

        public ToDoAssistantPreferences GetToDoAssistantPreferences(int id)
        {
            User user = _usersRepository.Get(id);
            return _mapper.Map<ToDoAssistantPreferences>(user);
        }

        public CookingAssistantPreferences GetCookingAssistantPreferences(int id)
        {
            User user = _usersRepository.Get(id);
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
