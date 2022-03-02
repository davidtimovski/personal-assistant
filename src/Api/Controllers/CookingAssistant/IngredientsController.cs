using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.CookingAssistant.Ingredients;
using Application.Contracts.CookingAssistant.Ingredients.Models;
using FluentValidation;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Api.Controllers.CookingAssistant;

[Authorize]
[EnableCors("AllowCookingAssistant")]
[Route("api/[controller]")]
public class IngredientsController : Controller
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
        int userId;
        try
        {
            userId = IdentityHelper.GetUserId(User);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }

        var ingredientDtos = _ingredientService.GetUserAndUsedPublicIngredients(userId);

        foreach (var ingredient in ingredientDtos.Where(x => x.IsPublic))
        {
            ingredient.Name = _localizer[ingredient.Name];
        }

        return Ok(ingredientDtos);
    }

    [HttpGet("{id}/update")]
    public IActionResult GetForUpdate(int id)
    {
        int userId;
        try
        {
            userId = IdentityHelper.GetUserId(User);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }

        EditIngredient editIngredientDto = _ingredientService.GetForUpdate(id, userId);
        if (editIngredientDto == null)
        {
            return NotFound();
        }

        return Ok(editIngredientDto);
    }

    [HttpGet("{id}/public")]
    public IActionResult GetPublic(int id)
    {
        int userId;
        try
        {
            userId = IdentityHelper.GetUserId(User);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }

        ViewIngredient viewIngredientDto = _ingredientService.GetPublic(id, userId);
        if (viewIngredientDto == null)
        {
            return NotFound();
        }

        viewIngredientDto.Name = _localizer[viewIngredientDto.Name];

        return Ok(viewIngredientDto);
    }

    [HttpGet("user-suggestions")]
    public IActionResult GetUserSuggestions()
    {
        int userId;
        try
        {
            userId = IdentityHelper.GetUserId(User);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }

        var suggestionsDtos = _ingredientService.GetUserSuggestions(userId);

        return Ok(suggestionsDtos);
    }

    // TODO: Use Output caching in future (possibly .NET 7)
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
        int userId;
        try
        {
            userId = IdentityHelper.GetUserId(User);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }

        IEnumerable<TaskSuggestion> taskSuggestions = _ingredientService.GetTaskSuggestions(userId);

        return Ok(taskSuggestions);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateIngredient dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        try
        {
            dto.UserId = IdentityHelper.GetUserId(User);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }

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

        try
        {
            dto.UserId = IdentityHelper.GetUserId(User);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }

        await _ingredientService.UpdateAsync(dto, _updatePublicValidator);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrRemoveFromRecipesAsync(int id)
    {
        int userId;
        try
        {
            userId = IdentityHelper.GetUserId(User);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }

        await _ingredientService.DeleteOrRemoveFromRecipesAsync(id, userId);

        return NoContent();
    }

    private void TranslateSuggestion(IngredientSuggestion suggestion)
    {
        suggestion.Name = _localizer[suggestion.Name];

        foreach (var child in suggestion.Children)
        {
            TranslateSuggestion(child);
        }
    }
}