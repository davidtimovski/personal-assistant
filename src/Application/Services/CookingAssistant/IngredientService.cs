using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.CookingAssistant.Ingredients;
using Application.Contracts.CookingAssistant.Ingredients.Models;
using AutoMapper;
using Domain.Entities.CookingAssistant;
using Domain.Entities.ToDoAssistant;
using FluentValidation;
using FluentValidation.Results;

namespace Application.Services.CookingAssistant;

public class IngredientService : IIngredientService
{
    private readonly IIngredientsRepository _ingredientsRepository;
    private readonly IMapper _mapper;

    public IngredientService(
        IIngredientsRepository ingredientsRepository,
        IMapper mapper)
    {
        _ingredientsRepository = ingredientsRepository;
        _mapper = mapper;
    }

    public List<IngredientDto> GetUserAndUsedPublicIngredients(int userId)
    {
        IEnumerable<Ingredient> ingredients = _ingredientsRepository.GetUserAndUsedPublicIngredients(userId);

        var result = ingredients.Select(x => _mapper.Map<IngredientDto>(x)).ToList();

        return result;
    }

    public EditIngredient GetForUpdate(int id, int userId)
    {
        Ingredient ingredient = _ingredientsRepository.GetForUpdate(id, userId);

        var result = _mapper.Map<EditIngredient>(ingredient);

        return result;
    }

    public ViewIngredient GetPublic(int id, int userId)
    {
        Ingredient ingredient = _ingredientsRepository.GetPublic(id, userId);

        var result = _mapper.Map<ViewIngredient>(ingredient);

        return result;
    }

    public IEnumerable<IngredientSuggestion> GetUserSuggestions(int userId)
    {
        IEnumerable<Ingredient> ingredients = _ingredientsRepository.GetAll(userId);
        return ingredients.Select(x => _mapper.Map<IngredientSuggestion>(x));
    }

    public PublicIngredientSuggestions GetPublicSuggestions()
    {
        var result = new PublicIngredientSuggestions();

        IEnumerable<Ingredient> ingredients = _ingredientsRepository.GetAll(1);
        IEnumerable<IngredientCategory> categories = _ingredientsRepository.GetIngredientCategories();

        List<IngredientSuggestion> publicSuggestions = ingredients.Select(x => _mapper.Map<IngredientSuggestion>(x)).ToList();
        
        // Create public ingredient hierarchy
        foreach (var ingredient in publicSuggestions)
        {
            ingredient.Children = publicSuggestions.Where(x => x.ParentId == ingredient.Id).ToList();
        }

        result.Uncategorized = publicSuggestions.Where(x => !x.CategoryId.HasValue && !x.ParentId.HasValue).ToList();

        var categorizedPublicSuggestions = publicSuggestions.Where(x => x.CategoryId.HasValue);
        result.Categories = categories.Select(x => _mapper.Map<IngredientCategoryDto>(x)).ToList();

        foreach (var category in result.Categories)
        {
            category.Ingredients = publicSuggestions.Where(x => x.CategoryId == category.Id && !x.ParentId.HasValue).ToList();
            category.Subcategories = result.Categories.Where(x => x.ParentId == category.Id).ToList();

            foreach (var subcategory in category.Subcategories)
            {
                subcategory.Ingredients = publicSuggestions.Where(x => x.CategoryId == category.Id && !x.ParentId.HasValue).ToList();
            }
        }
        result.Categories.RemoveAll(x => x.ParentId.HasValue);

        return result;
    }

    public IEnumerable<TaskSuggestion> GetTaskSuggestions(int userId)
    {
        IEnumerable<ToDoTask> tasks = _ingredientsRepository.GetTaskSuggestions(userId);

        var result = tasks.Select(x => _mapper.Map<TaskSuggestion>(x));
        result = result.OrderBy(x => x.Group);

        return result;
    }

    public bool Exists(int id, int userId)
    {
        return _ingredientsRepository.Exists(id, userId);
    }

    public bool Exists(int id, string name, int userId)
    {
        return _ingredientsRepository.Exists(id, name.Trim(), userId);
    }

    public bool ExistsInRecipe(int id, int recipeId)
    {
        return _ingredientsRepository.ExistsInRecipe(id, recipeId);
    }

    public async Task UpdateAsync(UpdateIngredient model, IValidator<UpdateIngredient> validator)
    {
        ValidateAndThrow(model, validator);

        var ingredient = _mapper.Map<Ingredient>(model);
        ingredient.Name = ingredient.Name.Trim();
        ingredient.ModifiedDate = DateTime.UtcNow;

        await _ingredientsRepository.UpdateAsync(ingredient);
    }

    public async Task UpdateAsync(UpdatePublicIngredient model, IValidator<UpdatePublicIngredient> validator)
    {
        ValidateAndThrow(model, validator);
        await _ingredientsRepository.UpdatePublicAsync(model.Id, model.TaskId, model.UserId, DateTime.UtcNow);
    }

    public async Task DeleteOrRemoveFromRecipesAsync(int id, int userId)
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

    private static void ValidateAndThrow<T>(T model, IValidator<T> validator)
    {
        ValidationResult result = validator.Validate(model);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}