using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models.CookingAssistant.Ingredients;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients;
using PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients.Models;
using PersonalAssistant.Infrastructure.Identity;

namespace Api.Controllers.CookingAssistant
{
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
        public async Task<IActionResult> GetAll()
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

            IEnumerable<IngredientDto> ingredientDtos = await _ingredientService.GetAllAsync(userId);

            return Ok(ingredientDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
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

            EditIngredient editIngredientDto = await _ingredientService.GetAsync(id, userId);
            if (editIngredientDto == null)
            {
                return NotFound();
            }

            return Ok(editIngredientDto);
        }

        [HttpGet("suggestions/{recipeId}")]
        public async Task<IActionResult> GetSuggestions(int recipeId)
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

            var ingredientSuggestionsVm = new IngredientSuggestionsVm
            {
                Suggestions = await _ingredientService.GetSuggestionsAsync(recipeId, userId),
                TaskSuggestions = await _ingredientService.GetTaskSuggestionsAsync(recipeId, userId)
            };

            return Ok(ingredientSuggestionsVm);
        }

        [HttpGet("task-suggestions")]
        public async Task<IActionResult> GetTaskSuggestions()
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

            IEnumerable<IngredientSuggestion> taskSuggestions = await _ingredientService.GetTaskSuggestionsAsync(userId);

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
}