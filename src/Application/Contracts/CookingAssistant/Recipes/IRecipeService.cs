using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Recipes
{
    public interface IRecipeService
    {
        Task<IEnumerable<SimpleRecipe>> GetAllAsync(int userId);
        Task<RecipeToNotify> GetAsync(int id);
        Task<RecipeDto> GetAsync(int id, int userId, string currency);
        Task<RecipeForUpdate> GetForUpdateAsync(int id, int userId);
        Task<RecipeForSending> GetForSendingAsync(int id, int userId);
        Task<RecipeWithShares> GetWithSharesAsync(int id, int userId);
        Task<IEnumerable<ShareRecipeRequest>> GetShareRequestsAsync(int userId);
        Task<int> GetPendingShareRequestsCountAsync(int userId);
        bool CanShareWithUser(int shareWithId, int userId);
        Task<IEnumerable<SendRequestDto>> GetSendRequestsAsync(int userId);
        Task<int> GetPendingSendRequestsCountAsync(int userId);
        bool SendRequestExists(int id, int userId);
        bool IngredientsReviewIsRequired(int id, int userId);
        Task<RecipeForReview> GetForReviewAsync(int id, int userId);
        Task<IEnumerable<string>> GetAllImageUrisAsync(int userId);
        Task<string> GetImageUriAsync(int id);
        bool Exists(int id, int userId);
        bool Exists(string name, int userId);
        bool Exists(int id, string name, int userId);
        int Count(int userId);
        (bool canSend, bool alreadySent) CheckSendRequest(int recipeId, int sendToId, int userId);
        Task<int> CreateAsync(CreateRecipe model, IValidator<CreateRecipe> validator);
        Task CreateSampleAsync(int userId, Dictionary<string, string> translations);
        Task<RecipeToNotify> UpdateAsync(UpdateRecipe model, IValidator<UpdateRecipe> validator);
        Task<string> DeleteAsync(int id, int userId);
        Task ShareAsync(ShareRecipe model, IValidator<ShareRecipe> validator);
        Task SetShareIsAcceptedAsync(int id, int userId, bool isAccepted);
        Task<bool> LeaveAsync(int id, int userId);
        Task SendAsync(CreateSendRequest model, IValidator<CreateSendRequest> validator);
        Task DeclineSendRequestAsync(int id, int userId);
        Task DeleteSendRequestAsync(int id, int userId);
        Task<int> ImportAsync(ImportRecipe model, IValidator<ImportRecipe> validator);
    }
}
