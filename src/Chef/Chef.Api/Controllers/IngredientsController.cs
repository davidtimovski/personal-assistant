using Chef.Api.Models.Ingredients.Requests;
using Chef.Application.Contracts.Ingredients;
using Chef.Application.Contracts.Ingredients.Models;
using Core.Application.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Chef.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class IngredientsController : BaseController
{
    private readonly IIngredientService _ingredientService;
    private readonly IStringLocalizer<IngredientsController> _localizer;
    private readonly IValidator<UpdateIngredient> _updateValidator;
    private readonly IValidator<UpdatePublicIngredient> _updatePublicValidator;

    public IngredientsController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        IIngredientService ingredientService,
        IStringLocalizer<IngredientsController> localizer,
        IValidator<UpdateIngredient> updateValidator,
        IValidator<UpdatePublicIngredient> updatePublicValidator) : base(userIdLookup, usersRepository)
    {
        _ingredientService = ingredientService;
        _localizer = localizer;
        _updateValidator = updateValidator;
        _updatePublicValidator = updatePublicValidator;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var ingredientDtos = _ingredientService.GetUserAndUsedPublicIngredients(UserId);

        foreach (var ingredient in ingredientDtos.Where(x => x.IsPublic && !x.IsProduct))
        {
            ingredient.Name = _localizer[ingredient.Name];
        }

        return Ok(ingredientDtos);
    }

    [HttpGet("{id}/update")]
    public IActionResult GetForUpdate(int id)
    {
        EditIngredient? editIngredientDto = _ingredientService.GetForUpdate(id, UserId);
        if (editIngredientDto is null)
        {
            return NotFound();
        }

        return Ok(editIngredientDto);
    }

    [HttpGet("{id}/public")]
    public IActionResult GetPublic(int id)
    {
        ViewIngredient? viewIngredientDto = _ingredientService.GetPublic(id, UserId);
        if (viewIngredientDto is null)
        {
            return NotFound();
        }

        if (!viewIngredientDto.IsProduct)
        {
            viewIngredientDto.Name = _localizer[viewIngredientDto.Name];
        }

        return Ok(viewIngredientDto);
    }

    [HttpGet("user-suggestions")]
    public IActionResult GetUserSuggestions()
    {
        var suggestionsDtos = _ingredientService.GetUserSuggestions(UserId);

        return Ok(suggestionsDtos);
    }

    [ResponseCache(Duration = 60 * 60 * 24)]
    [HttpGet("public-suggestions")]
    public IActionResult GetPublicSuggestions()
    {
        var suggestionsDto = _ingredientService.GetPublicSuggestions();

        suggestionsDto.Uncategorized.ForEach(suggestion =>
        {
            TranslateSuggestion(suggestion);
        });

        foreach (var category in suggestionsDto.Categories)
        {
            category.Name = _localizer[category.Name];
            category.Ingredients.ForEach(suggestion =>
            {
                TranslateSuggestion(suggestion);
            });

            foreach (var subcategory in category.Subcategories)
            {
                subcategory.Name = _localizer[subcategory.Name];
                subcategory.Ingredients.ForEach(suggestion =>
                {
                    TranslateSuggestion(suggestion);
                });
            }
        }

        return Ok(suggestionsDto);
    }

    [HttpGet("task-suggestions")]
    public IActionResult GetTaskSuggestions()
    {
        IEnumerable<TaskSuggestion> taskSuggestions = _ingredientService.GetTaskSuggestions(UserId);

        return Ok(taskSuggestions);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateIngredientRequest request)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var model = new UpdateIngredient
        {
            UserId = UserId,
            Id = request.Id,
            TaskId = request.TaskId,
            Name = request.Name,
            NutritionData = new Application.Contracts.Ingredients.Models.UpdateIngredientNutritionData
            {
                IsSet = request.NutritionData.IsSet,
                ServingSize = request.NutritionData.ServingSize,
                ServingSizeIsOneUnit = request.NutritionData.ServingSizeIsOneUnit,
                Calories = request.NutritionData.Calories,
                Fat = request.NutritionData.Fat,
                SaturatedFat = request.NutritionData.SaturatedFat,
                Carbohydrate = request.NutritionData.Carbohydrate,
                Sugars = request.NutritionData.Sugars,
                AddedSugars = request.NutritionData.AddedSugars,
                Fiber = request.NutritionData.Fiber,
                Protein = request.NutritionData.Protein,
                Sodium = request.NutritionData.Sodium,
                Cholesterol = request.NutritionData.Cholesterol,
                VitaminA = request.NutritionData.VitaminA,
                VitaminC = request.NutritionData.VitaminC,
                VitaminD = request.NutritionData.VitaminD,
                Calcium = request.NutritionData.Calcium,
                Iron = request.NutritionData.Iron,
                Potassium = request.NutritionData.Potassium,
                Magnesium = request.NutritionData.Magnesium,
            },
            PriceData = new Application.Contracts.Ingredients.Models.UpdateIngredientPriceData
            {
                IsSet = request.PriceData.IsSet,
                ProductSize = request.PriceData.ProductSize,
                ProductSizeIsOneUnit = request.PriceData.ProductSizeIsOneUnit,
                Price = request.PriceData.Price,
                Currency = request.PriceData.Currency,
            },
        };
        await _ingredientService.UpdateAsync(model, _updateValidator);

        return NoContent();
    }

    [HttpPut("public")]
    public async Task<IActionResult> UpdatePublic([FromBody] UpdatePublicIngredientRequest request)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var model = new UpdatePublicIngredient
        {
            UserId = UserId,
            Id = request.Id,
            TaskId = request.TaskId,
        };
        await _ingredientService.UpdateAsync(model, _updatePublicValidator);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrRemoveFromRecipesAsync(int id)
    {
        await _ingredientService.DeleteOrRemoveFromRecipesAsync(id, UserId);

        return NoContent();
    }

    private void TranslateSuggestion(IngredientSuggestion suggestion)
    {
        if (suggestion.IsProduct)
        {
            return;
        }

        suggestion.Name = _localizer[suggestion.Name];

        foreach (var child in suggestion.Children)
        {
            TranslateSuggestion(child);
        }
    }
}
