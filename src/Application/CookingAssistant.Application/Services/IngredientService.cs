using AutoMapper;
using CookingAssistant.Application.Contracts.Ingredients;
using CookingAssistant.Application.Contracts.Ingredients.Models;
using CookingAssistant.Application.Entities;
using Core.Application.Utils;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CookingAssistant.Application.Services;

public class IngredientService : IIngredientService
{
    private readonly IIngredientsRepository _ingredientsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<IngredientService> _logger;

    public IngredientService(
        IIngredientsRepository ingredientsRepository,
        IMapper mapper,
        ILogger<IngredientService> logger)
    {
        _ingredientsRepository = ingredientsRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public List<IngredientDto> GetUserAndUsedPublicIngredients(int userId)
    {
        try
        {
            IEnumerable<Ingredient> ingredients = _ingredientsRepository.GetUserAndUsedPublicIngredients(userId);

            var result = ingredients.Select(x => _mapper.Map<IngredientDto>(x)).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetUserAndUsedPublicIngredients)}");
            throw;
        }
    }

    public EditIngredient GetForUpdate(int id, int userId)
    {
        try
        {
            Ingredient ingredient = _ingredientsRepository.GetForUpdate(id, userId);

            var result = _mapper.Map<EditIngredient>(ingredient);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetForUpdate)}");
            throw;
        }
    }

    public ViewIngredient GetPublic(int id, int userId)
    {
        try
        {
            Ingredient ingredient = _ingredientsRepository.GetPublic(id, userId);

            var result = _mapper.Map<ViewIngredient>(ingredient);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetPublic)}");
            throw;
        }
    }

    public IEnumerable<IngredientSuggestion> GetUserSuggestions(int userId)
    {
        try
        {
            IEnumerable<Ingredient> ingredients = _ingredientsRepository.GetForSuggestions(userId);

            var suggestions = ingredients.Select(x => _mapper.Map<IngredientSuggestion>(x)).ToList();

            // Derive units
            var recipeIngredientsLookup = ingredients.ToDictionary(x => x.Id, x => x.RecipesIngredients);
            foreach (var suggestion in suggestions)
            {
                DeriveAndSetUnit(suggestion, recipeIngredientsLookup[suggestion.Id]);
            }

            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetUserSuggestions)}");
            throw;
        }
    }

    public PublicIngredientSuggestions GetPublicSuggestions()
    {
        try
        {
            var result = new PublicIngredientSuggestions();

            IEnumerable<Ingredient> ingredients = _ingredientsRepository.GetForSuggestions(1);
            IEnumerable<IngredientCategory> categories = _ingredientsRepository.GetIngredientCategories();

            List<IngredientSuggestion> suggestions = ingredients.Select(x => _mapper.Map<IngredientSuggestion>(x)).ToList();

            // Create public ingredient hierarchy, derive units
            var recipeIngredientsLookup = ingredients.ToDictionary(x => x.Id, x => x.RecipesIngredients);
            foreach (var suggestion in suggestions)
            {
                suggestion.Children = suggestions.Where(x => x.ParentId == suggestion.Id).OrderBy(x => x.Name).ToList();

                DeriveAndSetUnit(suggestion, recipeIngredientsLookup[suggestion.Id]);
            }

            result.Uncategorized = suggestions.Where(x => !x.CategoryId.HasValue && !x.ParentId.HasValue).OrderBy(x => x.Name).ToList();

            var categorizedPublicSuggestions = suggestions.Where(x => x.CategoryId.HasValue);
            result.Categories = categories.Select(x => _mapper.Map<IngredientCategoryDto>(x)).ToList();

            foreach (var category in result.Categories)
            {
                category.Ingredients = suggestions.Where(x => x.CategoryId == category.Id && !x.ParentId.HasValue).OrderBy(x => x.Name).ToList();
                category.Subcategories = result.Categories.Where(x => x.ParentId == category.Id).OrderBy(x => x.Id).ToList();

                foreach (var subcategory in category.Subcategories)
                {
                    subcategory.Ingredients = suggestions.Where(x => x.CategoryId == category.Id && !x.ParentId.HasValue).OrderBy(x => x.Name).ToList();
                }
            }
            result.Categories.RemoveAll(x => x.ParentId.HasValue);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetPublicSuggestions)}");
            throw;
        }
    }

    public IEnumerable<TaskSuggestion> GetTaskSuggestions(int userId)
    {
        try
        {
            IEnumerable<ToDoTask> tasks = _ingredientsRepository.GetTaskSuggestions(userId);

            var result = tasks.Select(x => _mapper.Map<TaskSuggestion>(x));
            result = result.OrderBy(x => x.Group);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetTaskSuggestions)}");
            throw;
        }
    }

    public bool Exists(int id, int userId)
    {
        try
        {
            return _ingredientsRepository.Exists(id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Exists)}");
            throw;
        }
    }

    public bool Exists(int id, string name, int userId)
    {
        try
        {
            return _ingredientsRepository.Exists(id, name.Trim(), userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Exists)}");
            throw;
        }
    }

    public bool ExistsInRecipe(int id, int recipeId)
    {
        try
        {
            return _ingredientsRepository.ExistsInRecipe(id, recipeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(ExistsInRecipe)}");
            throw;
        }
    }

    public async Task UpdateAsync(UpdateIngredient model, IValidator<UpdateIngredient> validator)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        try
        {
            var ingredient = _mapper.Map<Ingredient>(model);
            ingredient.Name = ingredient.Name.Trim();
            ingredient.ModifiedDate = DateTime.UtcNow;

            await _ingredientsRepository.UpdateAsync(ingredient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateAsync)}");
            throw;
        }
    }

    public async Task UpdateAsync(UpdatePublicIngredient model, IValidator<UpdatePublicIngredient> validator)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        try
        {
            await _ingredientsRepository.UpdatePublicAsync(model.Id, model.TaskId, model.UserId, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateAsync)}");
            throw;
        }
    }

    public async Task DeleteOrRemoveFromRecipesAsync(int id, int userId)
    {
        try
        {
            var ingredient = _ingredientsRepository.Get(id);
            if (ingredient.UserId == 1)
            {
                await _ingredientsRepository.RemoveFromRecipesAsync(id, userId);
            }
            else if (ingredient.UserId == userId)
            {
                await _ingredientsRepository.DeleteAsync(id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteOrRemoveFromRecipesAsync)}");
            throw;
        }
    }

    private static void DeriveAndSetUnit(IngredientSuggestion suggestion, List<RecipeIngredient> recipesIngredients)
    {
        if (!recipesIngredients.Any())
        {
            return;
        }

        var metricUnits = new string[] { "g", "ml", "tbsp", "tsp", "pinch" };
        var metric = recipesIngredients.Where(x => metricUnits.Contains(x.Unit)).ToList();
        if (metric.Any())
        {
            var metricGrouped = metric.GroupBy(x => x.Unit);
            if (metricGrouped.Count() > 1)
            {
                metricGrouped = metricGrouped.OrderByDescending(x => x.Count());
            }
            suggestion.Unit = metricGrouped.First().Key;
        }

        var imperialUnits = new string[] { "oz", "cup", "tbsp", "tsp", "pinch" };
        var imperial = recipesIngredients.Where(x => imperialUnits.Contains(x.Unit)).ToList();
        if (imperial.Any())
        {
            var imperialGrouped = imperial.GroupBy(x => x.Unit);
            if (imperialGrouped.Count() > 1)
            {
                imperialGrouped = imperialGrouped.OrderByDescending(x => x.Count());
            }
            suggestion.UnitImperial = imperialGrouped.First().Key;
        }
    }
}
