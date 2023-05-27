using Core.Application.Contracts.Models;
using Sentry;
using User = Core.Application.Entities.User;

namespace Core.Application.Contracts;

public interface IUserService
{
    User Get(int id);
    User Get(string email);
    T Get<T>(int id) where T : UserDto;
    bool Exists(int id);
    CookingAssistantPreferences GetCookingAssistantPreferences(int id, ISpan metricsSpan);
    Task<int> CreateAsync(string auth0Id, string email, string name, string language, string culture, string imageUri, ISpan metricsSpan);
    Task UpdateProfileAsync(int id, string name, string language, string culture, string imageUri, ISpan metricsSpan);
    Task UpdateToDoNotificationsEnabledAsync(int id, bool enabled, ISpan metricsSpan);
    Task UpdateCookingNotificationsEnabledAsync(int id, bool enabled, ISpan metricsSpan);
    Task UpdateImperialSystemAsync(int id, bool imperialSystem, ISpan metricsSpan);
}
