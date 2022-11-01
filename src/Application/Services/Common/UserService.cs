using System;
using System.Threading.Tasks;
using Application.Contracts.Common;
using Application.Contracts.Common.Models;
using AutoMapper;
using Domain.Entities.Common;
using Microsoft.Extensions.Logging;

namespace Application.Services.Common;

public class UserService : IUserService
{
    private readonly ICdnService _cdnService;
    private readonly IUsersRepository _usersRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        ICdnService cdnService,
        IUsersRepository usersRepository,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _cdnService = cdnService;
        _usersRepository = usersRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public User Get(int id)
    {
        try
        {
            return _usersRepository.Get(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Get)}");
            throw;
        }
    }

    public User Get(string email)
    {
        try
        {
            return _usersRepository.Get(email.Trim());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Get)}");
            throw;
        }
    }

    public bool Exists(int id)
    {
        try
        {
            return _usersRepository.Exists(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Exists)}");
            throw;
        }
    }

    public string GetLanguage(int id)
    {
        try
        {
            return _usersRepository.GetLanguage(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetLanguage)}");
            throw;
        }
    }

    public string GetImageUri(int id)
    {
        try
        {
            string imageUri = _usersRepository.GetImageUri(id);
            return _cdnService.ImageUriToThumbnail(imageUri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetImageUri)}");
            throw;
        }
    }

    public ToDoAssistantPreferences GetToDoAssistantPreferences(int id)
    {
        try
        {
            User user = _usersRepository.Get(id);
            return _mapper.Map<ToDoAssistantPreferences>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetToDoAssistantPreferences)}");
            throw;
        }
    }

    public CookingAssistantPreferences GetCookingAssistantPreferences(int id)
    {
        try
        {
            User user = _usersRepository.Get(id);
            return _mapper.Map<CookingAssistantPreferences>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetCookingAssistantPreferences)}");
            throw;
        }
    }

    public async Task UpdateProfileAsync(int id, string name, string language, string culture, string imageUri)
    {
        try
        {
            await _usersRepository.UpdateProfileAsync(id, name.Trim(), language, culture, imageUri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateProfileAsync)}");
            throw;
        }
    }

    public async Task UpdateToDoNotificationsEnabledAsync(int id, bool enabled)
    {
        try
        {
            await _usersRepository.UpdateToDoNotificationsEnabledAsync(id, enabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateToDoNotificationsEnabledAsync)}");
            throw;
        }
    }

    public async Task UpdateCookingNotificationsEnabledAsync(int id, bool enabled)
    {
        try
        {
            await _usersRepository.UpdateCookingNotificationsEnabledAsync(id, enabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateCookingNotificationsEnabledAsync)}");
            throw;
        }
    }

    public async Task UpdateImperialSystemAsync(int id, bool imperialSystem)
    {
        try
        {
            await _usersRepository.UpdateImperialSystemAsync(id, imperialSystem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateImperialSystemAsync)}");
            throw;
        }
    }
}
