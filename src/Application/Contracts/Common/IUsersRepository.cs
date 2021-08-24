using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Application.Contracts.Common
{
    public interface IUsersRepository
    {
        User Get(int id);
        User Get(string email);
        bool Exists(int id);
        string GetLanguage(int id);
        string GetImageUri(int id);
        Task UpdateToDoNotificationsEnabledAsync(int id, bool enabled);
        Task UpdateCookingNotificationsEnabledAsync(int id, bool enabled);
        Task UpdateImperialSystemAsync(int id, bool imperialSystem);
    }
}
