using Chef.Application.Contracts.Recipes.Models;
using FluentValidation;

namespace Chef.Application.Contracts.Recipes;

public interface IRecipeService
{
    IReadOnlyList<SimpleRecipe> GetAll(int userId, ISpan metricsSpan);
    RecipeToNotify Get(int id, ISpan metricsSpan);
    RecipeDto? Get(int id, int userId, string currency, ISpan metricsSpan);
    RecipeForUpdate? GetForUpdate(int id, int userId, ISpan metricsSpan);
    RecipeForSending? GetForSending(int id, int userId, ISpan metricsSpan);
    RecipeWithShares? GetWithShares(int id, int userId, ISpan metricsSpan);
    IReadOnlyList<ShareRecipeRequest> GetShareRequests(int userId, ISpan metricsSpan);
    int GetPendingShareRequestsCount(int userId, ISpan metricsSpan);
    bool CanShareWithUser(int shareWithId, int userId, ISpan metricsSpan);
    IReadOnlyList<SendRequestDto> GetSendRequests(int userId, ISpan metricsSpan);
    int GetPendingSendRequestsCount(int userId, ISpan metricsSpan);
    bool SendRequestExists(int id, int userId);
    bool IngredientsReviewIsRequired(int id, int userId, ISpan metricsSpan);
    RecipeForReview? GetForReview(int id, int userId, ISpan metricsSpan);
    IReadOnlyList<string> GetAllImageUris(int userId, ISpan metricsSpan);
    bool Exists(int id, int userId);
    bool Exists(string name, int userId);
    bool Exists(int id, string name, int userId);
    int Count(int userId);
    (bool canSend, bool alreadySent) CheckSendRequest(int recipeId, int sendToId, int userId, ISpan metricsSpan);
    bool CheckIfUserCanBeNotifiedOfRecipeChange(int id, int userId, ISpan metricsSpan);
    Task<int> CreateAsync(CreateRecipe model, IValidator<CreateRecipe> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task CreateSampleAsync(int userId, Dictionary<string, string> translations, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<UpdateRecipeResult> UpdateAsync(UpdateRecipe model, IValidator<UpdateRecipe> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<DeleteRecipeResult> DeleteAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task ShareAsync(ShareRecipe model, IValidator<ShareRecipe> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<SetShareIsAcceptedResult> SetShareIsAcceptedAsync(int recipeId, int userId, bool isAccepted, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<LeaveRecipeResult> LeaveAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<SendRecipeResult> SendAsync(CreateSendRequest model, IValidator<CreateSendRequest> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<DeclineSendRequestResult> DeclineSendRequestAsync(int recipeId, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task DeleteSendRequestAsync(int recipeId, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
    Task<int> ImportAsync(ImportRecipe model, IValidator<ImportRecipe> validator, ISpan metricsSpan, CancellationToken cancellationToken);
}
