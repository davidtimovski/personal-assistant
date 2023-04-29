using Core.Application.Contracts.Models;
using Sentry;
using User = Application.Domain.Common.User;

namespace Core.Application.Contracts;

public interface IUserService
{
    User Get(int id);
    User Get(string email);
    T Get<T>(int id) where T : UserDto;
    bool Exists(int id);
    CookingAssistantPreferences GetCookingAssistantPreferences(int id, ITransaction tr);
    Task<int> CreateAsync(string auth0Id, string email, string name, string language, string culture, string imageUri, ITransaction tr);
    Task UpdateProfileAsync(int id, string name, string language, string culture, string imageUri, ITransaction tr);
    Task UpdateToDoNotificationsEnabledAsync(int id, bool enabled, ITransaction tr);
    Task UpdateCookingNotificationsEnabledAsync(int id, bool enabled, ITransaction tr);
    Task UpdateImperialSystemAsync(int id, bool imperialSystem, ITransaction tr);
}
