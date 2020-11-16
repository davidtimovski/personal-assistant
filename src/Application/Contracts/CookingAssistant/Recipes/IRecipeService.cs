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
        Task<IEnumerable<SendRequestDto>> GetSendRequestsAsync(int userId);
        Task<int> GetPendingSendRequestsCountAsync(int userId);
        Task<bool> SendRequestExistsAsync(int id, int userId);
        Task<bool> IngredientsReviewIsRequiredAsync(int id, int userId);
        Task<RecipeForReview> GetForReviewAsync(int id, int userId);
        Task<IEnumerable<string>> GetAllImageUrisAsync(int userId);
        Task<string> GetImageUriAsync(int id);
        Task<bool> ExistsAsync(int id, int userId);
        Task<bool> ExistsAsync(string name, int userId);
        Task<bool> ExistsAsync(int id, string name, int userId);
        Task<int> CountAsync(int userId);
        Task<(bool canSend, bool alreadySent)> CheckSendRequestAsync(int recipeId, int sendToId, int userId);
        Task<int> CreateAsync(CreateRecipe model, IValidator<CreateRecipe> validator);
        Task CreateSampleAsync(int userId, Dictionary<string, string> translations);
        Task UpdateAsync(UpdateRecipe model, IValidator<UpdateRecipe> validator);
        Task DeleteAsync(int id, int userId);
        Task SendAsync(CreateSendRequest model, IValidator<CreateSendRequest> validator);
        Task DeclineSendRequestAsync(int id, int userId);
        Task DeleteSendRequestAsync(int id, int userId);
        Task<int> ImportAsync(ImportRecipe model, IValidator<ImportRecipe> validator);
    }
}
