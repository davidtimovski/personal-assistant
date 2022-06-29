using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.CookingAssistant.Ingredients;
using Application.Contracts.CookingAssistant.Ingredients.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Api.Controllers.CookingAssistant;

[Authorize]
[EnableCors("AllowCookingAssistant")]
[Route("api/[controller]")]
public class IngredientsController : BaseController
{
    private readonly IIngredientService _ingredientService;
    private readonly IStringLocalizer<IngredientsController> _localizer;
    private readonly IValidator<UpdateIngredient> _updateValidator;
    private readonly IValidator<UpdatePublicIngredient> _updatePublicValidator;

    public IngredientsController(
        IIngredientService ingredientService,
        IStringLocalizer<IngredientsController> localizer,
        IValidator<UpdateIngredient> updateValidator,
        IValidator<UpdatePublicIngredient> updatePublicValidator)
    {
        _ingredientService = ingredientService;
        _localizer = localizer;
        _updateValidator = updateValidator;
        _updatePublicValidator = updatePublicValidator;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var ingredientDtos = _ingredientService.GetUserAndUsedPublicIngredients(CurrentUserId);

        foreach (var ingredient in ingredientDtos.Where(x => x.IsPublic && !x.IsProduct))
        {
            ingredient.Name = _localizer[ingredient.Name];
        }

        return Ok(ingredientDtos);
    }

    [HttpGet("{id}/update")]
    public IActionResult GetForUpdate(int id)
    {
        EditIngredient editIngredientDto = _ingredientService.GetForUpdate(id, CurrentUserId);
        if (editIngredientDto == null)
        {
            return NotFound();
        }

        return Ok(editIngredientDto);
    }

    [HttpGet("{id}/public")]
    public IActionResult GetPublic(int id)
    {
        ViewIngredient viewIngredientDto = _ingredientService.GetPublic(id, CurrentUserId);
        if (viewIngredientDto == null)
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
        var suggestionsDtos = _ingredientService.GetUserSuggestions(CurrentUserId);

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
        IEnumerable<TaskSuggestion> taskSuggestions = _ingredientService.GetTaskSuggestions(CurrentUserId);

        return Ok(taskSuggestions);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateIngredient dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        dto.UserId = CurrentUserId;

        await _ingredientService.UpdateAsync(dto, _updateValidator);

        return NoContent();
    }

    [HttpPut("public")]
    public async Task<IActionResult> UpdatePublic([FromBody] UpdatePublicIngredient dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        dto.UserId = CurrentUserId;

        await _ingredientService.UpdateAsync(dto, _updatePublicValidator);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrRemoveFromRecipesAsync(int id)
    {
        await _ingredientService.DeleteOrRemoveFromRecipesAsync(id, CurrentUserId);

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
