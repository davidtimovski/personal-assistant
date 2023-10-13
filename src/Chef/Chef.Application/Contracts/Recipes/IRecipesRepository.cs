using Chef.Application.Entities;
using Sentry;
using User = Chef.Application.Entities.User;

namespace Chef.Application.Contracts.Recipes;

public interface IRecipesRepository
{
    IEnumerable<Recipe> GetAll(int userId, ISpan metricsSpan);
    Recipe Get(int id, ISpan metricsSpan);
    Recipe? Get(int id, int userId, ISpan metricsSpan);
    Recipe? GetForUpdate(int id, int userId, ISpan metricsSpan);
    Recipe? GetWithOwner(int id, int userId, ISpan metricsSpan);
    IEnumerable<RecipeShare> GetShares(int id, ISpan metricsSpan);
    IEnumerable<RecipeShare> GetShareRequests(int userId, ISpan metricsSpan);
    int GetPendingShareRequestsCount(int userId, ISpan metricsSpan);
    bool CanShareWithUser(int shareWithId, int userId, ISpan metricsSpan);
    Recipe GetForSending(int id, int userId, ISpan metricsSpan);
    IEnumerable<SendRequest> GetSendRequests(int userId, ISpan metricsSpan);
    int GetPendingSendRequestsCount(int userId, ISpan metricsSpan);
    bool SendRequestExists(int id, int userId);
    bool IngredientsReviewIsRequired(int id, int userId, ISpan metricsSpan);
    Recipe? GetForReview(int id, ISpan metricsSpan);
    IEnumerable<string> GetAllImageUris(int userId, ISpan metricsSpan);
    string GetImageUri(int id, ISpan metricsSpan);
    bool UserOwns(int id, int userId, ISpan metricsSpan);
    bool Exists(int id, int userId);
    bool Exists(string name, int userId);
    bool Exists(int id, string name, int userId);
    int Count(int userId);
    bool UserHasBlockedSharing(int recipeId, int userId, int sharedWithId, ISpan metricsSpan);
    (bool canSend, bool alreadySent) CheckSendRequest(int recipeId, int sendToId, int userId, ISpan metricsSpan);
    IEnumerable<User> GetUsersToBeNotifiedOfRecipeChange(int id, int excludeUserId, ISpan metricsSpan);
    bool CheckIfUserCanBeNotifiedOfRecipeChange(int id, int userId, ISpan metricsSpan);
    IEnumerable<User> GetUsersToBeNotifiedOfRecipeDeletion(int id, ISpan metricsSpan);
    IEnumerable<User> GetUsersToBeNotifiedOfRecipeSent(int id, ISpan metricsSpan);
    Task<int> CreateAsync(Recipe recipe, ISpan metricsSpan, CancellationToken cancellationToken);
    /// <remarks>
    /// If the recipe does not belong to the user
    /// it does not change the ingredients, only their Amount and Unit.
    /// </remarks>
    /// <returns>The Name of the original recipe</returns>
    Task<string> UpdateAsync(Recipe recipe, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    /// <returns>The Name of the deleted recipe</returns>
    Task<string> DeleteAsync(int id, ISpan metricsSpan, CancellationToken cancellationToken);
    Task SaveSharingDetailsAsync(IEnumerable<RecipeShare> newShares, IEnumerable<RecipeShare> removedShares, ISpan metricsSpan, CancellationToken cancellationToken);
    Task SetShareIsAcceptedAsync(int recipeId, int userId, bool isAccepted, DateTime modifiedDate, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<RecipeShare> LeaveAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task CreateSendRequestsAsync(IEnumerable<SendRequest> sendRequests, ISpan metricsSpan, CancellationToken cancellationToken);
    Task DeclineSendRequestAsync(int recipeId, int userId, DateTime modifiedDate, ISpan metricsSpan, CancellationToken cancellationToken);
    Task DeleteSendRequestAsync(int recipeId, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<int> ImportAsync(int id, IEnumerable<(int Id, int ReplacementId, bool TransferNutritionData, bool TransferPriceData)> ingredientReplacements, string imageUri, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
}
