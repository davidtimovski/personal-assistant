using System.Threading.Tasks;
using Domain.Entities.Common;

namespace Application.Contracts.Common;

public interface IUsersRepository
{
    User Get(int id);
    User Get(string email);
    int GetId(string auth0Id);
    bool Exists(int id);
    string GetLanguage(int id);
    string GetImageUri(int id);
    Task UpdateToDoNotificationsEnabledAsync(int id, bool enabled);
    Task UpdateCookingNotificationsEnabledAsync(int id, bool enabled);
    Task UpdateImperialSystemAsync(int id, bool imperialSystem);
}
