using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Contracts.CookingAssistant.Ingredients;
using Application.Contracts.CookingAssistant.Ingredients.Models;
using FluentValidation;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.CookingAssistant;

[Authorize]
[EnableCors("AllowCookingAssistant")]
[Route("api/[controller]")]
public class IngredientsController : Controller
{
    private readonly IIngredientService _ingredientService;
    private readonly IValidator<UpdateIngredient> _updateValidator;

    public IngredientsController(
        IIngredientService ingredientService,
        IValidator<UpdateIngredient> updateValidator)
    {
        _ingredientService = ingredientService;
        _updateValidator = updateValidator;
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

        IEnumerable<IngredientDto> ingredientDtos = _ingredientService.GetAll(userId);

        return Ok(ingredientDtos);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
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

        EditIngredient editIngredientDto = _ingredientService.Get(id, userId);
        if (editIngredientDto == null)
        {
            return NotFound();
        }

        return Ok(editIngredientDto);
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
        int userId;
        try
        {
            userId = IdentityHelper.GetUserId(User);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }

        var suggestionsDto = _ingredientService.GetPublicSuggestions();

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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
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

        await _ingredientService.DeleteAsync(id, userId);

        return NoContent();
    }
}