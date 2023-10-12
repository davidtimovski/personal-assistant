using Chef.Application.Contracts.Recipes.Models;
using FluentValidation;
using Sentry;

namespace Chef.Application.Contracts.Recipes;

public interface IRecipeService
{
    IEnumerable<SimpleRecipe> GetAll(int userId, ISpan metricsSpan);
    RecipeToNotify Get(int id, ISpan metricsSpan);
    RecipeDto? Get(int id, int userId, string currency, ISpan metricsSpan);
    RecipeForUpdate? GetForUpdate(int id, int userId, ISpan metricsSpan);
    RecipeForSending GetForSending(int id, int userId, ISpan metricsSpan);
    RecipeWithShares? GetWithShares(int id, int userId, ISpan metricsSpan);
    IEnumerable<ShareRecipeRequest> GetShareRequests(int userId, ISpan metricsSpan);
    int GetPendingShareRequestsCount(int userId, ISpan metricsSpan);
    bool CanShareWithUser(int shareWithId, int userId, ISpan metricsSpan);
    IEnumerable<SendRequestDto> GetSendRequests(int userId, ISpan metricsSpan);
    int GetPendingSendRequestsCount(int userId, ISpan metricsSpan);
    bool SendRequestExists(int id, int userId);
    bool IngredientsReviewIsRequired(int id, int userId, ISpan metricsSpan);
    RecipeForReview? GetForReview(int id, int userId, ISpan metricsSpan);
    IEnumerable<string> GetAllImageUris(int userId, ISpan metricsSpan);
    bool Exists(int id, int userId);
    bool Exists(string name, int userId);
    bool Exists(int id, string name, int userId);
    int Count(int userId);
    (bool canSend, bool alreadySent) CheckSendRequest(int recipeId, int sendToId, int userId, ISpan metricsSpan);
    bool CheckIfUserCanBeNotifiedOfRecipeChange(int id, int userId, ISpan metricsSpan);
    Task<int> CreateAsync(CreateRecipe model, IValidator<CreateRecipe> validator, ISpan metricsSpan);
    Task CreateSampleAsync(int userId, Dictionary<string, string> translations, ISpan metricsSpan);
    Task<UpdateRecipeResult> UpdateAsync(UpdateRecipe model, IValidator<UpdateRecipe> validator, ISpan metricsSpan);
    Task<DeleteRecipeResult> DeleteAsync(int id, int userId, ISpan metricsSpan);
    Task ShareAsync(ShareRecipe model, IValidator<ShareRecipe> validator, ISpan metricsSpan);
    Task<SetShareIsAcceptedResult> SetShareIsAcceptedAsync(int recipeId, int userId, bool isAccepted, ISpan metricsSpan);
    Task<LeaveRecipeResult> LeaveAsync(int id, int userId, ISpan metricsSpan);
    Task<SendRecipeResult> SendAsync(CreateSendRequest model, IValidator<CreateSendRequest> validator, ISpan metricsSpan);
    Task<DeclineSendRequestResult> DeclineSendRequestAsync(int recipeId, int userId, ISpan metricsSpan);
    Task DeleteSendRequestAsync(int recipeId, int userId, ISpan metricsSpan);
    Task<int> ImportAsync(ImportRecipe model, IValidator<ImportRecipe> validator, ISpan metricsSpan);
}
