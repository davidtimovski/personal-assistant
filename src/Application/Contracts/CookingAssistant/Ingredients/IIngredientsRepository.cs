using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities.CookingAssistant;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients
{
    public interface IIngredientsRepository
    {
        Task<IEnumerable<Ingredient>> GetAllAsync(int userId);
        Task<Ingredient> GetAsync(int id, int userId);
        Task<IEnumerable<Ingredient>> GetSuggestionsAsync(int recipeId, int userId);
        Task<IEnumerable<ToDoTask>> GetTaskSuggestionsAsync(int userId);
        Task<IEnumerable<Ingredient>> GetTaskSuggestionsAsync(int recipeId, int userId);
        Task<IEnumerable<Ingredient>> GetIngredientSuggestionsAsync(int userId);
        bool Exists(int id, int userId);
        bool Exists(int id, string name, int userId);
        bool ExistsInRecipe(int id, int recipeId);
        Task UpdateAsync(Ingredient ingredient);
        Task DeleteAsync(int id);
    }
}
