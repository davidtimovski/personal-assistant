using Chef.Application.Contracts.Ingredients.Models;
using FluentValidation;
using Sentry;

namespace Chef.Application.Contracts.Ingredients;

public interface IIngredientService
{
    IReadOnlyList<IngredientDto> GetUserAndUsedPublicIngredients(int userId, ISpan metricsSpan);
    EditIngredient? GetForUpdate(int id, int userId, ISpan metricsSpan);
    ViewIngredient? GetPublic(int id, int userId, ISpan metricsSpan);
    IReadOnlyList<IngredientSuggestion> GetUserSuggestions(int userId, ISpan metricsSpan);
    PublicIngredientSuggestions GetPublicSuggestions(ISpan metricsSpan);
    IReadOnlyList<TaskSuggestion> GetTaskSuggestions(int userId, ISpan metricsSpan);
    bool Exists(int id, int userId);
    bool Exists(int id, string name, int userId);
    bool ExistsInRecipe(int id, int recipeId);
    Task UpdateAsync(UpdateIngredient model, IValidator<UpdateIngredient> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task UpdateAsync(UpdatePublicIngredient model, IValidator<UpdatePublicIngredient> validator, ISpan metricsSpan, CancellationToken cancellationToken);
    Task DeleteOrRemoveFromRecipesAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken);
}
