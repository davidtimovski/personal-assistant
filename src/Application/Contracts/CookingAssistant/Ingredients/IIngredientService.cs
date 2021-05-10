using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients.Models;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients
{
    public interface IIngredientService
    {
        Task<IEnumerable<IngredientDto>> GetAllAsync(int userId);
        Task<EditIngredient> GetAsync(int id, int userId);
        Task<IEnumerable<IngredientSuggestion>> GetSuggestionsAsync(int recipeId, int userId);
        Task<IEnumerable<IngredientSuggestion>> GetTaskSuggestionsAsync(int userId);
        Task<IEnumerable<IngredientSuggestion>> GetTaskSuggestionsAsync(int recipeId, int userId);
        Task<IEnumerable<IngredientSuggestion>> GetIngredientSuggestionsAsync(int userId);
        Task<IEnumerable<IngredientReviewSuggestion>> GetIngredientReviewSuggestionsAsync(int userId);
        bool Exists(int id, int userId);
        bool Exists(int id, string name, int userId);
        bool ExistsInRecipe(int id, int recipeId);
        Task UpdateAsync(UpdateIngredient model, IValidator<UpdateIngredient> validator);
        Task DeleteAsync(int id, int userId);
    }
}
