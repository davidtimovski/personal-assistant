using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Contracts.CookingAssistant.Ingredients.Models;
using FluentValidation;

namespace Application.Contracts.CookingAssistant.Ingredients;

public interface IIngredientService
{
    List<IngredientDto> GetUserAndUsedPublicIngredients(int userId);
    EditIngredient GetForUpdate(int id, int userId);
    ViewIngredient GetPublic(int id, int userId);
    IEnumerable<IngredientSuggestion> GetUserSuggestions(int userId);
    PublicIngredientSuggestions GetPublicSuggestions();
    IEnumerable<TaskSuggestion> GetTaskSuggestions(int userId);
    bool Exists(int id, int userId);
    bool Exists(int id, string name, int userId);
    bool ExistsInRecipe(int id, int recipeId);
    Task UpdateAsync(UpdateIngredient model, IValidator<UpdateIngredient> validator);
    Task UpdateAsync(UpdatePublicIngredient model, IValidator<UpdatePublicIngredient> validator);
    Task DeleteOrRemoveFromRecipesAsync(int id, int userId);
}
