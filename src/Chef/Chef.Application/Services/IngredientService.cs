using AutoMapper;
using Chef.Application.Contracts.Ingredients;
using Chef.Application.Contracts.Ingredients.Models;
using Chef.Application.Entities;
using Core.Application.Utils;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Sentry;

namespace Chef.Application.Services;

public class IngredientService : IIngredientService
{
    private readonly IIngredientsRepository _ingredientsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<IngredientService> _logger;

    public IngredientService(
        IIngredientsRepository? ingredientsRepository,
        IMapper? mapper,
        ILogger<IngredientService>? logger)
    {
        _ingredientsRepository = ArgValidator.NotNull(ingredientsRepository);
        _mapper = ArgValidator.NotNull(mapper);
        _logger = ArgValidator.NotNull(logger);
    }

    public IReadOnlyList<IngredientDto> GetUserAndUsedPublicIngredients(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(IngredientService)}.{nameof(GetUserAndUsedPublicIngredients)}");

        try
        {
            var ingredients = _ingredientsRepository.GetUserAndUsedPublicIngredients(userId, metric);

            var result = ingredients.Select(x => _mapper.Map<IngredientDto>(x)).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetUserAndUsedPublicIngredients)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public EditIngredient? GetForUpdate(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(IngredientService)}.{nameof(GetForUpdate)}");

        try
        {
            Ingredient? ingredient = _ingredientsRepository.GetForUpdate(id, userId, metric);

            var result = _mapper.Map<EditIngredient>(ingredient);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetForUpdate)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public ViewIngredient? GetPublic(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(IngredientService)}.{nameof(GetPublic)}");

        try
        {
            Ingredient? ingredient = _ingredientsRepository.GetPublic(id, userId, metric);

            var result = _mapper.Map<ViewIngredient>(ingredient);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetPublic)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public IReadOnlyList<IngredientSuggestion> GetUserSuggestions(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(IngredientService)}.{nameof(GetUserSuggestions)}");

        try
        {
            var ingredients = _ingredientsRepository.GetForSuggestions(userId, metric);

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
        finally
        {
            metric.Finish();
        }
    }

    public PublicIngredientSuggestions GetPublicSuggestions(ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(IngredientService)}.{nameof(GetPublicSuggestions)}");

        try
        {
            var result = new PublicIngredientSuggestions();

            var ingredients = _ingredientsRepository.GetForSuggestions(1, metric);
            var categories = _ingredientsRepository.GetIngredientCategories(metric);

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
        finally
        {
            metric.Finish();
        }
    }

    public IReadOnlyList<TaskSuggestion> GetTaskSuggestions(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(IngredientService)}.{nameof(GetTaskSuggestions)}");

        try
        {
            var tasks = _ingredientsRepository.GetTaskSuggestions(userId, metric);

            var result = tasks.Select(x => _mapper.Map<TaskSuggestion>(x))
                            .OrderBy(x => x.Group)
                            .ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetTaskSuggestions)}");
            throw;
        }
        finally
        {
            metric.Finish();
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

    public async Task UpdateAsync(UpdateIngredient model, IValidator<UpdateIngredient> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        var metric = metricsSpan.StartChild($"{nameof(IngredientService)}.{nameof(UpdateAsync)}");

        try
        {
            var ingredient = _mapper.Map<Ingredient>(model);
            ingredient.Name = ingredient.Name.Trim();
            ingredient.ModifiedDate = DateTime.UtcNow;

            await _ingredientsRepository.UpdateAsync(ingredient, metric, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task UpdateAsync(UpdatePublicIngredient model, IValidator<UpdatePublicIngredient> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        var metric = metricsSpan.StartChild($"{nameof(IngredientService)}.{nameof(UpdateAsync)}");

        try
        {
            await _ingredientsRepository.UpdatePublicAsync(model.Id, model.TaskId, model.UserId, DateTime.UtcNow, metric, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task DeleteOrRemoveFromRecipesAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(IngredientService)}.{nameof(DeleteOrRemoveFromRecipesAsync)}");

        try
        {
            var ingredient = _ingredientsRepository.Get(id, metric);
            if (ingredient.UserId == 1)
            {
                await _ingredientsRepository.RemoveFromRecipesAsync(id, userId, metric, cancellationToken);
            }
            else if (ingredient.UserId == userId)
            {
                await _ingredientsRepository.DeleteAsync(id, metric, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteOrRemoveFromRecipesAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    private static void DeriveAndSetUnit(IngredientSuggestion suggestion, List<RecipeIngredient> recipesIngredients)
    {
        if (!recipesIngredients.Any())
        {
            return;
        }

        var metricUnits = new HashSet<string> { "g", "ml", "tbsp", "tsp", "pinch" };
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

        var imperialUnits = new HashSet<string> { "oz", "cup", "tbsp", "tsp", "pinch" };
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
