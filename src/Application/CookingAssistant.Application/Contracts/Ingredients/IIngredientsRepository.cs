using Application.Domain.CookingAssistant;
using Application.Domain.ToDoAssistant;

namespace CookingAssistant.Application.Contracts.Ingredients;

public interface IIngredientsRepository
{
    IEnumerable<Ingredient> GetUserAndUsedPublicIngredients(int userId);
    Ingredient Get(int id);
    Ingredient GetForUpdate(int id, int userId);
    Ingredient GetPublic(int id, int userId);
    IEnumerable<Ingredient> GetForSuggestions(int userId);
    IEnumerable<IngredientCategory> GetIngredientCategories();
    IEnumerable<ToDoTask> GetTaskSuggestions(int userId);
    bool Exists(int id, int userId);
    bool Exists(int id, string name, int userId);
    bool ExistsInRecipe(int id, int recipeId);
    Task UpdateAsync(Ingredient ingredient);
    Task UpdatePublicAsync(int id, int? taskId, int userId, DateTime createdDate);
    Task DeleteAsync(int id);
    Task RemoveFromRecipesAsync(int id, int userId);
}
