﻿using CookingAssistant.Application.Contracts.Ingredients.Models;
using FluentValidation;

namespace CookingAssistant.Application.Contracts.Ingredients;

public interface IIngredientService
{
    List<IngredientDto> GetUserAndUsedPublicIngredients(int userId);
    EditIngredient? GetForUpdate(int id, int userId);
    ViewIngredient? GetPublic(int id, int userId);
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
