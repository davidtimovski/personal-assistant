using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities.Common;
using PersonalAssistant.Domain.Entities.CookingAssistant;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Recipes
{
    public interface IRecipesRepository
    {
        IEnumerable<Recipe> GetAll(int userId);
        Recipe Get(int id);
        Recipe Get(int id, int userId);
        Recipe GetForUpdate(int id, int userId);
        Recipe GetWithOwner(int id, int userId);
        IEnumerable<RecipeShare> GetShares(int id);
        IEnumerable<RecipeShare> GetShareRequests(int userId);
        int GetPendingShareRequestsCount(int userId);
        bool CanShareWithUser(int shareWithId, int userId);
        Recipe GetForSending(int id, int userId);
        IEnumerable<SendRequest> GetSendRequests(int userId);
        int GetPendingSendRequestsCount(int userId);
        bool SendRequestExists(int id, int userId);
        bool IngredientsReviewIsRequired(int id, int userId);
        Recipe GetForReview(int id);
        IEnumerable<string> GetAllImageUris(int userId);
        string GetImageUri(int id);
        bool UserOwns(int id, int userId);
        bool Exists(int id, int userId);
        bool Exists(string name, int userId);
        bool Exists(int id, string name, int userId);
        int Count(int userId);
        bool UserHasBlockedSharing(int recipeId, int userId, int sharedWithId);
        (bool canSend, bool alreadySent) CheckSendRequest(int recipeId, int sendToId, int userId);
        IEnumerable<User> GetUsersToBeNotifiedOfRecipeChange(int id, int excludeUserId);
        bool CheckIfUserCanBeNotifiedOfRecipeChange(int id, int userId);
        IEnumerable<User> GetUsersToBeNotifiedOfRecipeDeletion(int id);
        IEnumerable<User> GetUsersToBeNotifiedOfRecipeSent(int id);
        Task<int> CreateAsync(Recipe recipe);
        Task<Recipe> UpdateAsync(Recipe recipe, List<int> ingredientIdsToRemove);
        Task<string> DeleteAsync(int id);
        Task SaveSharingDetailsAsync(IEnumerable<RecipeShare> newShares, IEnumerable<RecipeShare> removedShares);
        Task SetShareIsAcceptedAsync(int recipeId, int userId, bool isAccepted, DateTime modifiedDate);
        Task<RecipeShare> LeaveAsync(int id, int userId);
        Task CreateSendRequestsAsync(IEnumerable<SendRequest> sendRequests);
        Task DeclineSendRequestAsync(int recipeId, int userId, DateTime modifiedDate);
        Task DeleteSendRequestAsync(int recipeId, int userId);
        Task<int> ImportAsync(int id, IEnumerable<(int Id, int ReplacementId, bool TransferNutritionData, bool TransferPriceData)> ingredientReplacements, string imageUri, int userId);
    }
}
