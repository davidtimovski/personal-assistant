using AutoMapper;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using Core.Application.Utils;
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
        IUsersRepository? usersRepository,
        IMapper? mapper,
        ILogger<UserService>? logger)
    {
        _usersRepository = ArgValidator.NotNull(usersRepository);
        _mapper = ArgValidator.NotNull(mapper);
        _logger = ArgValidator.NotNull(logger);
    }

    public Result<User> Get(int id)
    {
        try
        {
            var result = _usersRepository.Get(id);
            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Get)}");
            return new();
        }
    }

    public Result<User> Get(string email)
    {
        try
        {
            var result = _usersRepository.Get(email.Trim());
            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Get)}");
            return new();
        }
    }

    public Result<T> Get<T>(int id) where T : UserDto
    {
        try
        {
            var user = _usersRepository.Get(id);
            var result = _mapper.Map<T>(user);

            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Get)}<T>");
            return new();
        }
    }

    public Result<bool> Exists(int id)
    {
        try
        {
            var exists = _usersRepository.Exists(id);
            return new(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Exists)}");
            return new();
        }
    }

    public Result<ChefPreferences> GetChefPreferences(int id, ISpan metricsSpan)
    {
        try
        {
            User user = _usersRepository.Get(id);
            var result = _mapper.Map<ChefPreferences>(user);

            return new(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetChefPreferences)}");
            return new();
        }
    }

    public async Task<Result<int>> CreateAsync(string auth0Id, string email, string name, string language, string culture, string imageUri, ISpan metricsSpan, CancellationToken cancellationToken)
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

            var id = await _usersRepository.CreateAsync(auth0Id, user, metric, cancellationToken);

            return new(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<Result> UpdateProfileAsync(int id, string name, string language, string culture, string imageUri, ISpan metricsSpan, CancellationToken cancellationToken)
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

            return new(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateProfileAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<Result> UpdateToDoNotificationsEnabledAsync(int id, bool enabled, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(UserService)}.{nameof(UpdateToDoNotificationsEnabledAsync)}");

        try
        {
            var user = _usersRepository.Get(id);
            user.ToDoNotificationsEnabled = enabled;
            user.ModifiedDate = DateTime.UtcNow;

            await _usersRepository.UpdateAsync(user, metric, cancellationToken);

            return new(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateToDoNotificationsEnabledAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<Result> UpdateChefNotificationsEnabledAsync(int id, bool enabled, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(UserService)}.{nameof(UpdateChefNotificationsEnabledAsync)}");

        try
        {
            var user = _usersRepository.Get(id);
            user.ChefNotificationsEnabled = enabled;
            user.ModifiedDate = DateTime.UtcNow;

            await _usersRepository.UpdateAsync(user, metric, cancellationToken);

            return new(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateChefNotificationsEnabledAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<Result> UpdateImperialSystemAsync(int id, bool imperialSystem, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(UserService)}.{nameof(UpdateImperialSystemAsync)}");

        try
        {
            var user = _usersRepository.Get(id);
            user.ImperialSystem = imperialSystem;
            user.ModifiedDate = DateTime.UtcNow;

            await _usersRepository.UpdateAsync(user, metric, cancellationToken);

            return new(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateImperialSystemAsync)}");
            return new();
        }
        finally
        {
            metric.Finish();
        }
    }
}
