using System.Threading.Tasks;
using PersonalAssistant.Application.Contracts.Common.Models;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Application.Contracts.Common
{
    public interface IUserService
    {
        User Get(int id);
        User Get(string email);
        bool Exists(int id);
        string GetLanguage(int id);
        string GetImageUri(int id);
        ToDoAssistantPreferences GetToDoAssistantPreferences(int id);
        CookingAssistantPreferences GetCookingAssistantPreferences(int id);
        Task UpdateToDoNotificationsEnabledAsync(int id, bool enabled);
        Task UpdateCookingNotificationsEnabledAsync(int id, bool enabled);
        Task UpdateImperialSystemAsync(int id, bool imperialSystem);
    }
}
