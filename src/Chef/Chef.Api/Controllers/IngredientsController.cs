using Api.Common;
using Chef.Api.Models.Ingredients.Requests;
using Chef.Application.Contracts.Ingredients;
using Chef.Application.Contracts.Ingredients.Models;
using Core.Application.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Sentry;

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
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/ingredients",
            $"{nameof(IngredientsController)}.{nameof(GetAll)}",
            UserId
        );

        try
        {
            var ingredientDtos = _ingredientService.GetUserAndUsedPublicIngredients(UserId, tr);

            foreach (var ingredient in ingredientDtos.Where(x => x.IsPublic && !x.IsProduct))
            {
                ingredient.Name = _localizer[ingredient.Name];
            }

            return Ok(ingredientDtos);
        }
        catch
        {
            tr.Status = SpanStatus.InternalError;
            return StatusCode(500);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpGet("{id}/update")]
    public IActionResult GetForUpdate(int id)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/ingredients/{id}/update",
            $"{nameof(IngredientsController)}.{nameof(GetForUpdate)}",
            UserId
        );

        try
        {
            EditIngredient? editIngredientDto = _ingredientService.GetForUpdate(id, UserId, tr);
            if (editIngredientDto is null)
            {
                tr.Status = SpanStatus.NotFound;
                return NotFound();
            }

            return Ok(editIngredientDto);
        }
        catch
        {
            tr.Status = SpanStatus.InternalError;
            return StatusCode(500);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpGet("{id}/public")]
    public IActionResult GetPublic(int id)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/ingredients/{id}/public",
            $"{nameof(IngredientsController)}.{nameof(GetPublic)}",
            UserId
        );

        try
        {
            ViewIngredient? viewIngredientDto = _ingredientService.GetPublic(id, UserId, tr);
            if (viewIngredientDto is null)
            {
                tr.Status = SpanStatus.NotFound;
                return NotFound();
            }

            if (!viewIngredientDto.IsProduct)
            {
                viewIngredientDto.Name = _localizer[viewIngredientDto.Name];
            }

            return Ok(viewIngredientDto);
        }
        catch
        {
            tr.Status = SpanStatus.InternalError;
            return StatusCode(500);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpGet("user-suggestions")]
    public IActionResult GetUserSuggestions()
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/ingredients/user-suggestions",
            $"{nameof(IngredientsController)}.{nameof(GetUserSuggestions)}",
            UserId
        );

        try
        {
            var suggestionsDtos = _ingredientService.GetUserSuggestions(UserId, tr);
            return Ok(suggestionsDtos);
        }
        catch
        {
            tr.Status = SpanStatus.InternalError;
            return StatusCode(500);
        }
        finally
        {
            tr.Finish();
        }
    }

    [ResponseCache(Duration = 60 * 60 * 24)]
    [HttpGet("public-suggestions")]
    public IActionResult GetPublicSuggestions()
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/ingredients/public-suggestions",
            $"{nameof(IngredientsController)}.{nameof(GetPublicSuggestions)}",
            UserId
        );

        try
        {
            var suggestionsDto = _ingredientService.GetPublicSuggestions(tr);

            suggestionsDto.Uncategorized.ForEach(TranslateSuggestion);

            foreach (var category in suggestionsDto.Categories)
            {
                category.Name = _localizer[category.Name];
                category.Ingredients.ForEach(TranslateSuggestion);

                foreach (var subcategory in category.Subcategories)
                {
                    subcategory.Name = _localizer[subcategory.Name];
                    subcategory.Ingredients.ForEach(TranslateSuggestion);
                }
            }

            return Ok(suggestionsDto);
        }
        catch
        {
            tr.Status = SpanStatus.InternalError;
            return StatusCode(500);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpGet("task-suggestions")]
    public IActionResult GetTaskSuggestions()
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/ingredients/task-suggestions",
            $"{nameof(IngredientsController)}.{nameof(GetTaskSuggestions)}",
            UserId
        );

        try
        {
            IEnumerable<TaskSuggestion> taskSuggestions = _ingredientService.GetTaskSuggestions(UserId, tr);

            return Ok(taskSuggestions);
        }
        catch
        {
            tr.Status = SpanStatus.InternalError;
            return StatusCode(500);
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateIngredientRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/ingredients",
            $"{nameof(IngredientsController)}.{nameof(Update)}",
            UserId
        );

        try
        {
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
            await _ingredientService.UpdateAsync(model, _updateValidator, tr, cancellationToken);
        }
        catch
        {
            tr.Status = SpanStatus.InternalError;
            return StatusCode(500);
        }
        finally
        {
            tr.Finish();
        }

        return NoContent();
    }

    [HttpPut("public")]
    public async Task<IActionResult> UpdatePublic([FromBody] UpdatePublicIngredientRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/ingredients/public",
            $"{nameof(IngredientsController)}.{nameof(UpdatePublic)}",
            UserId
        );

        try
        {
            var model = new UpdatePublicIngredient
            {
                UserId = UserId,
                Id = request.Id,
                TaskId = request.TaskId,
            };
            await _ingredientService.UpdateAsync(model, _updatePublicValidator, tr, cancellationToken);
        }
        catch
        {
            tr.Status = SpanStatus.InternalError;
            return StatusCode(500);
        }
        finally
        {
            tr.Finish();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrRemoveFromRecipesAsync(int id, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/ingredients/{id}",
            $"{nameof(IngredientsController)}.{nameof(DeleteOrRemoveFromRecipesAsync)}",
            UserId
        );

        try
        {
            await _ingredientService.DeleteOrRemoveFromRecipesAsync(id, UserId, tr, cancellationToken);
        }
        catch
        {
            tr.Status = SpanStatus.InternalError;
            return StatusCode(500);
        }
        finally
        {
            tr.Finish();
        }

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
