using Application.Contracts;
using Application.Contracts.Models;
using AutoMapper;
using Domain.Common;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUsersRepository usersRepository,
        IMapper mapper,
        ILogger<UserService> logger)
    {
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

    public T Get<T>(int id) where T : UserDto
    {
        try
        {
            var user = _usersRepository.Get(id);
            var result = _mapper.Map<T>(user);

            return result;
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

    public async Task<int> CreateAsync(string auth0Id, string email, string name, string language, string culture, string imageUri)
    {
        try
        {
            var user = new User
            {
                Email = email.Trim(),
                Name = name.Trim(),
                Language = language,
                Culture = culture.Trim(),
                ToDoNotificationsEnabled = false,
                CookingNotificationsEnabled = false,
                ImperialSystem = false,
                ImageUri = imageUri.Trim(),
                ModifiedDate = DateTime.UtcNow,
            };

            return await _usersRepository.CreateAsync(auth0Id, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateAsync)}");
            throw;
        }
    }

    public async Task UpdateProfileAsync(int id, string name, string language, string culture, string imageUri)
    {
        try
        {
            var user = _usersRepository.Get(id);
            user.Name = name;
            user.Language = language;
            user.Culture = culture;
            user.ImageUri = imageUri;
            user.ModifiedDate = DateTime.UtcNow;

            await _usersRepository.UpdateAsync(user);
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
            var user = _usersRepository.Get(id);
            user.ToDoNotificationsEnabled = enabled;
            user.ModifiedDate = DateTime.UtcNow;

            await _usersRepository.UpdateAsync(user);
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
            var user = _usersRepository.Get(id);
            user.CookingNotificationsEnabled = enabled;
            user.ModifiedDate = DateTime.UtcNow;

            await _usersRepository.UpdateAsync(user);
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
            var user = _usersRepository.Get(id);
            user.ImperialSystem = imperialSystem;
            user.ModifiedDate = DateTime.UtcNow;

            await _usersRepository.UpdateAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateImperialSystemAsync)}");
            throw;
        }
    }
}
