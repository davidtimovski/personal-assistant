using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities.CookingAssistant;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Recipes
{
    public interface IRecipesRepository
    {
        Task<IEnumerable<Recipe>> GetAllAsync(int userId);
        Task<Recipe> GetAsync(int id);
        Task<Recipe> GetAsync(int id, int userId);
        Task<Recipe> GetForUpdateAsync(int id, int userId);
        Task<Recipe> GetForSendingAsync(int id, int userId);
        Task<IEnumerable<SendRequest>> GetSendRequestsAsync(int userId);
        Task<int> GetPendingSendRequestsCountAsync(int userId);
        Task<bool> SendRequestExistsAsync(int id, int userId);
        Task<bool> IngredientsReviewIsRequiredAsync(int id, int userId);
        Task<Recipe> GetForReviewAsync(int id);
        Task<IEnumerable<string>> GetAllImageUrisAsync(int userId);
        Task<string> GetImageUriAsync(int id);
        Task<bool> ExistsAsync(int id, int userId);
        Task<bool> ExistsAsync(string name, int userId);
        Task<bool> ExistsAsync(int id, string name, int userId);
        Task<int> CountAsync(int userId);
        Task<(bool canSend, bool alreadySent)> CheckSendRequestAsync(int recipeId, int sendToId, int userId);
        Task<int> CreateAsync(Recipe recipe);
        Task UpdateAsync(Recipe recipe, List<int> ingredientIdsToRemove);
        Task DeleteAsync(int id);
        Task CreateSendRequestsAsync(IEnumerable<SendRequest> sendRequests);
        Task DeclineSendRequestAsync(int id, int userId, DateTime modifiedDate);
        Task DeleteSendRequestAsync(int id, int userId);
        Task<int> ImportAsync(int id, IEnumerable<(int Id, int ReplacementId, bool TransferNutritionData, bool TransferPriceData)> ingredientReplacements, string imageUri, int userId);
    }
}
