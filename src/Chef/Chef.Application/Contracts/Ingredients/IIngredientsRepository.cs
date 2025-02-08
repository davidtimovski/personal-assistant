using Chef.Application.Entities;

namespace Chef.Application.Contracts.Ingredients;

public interface IIngredientsRepository
{
    IReadOnlyList<Ingredient> GetUserAndUsedPublicIngredients(int userId, ISpan metricsSpan);
    Ingredient? Get(int id, ISpan metricsSpan);
    Ingredient? GetForUpdate(int id, int userId, ISpan metricsSpan);
    Ingredient? GetPublic(int id, int userId, string? country, ISpan metricsSpan);
    IReadOnlyList<Ingredient> GetForSuggestions(int userId, ISpan metricsSpan);
    IReadOnlyList<Ingredient> GetPublicForSuggestions(string? country, ISpan metricsSpan);
    IReadOnlyList<IngredientCategory> GetIngredientCategories(ISpan metricsSpan);
    IReadOnlyList<ToDoTask> GetTaskSuggestions(int userId, ISpan metricsSpan);
    bool Exists(int id, int userId);
    bool Exists(int id, string name, int userId);
    bool ExistsInRecipe(int id, int recipeId);
    Task UpdateAsync(Ingredient ingredient, ISpan metricsSpan, CancellationToken cancellationToken);
    Task UpdatePublicAsync(int id, int? taskId, int userId, DateTime createdDate, ISpan metricsSpan, CancellationToken cancellationToken);
    Task DeleteAsync(int id, ISpan metricsSpan, CancellationToken cancellationToken);
    Task RemoveFromRecipesAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
}
