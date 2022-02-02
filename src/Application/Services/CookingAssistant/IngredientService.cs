using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Application.Contracts.CookingAssistant.Ingredients;
using Application.Contracts.CookingAssistant.Ingredients.Models;
using Application.Contracts.CookingAssistant.Recipes.Models;
using Domain.Entities.CookingAssistant;
using Domain.Entities.ToDoAssistant;

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

    public IEnumerable<IngredientDto> GetAll(int userId)
    {
        IEnumerable<Ingredient> ingredients = _ingredientsRepository.GetAll(userId);

        var result = ingredients.Select(x => _mapper.Map<IngredientDto>(x));

        return result;
    }

    public EditIngredient Get(int id, int userId)
    {
        Ingredient ingredient = _ingredientsRepository.Get(id, userId);

        var result = _mapper.Map<EditIngredient>(ingredient);

        return result;
    }

    public IEnumerable<IngredientSuggestion> GetSuggestions(int recipeId, int userId)
    {
        IEnumerable<Ingredient> ingredients = _ingredientsRepository.GetSuggestions(recipeId, userId);

        var result = ingredients.Select(x => _mapper.Map<IngredientSuggestion>(x));

        return result;
    }

    public IEnumerable<IngredientSuggestion> GetTaskSuggestions(int userId)
    {
        IEnumerable<ToDoTask> tasks = _ingredientsRepository.GetTaskSuggestions(userId);

        var result = tasks.Select(x => _mapper.Map<IngredientSuggestion>(x));
        result = result.OrderBy(x => x.Group);

        return result;
    }

    public IEnumerable<IngredientSuggestion> GetTaskSuggestions(int recipeId, int userId)
    {
        IEnumerable<Ingredient> ingredients = _ingredientsRepository.GetTaskSuggestions(recipeId, userId);

        var result = ingredients.Select(x => _mapper.Map<IngredientSuggestion>(x));

        return result;
    }

    public IEnumerable<IngredientReviewSuggestion> GetIngredientReviewSuggestions(int userId)
    {
        IEnumerable<Ingredient> ingredients = _ingredientsRepository.GetIngredientSuggestions(userId);

        var result = ingredients.Select(x => _mapper.Map<IngredientReviewSuggestion>(x));

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

        if (ingredient.TaskId.HasValue)
        {
            ingredient.Name = null;
        }
        else
        {
            ingredient.Name = ingredient.Name.Trim();
        }

        ingredient.ModifiedDate = DateTime.UtcNow;

        await _ingredientsRepository.UpdateAsync(ingredient);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        if (!Exists(id, userId))
        {
            throw new ValidationException("Unauthorized");
        }

        await _ingredientsRepository.DeleteAsync(id);
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