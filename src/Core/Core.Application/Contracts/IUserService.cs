using Core.Application.Contracts.Models;
using Sentry;
using User = Core.Application.Entities.User;

namespace Core.Application.Contracts;

public interface IUserService
{
    Result<User> Get(int id);
    Result<User> Get(string email);
    Result<T> Get<T>(int id) where T : UserDto;
    Result<bool> Exists(int id);
    Result<ChefPreferences> GetChefPreferences(int id, ISpan metricsSpan);
    Task<Result<int>> CreateAsync(string auth0Id, string email, string name, string? country, string language, string culture, string imageUri, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<Result> UpdateProfileAsync(int id, string name, string? country, string language, string culture, string imageUri, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<Result> UpdateToDoNotificationsEnabledAsync(int id, bool enabled, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<Result> UpdateChefNotificationsEnabledAsync(int id, bool enabled, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<Result> UpdateImperialSystemAsync(int id, bool imperialSystem, ISpan metricsSpan, CancellationToken cancellationToken);
}
