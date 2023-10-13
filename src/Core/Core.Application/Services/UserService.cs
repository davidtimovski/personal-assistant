using AutoMapper;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using Microsoft.Extensions.Logging;
using Sentry;
using User = Core.Application.Entities.User;

namespace Core.Application.Services;

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
            _logger.LogError(ex, $"Unexpected error in {nameof(Get)}<T>");
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

    public ChefPreferences GetChefPreferences(int id, ISpan metricsSpan)
    {
        try
        {
            User user = _usersRepository.Get(id);
            return _mapper.Map<ChefPreferences>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetChefPreferences)}");
            throw;
        }
    }

    public async Task<int> CreateAsync(string auth0Id, string email, string name, string language, string culture, string imageUri, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(UserService)}.{nameof(CreateAsync)}");

        try
        {
            var user = new User
            {
                Email = email.Trim(),
                Name = name.Trim(),
                Language = language,
                Culture = culture.Trim(),
                ToDoNotificationsEnabled = false,
                ChefNotificationsEnabled = false,
                ImperialSystem = false,
                ImageUri = imageUri.Trim(),
                ModifiedDate = DateTime.UtcNow,
            };

            return await _usersRepository.CreateAsync(auth0Id, user, metric, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task UpdateProfileAsync(int id, string name, string language, string culture, string imageUri, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(UserService)}.{nameof(UpdateProfileAsync)}");

        try
        {
            var user = _usersRepository.Get(id);
            user.Name = name;
            user.Language = language;
            user.Culture = culture;
            user.ImageUri = imageUri;
            user.ModifiedDate = DateTime.UtcNow;

            await _usersRepository.UpdateAsync(user, metric, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateProfileAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task UpdateToDoNotificationsEnabledAsync(int id, bool enabled, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(UserService)}.{nameof(UpdateToDoNotificationsEnabledAsync)}");

        try
        {
            var user = _usersRepository.Get(id);
            user.ToDoNotificationsEnabled = enabled;
            user.ModifiedDate = DateTime.UtcNow;

            await _usersRepository.UpdateAsync(user, metric, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateToDoNotificationsEnabledAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task UpdateChefNotificationsEnabledAsync(int id, bool enabled, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(UserService)}.{nameof(UpdateChefNotificationsEnabledAsync)}");

        try
        {
            var user = _usersRepository.Get(id);
            user.ChefNotificationsEnabled = enabled;
            user.ModifiedDate = DateTime.UtcNow;

            await _usersRepository.UpdateAsync(user, metric, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateChefNotificationsEnabledAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task UpdateImperialSystemAsync(int id, bool imperialSystem, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(UserService)}.{nameof(UpdateImperialSystemAsync)}");

        try
        {
            var user = _usersRepository.Get(id);
            user.ImperialSystem = imperialSystem;
            user.ModifiedDate = DateTime.UtcNow;

            await _usersRepository.UpdateAsync(user, metric, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateImperialSystemAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }
}
