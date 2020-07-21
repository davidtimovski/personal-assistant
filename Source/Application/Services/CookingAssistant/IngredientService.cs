using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using PersonalAssistant.Application.Contracts;
using PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients;
using PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients.Models;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using PersonalAssistant.Domain.Entities.CookingAssistant;

namespace PersonalAssistant.Application.Services.CookingAssistant
{
    public class IngredientService : IIngredientService
    {
        private readonly ITaskService _taskService;
        private readonly IIngredientsRepository _ingredientsRepository;
        private readonly IMapper _mapper;

        public IngredientService(
            ITaskService taskService,
            IIngredientsRepository ingredientsRepository,
            IMapper mapper)
        {
            _taskService = taskService;
            _ingredientsRepository = ingredientsRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<IngredientDto>> GetAllAsync(int userId)
        {
            IEnumerable<Ingredient> ingredients = await _ingredientsRepository.GetAllAsync(userId);

            var result = ingredients.Select(x => _mapper.Map<IngredientDto>(x));

            return result;
        }

        public async Task<EditIngredient> GetAsync(int id, int userId)
        {
            Ingredient ingredient = await _ingredientsRepository.GetAsync(id, userId);

            var result = _mapper.Map<EditIngredient>(ingredient);

            return result;
        }

        public async Task<IEnumerable<IngredientSuggestion>> GetSuggestionsAsync(int recipeId, int userId)
        {
            IEnumerable<Ingredient> ingredients = await _ingredientsRepository.GetSuggestionsAsync(recipeId, userId);

            var result = ingredients.Select(x => _mapper.Map<IngredientSuggestion>(x));

            return result;
        }

        public async Task<IEnumerable<IngredientSuggestion>> GetTaskSuggestionsAsync(int userId)
        {
            IEnumerable<Ingredient> ingredients = await _ingredientsRepository.GetTaskSuggestionsAsync(userId);

            var result = ingredients.Select(x => _mapper.Map<IngredientSuggestion>(x));
            result = result.OrderBy(x => x.Group);

            return result;
        }

        public async Task<IEnumerable<IngredientSuggestion>> GetTaskSuggestionsAsync(int recipeId, int userId)
        {
            IEnumerable<Ingredient> ingredients = await _ingredientsRepository.GetTaskSuggestionsAsync(recipeId, userId);

            var result = ingredients.Select(x => _mapper.Map<IngredientSuggestion>(x));

            return result;
        }

        public async Task<IEnumerable<IngredientSuggestion>> GetIngredientSuggestionsAsync(int userId)
        {
            IEnumerable<Ingredient> ingredients = await _ingredientsRepository.GetIngredientSuggestionsAsync(userId);

            var result = ingredients.Select(x => _mapper.Map<IngredientSuggestion>(x));

            return result;
        }

        public async Task<IEnumerable<IngredientReviewSuggestion>> GetIngredientReviewSuggestionsAsync(int userId)
        {
            IEnumerable<Ingredient> ingredients = await _ingredientsRepository.GetIngredientSuggestionsAsync(userId);

            var result = ingredients.Select(x => _mapper.Map<IngredientReviewSuggestion>(x));

            return result;
        }

        public Task<bool> ExistsAsync(int id, int userId)
        {
            return _ingredientsRepository.ExistsAsync(id, userId);
        }

        public Task<bool> ExistsAsync(int id, string name, int userId)
        {
            return _ingredientsRepository.ExistsAsync(id, name.Trim(), userId);
        }

        public Task<bool> ExistsInRecipeAsync(int id, int recipeId)
        {
            return _ingredientsRepository.ExistsInRecipeAsync(id, recipeId);
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

            ingredient.ModifiedDate = DateTime.Now;

            await _ingredientsRepository.UpdateAsync(ingredient);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            if (!await ExistsAsync(id, userId))
            {
                throw new ValidationException("Unauthorized");
            }

            await _ingredientsRepository.DeleteAsync(id);
        }

        private void ValidateAndThrow<T>(T model, IValidator<T> validator)
        {
            ValidationResult result = validator.Validate(model);
            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }
    }
}
