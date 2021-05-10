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
        Task<Recipe> GetWithOwnerAsync(int id, int userId);
        Task<IEnumerable<RecipeShare>> GetSharesAsync(int id);
        Task<IEnumerable<RecipeShare>> GetShareRequestsAsync(int userId);
        Task<int> GetPendingShareRequestsCountAsync(int userId);
        bool CanShareWithUser(int shareWithId, int userId);
        Task<Recipe> GetForSendingAsync(int id, int userId);
        Task<IEnumerable<SendRequest>> GetSendRequestsAsync(int userId);
        Task<int> GetPendingSendRequestsCountAsync(int userId);
        bool SendRequestExists(int id, int userId);
        bool IngredientsReviewIsRequired(int id, int userId);
        Task<Recipe> GetForReviewAsync(int id);
        Task<IEnumerable<string>> GetAllImageUrisAsync(int userId);
        Task<string> GetImageUriAsync(int id);
        bool UserOwns(int id, int userId);
        bool Exists(int id, int userId);
        bool Exists(string name, int userId);
        bool Exists(int id, string name, int userId);
        int Count(int userId);
        bool UserHasBlockedSharing(int recipeId, int userId, int sharedWithId);
        (bool canSend, bool alreadySent) CheckSendRequest(int recipeId, int sendToId, int userId);
        Task<int> CreateAsync(Recipe recipe);
        Task<Recipe> UpdateAsync(Recipe recipe, List<int> ingredientIdsToRemove);
        Task<string> DeleteAsync(int id);
        Task SaveSharingDetailsAsync(IEnumerable<RecipeShare> newShares, IEnumerable<RecipeShare> removedShares);
        Task SetShareIsAcceptedAsync(int id, int userId, bool isAccepted, DateTime modifiedDate);
        Task<RecipeShare> LeaveAsync(int id, int userId);
        Task CreateSendRequestsAsync(IEnumerable<SendRequest> sendRequests);
        Task DeclineSendRequestAsync(int id, int userId, DateTime modifiedDate);
        Task DeleteSendRequestAsync(int id, int userId);
        Task<int> ImportAsync(int id, IEnumerable<(int Id, int ReplacementId, bool TransferNutritionData, bool TransferPriceData)> ingredientReplacements, string imageUri, int userId);
    }
}
