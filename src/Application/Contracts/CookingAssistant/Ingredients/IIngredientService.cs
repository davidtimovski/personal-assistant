using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Application.Contracts.CookingAssistant.Ingredients.Models;
using Application.Contracts.CookingAssistant.Recipes.Models;

namespace Application.Contracts.CookingAssistant.Ingredients
{
    public interface IIngredientService
    {
        IEnumerable<IngredientDto> GetAll(int userId);
        EditIngredient Get(int id, int userId);
        IEnumerable<IngredientSuggestion> GetSuggestions(int recipeId, int userId);
        IEnumerable<IngredientSuggestion> GetTaskSuggestions(int userId);
        IEnumerable<IngredientSuggestion> GetTaskSuggestions(int recipeId, int userId);
        IEnumerable<IngredientReviewSuggestion> GetIngredientReviewSuggestions(int userId);
        bool Exists(int id, int userId);
        bool Exists(int id, string name, int userId);
        bool ExistsInRecipe(int id, int recipeId);
        Task UpdateAsync(UpdateIngredient model, IValidator<UpdateIngredient> validator);
        Task DeleteAsync(int id, int userId);
    }
}
