using Chef.Application.Contracts.Ingredients.Models;
using FluentValidation;
using Sentry;

namespace Chef.Application.Contracts.Ingredients;

public interface IIngredientService
{
    List<IngredientDto> GetUserAndUsedPublicIngredients(int userId, ISpan metricsSpan);
    EditIngredient? GetForUpdate(int id, int userId, ISpan metricsSpan);
    ViewIngredient? GetPublic(int id, int userId, ISpan metricsSpan);
    IEnumerable<IngredientSuggestion> GetUserSuggestions(int userId, ISpan metricsSpan);
    PublicIngredientSuggestions GetPublicSuggestions(ISpan metricsSpan);
    IEnumerable<TaskSuggestion> GetTaskSuggestions(int userId, ISpan metricsSpan);
    bool Exists(int id, int userId);
    bool Exists(int id, string name, int userId);
    bool ExistsInRecipe(int id, int recipeId);
    Task UpdateAsync(UpdateIngredient model, IValidator<UpdateIngredient> validator, ISpan metricsSpan);
    Task UpdateAsync(UpdatePublicIngredient model, IValidator<UpdatePublicIngredient> validator, ISpan metricsSpan);
    Task DeleteOrRemoveFromRecipesAsync(int id, int userId, ISpan metricsSpan);
}
