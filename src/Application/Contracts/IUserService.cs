using Application.Contracts.Models;
using Domain.Common;

namespace Application.Contracts;

public interface IUserService
{
    User Get(int id);
    User Get(string email);
    T Get<T>(int id) where T : UserDto;
    bool Exists(int id);
    CookingAssistantPreferences GetCookingAssistantPreferences(int id);
    Task<int> CreateAsync(string auth0Id, string email, string name, string language, string culture, string imageUri);
    Task UpdateProfileAsync(int id, string name, string language, string culture, string imageUri);
    Task UpdateToDoNotificationsEnabledAsync(int id, bool enabled);
    Task UpdateCookingNotificationsEnabledAsync(int id, bool enabled);
    Task UpdateImperialSystemAsync(int id, bool imperialSystem);
}
