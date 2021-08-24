using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Recipes
{
    public interface IRecipeService
    {
        IEnumerable<SimpleRecipe> GetAll(int userId);
        RecipeToNotify Get(int id);
        RecipeDto Get(int id, int userId, string currency);
        RecipeForUpdate GetForUpdate(int id, int userId);
        RecipeForSending GetForSending(int id, int userId);
        RecipeWithShares GetWithShares(int id, int userId);
        IEnumerable<ShareRecipeRequest> GetShareRequests(int userId);
        int GetPendingShareRequestsCount(int userId);
        bool CanShareWithUser(int shareWithId, int userId);
        IEnumerable<SendRequestDto> GetSendRequests(int userId);
        int GetPendingSendRequestsCount(int userId);
        bool SendRequestExists(int id, int userId);
        bool IngredientsReviewIsRequired(int id, int userId);
        RecipeForReview GetForReview(int id, int userId);
        IEnumerable<string> GetAllImageUris(int userId);
        string GetImageUri(int id);
        bool Exists(int id, int userId);
        bool Exists(string name, int userId);
        bool Exists(int id, string name, int userId);
        int Count(int userId);
        (bool canSend, bool alreadySent) CheckSendRequest(int recipeId, int sendToId, int userId);
        IEnumerable<User> GetUsersToBeNotifiedOfRecipeChange(int id, int excludeUserId);
        bool CheckIfUserCanBeNotifiedOfRecipeChange(int id, int userId);
        IEnumerable<User> GetUsersToBeNotifiedOfRecipeDeletion(int id);
        IEnumerable<User> GetUsersToBeNotifiedOfRecipeSent(int id);
        Task<int> CreateAsync(CreateRecipe model, IValidator<CreateRecipe> validator);
        Task CreateSampleAsync(int userId, Dictionary<string, string> translations);
        Task<UpdateRecipeResult> UpdateAsync(UpdateRecipe model, IValidator<UpdateRecipe> validator);
        Task<DeleteRecipeResult> DeleteAsync(int id, int userId);
        Task ShareAsync(ShareRecipe model, IValidator<ShareRecipe> validator);
        Task<SetShareIsAcceptedResult> SetShareIsAcceptedAsync(int id, int userId, bool isAccepted);
        Task<LeaveRecipeResult> LeaveAsync(int id, int userId);
        Task<SendRecipeResult> SendAsync(CreateSendRequest model, IValidator<CreateSendRequest> validator);
        Task<DeclineSendRequestResult> DeclineSendRequestAsync(int id, int userId);
        Task DeleteSendRequestAsync(int id, int userId);
        Task<int> ImportAsync(ImportRecipe model, IValidator<ImportRecipe> validator);
    }
}
