using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Application.Contracts.Common.Models;
using PersonalAssistant.Domain.Entities;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Application.Contracts.Common
{
    public interface IUserService
    {
        Task<User> GetAsync(int id);
        Task<User> GetAsync(string email);
        Task<IEnumerable<User>> GetToBeNotifiedOfListChangeAsync(int listId, int excludeUserId);
        Task<IEnumerable<User>> GetToBeNotifiedOfRecipeChangeAsync(int recipeId, int excludeUserId);
        Task<IEnumerable<User>> GetToBeNotifiedOfListChangeAsync(int listId, int excludeUserId, bool isPrivate);
        Task<IEnumerable<User>> GetToBeNotifiedOfListChangeAsync(int listId, int excludeUserId, int taskId);
        Task<bool> CheckIfUserCanBeNotifiedOfListChangeAsync(int listId, int userId);
        Task<bool> CheckIfUserCanBeNotifiedOfRecipeChangeAsync(int recipeId, int userId);
        Task<IEnumerable<User>> GetToBeNotifiedOfListDeletionAsync(int listId);
        Task<IEnumerable<User>> GetToBeNotifiedOfRecipeDeletionAsync(int recipeId);
        Task<IEnumerable<User>> GetToBeNotifiedOfRecipeSentAsync(int recipeId);
        bool Exists(int id);
        Task<string> GetLanguageAsync(int id);
        Task<string> GetImageUriAsync(int id);
        Task<ToDoAssistantPreferences> GetToDoAssistantPreferencesAsync(int id);
        Task<CookingAssistantPreferences> GetCookingAssistantPreferencesAsync(int id);
        Task UpdateToDoNotificationsEnabledAsync(int id, bool enabled);
        Task UpdateCookingNotificationsEnabledAsync(int id, bool enabled);
        Task UpdateImperialSystemAsync(int id, bool imperialSystem);
    }
}
