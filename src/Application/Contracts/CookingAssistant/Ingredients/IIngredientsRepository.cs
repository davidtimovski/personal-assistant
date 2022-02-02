using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities.CookingAssistant;
using Domain.Entities.ToDoAssistant;

namespace Application.Contracts.CookingAssistant.Ingredients;

public interface IIngredientsRepository
{
    IEnumerable<Ingredient> GetAll(int userId);
    Ingredient Get(int id, int userId);
    IEnumerable<Ingredient> GetSuggestions(int recipeId, int userId);
    IEnumerable<ToDoTask> GetTaskSuggestions(int userId);
    IEnumerable<Ingredient> GetTaskSuggestions(int recipeId, int userId);
    IEnumerable<Ingredient> GetIngredientSuggestions(int userId);
    bool Exists(int id, int userId);
    bool Exists(int id, string name, int userId);
    bool ExistsInRecipe(int id, int recipeId);
    Task UpdateAsync(Ingredient ingredient);
    Task DeleteAsync(int id);
}