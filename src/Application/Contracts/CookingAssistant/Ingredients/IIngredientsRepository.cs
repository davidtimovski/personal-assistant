﻿using System.Collections.Generic;
using System.Threading.Tasks;
using PersonalAssistant.Domain.Entities.CookingAssistant;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients
{
    public interface IIngredientsRepository
    {
        Task<IEnumerable<Ingredient>> GetAllAsync(int userId);
        Task<Ingredient> GetAsync(int id, int userId);
        Task<IEnumerable<Ingredient>> GetSuggestionsAsync(int recipeId, int userId);
        Task<IEnumerable<Ingredient>> GetTaskSuggestionsAsync(int userId);
        Task<IEnumerable<Ingredient>> GetTaskSuggestionsAsync(int recipeId, int userId);
        Task<IEnumerable<Ingredient>> GetIngredientSuggestionsAsync(int userId);
        Task<bool> ExistsAsync(int id, int userId);
        Task<bool> ExistsAsync(int id, string name, int userId);
        Task<bool> ExistsInRecipeAsync(int id, int recipeId);
        Task UpdateAsync(Ingredient ingredient);
        Task DeleteAsync(int id);
    }
}