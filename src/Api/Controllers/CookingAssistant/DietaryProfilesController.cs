﻿using System.Threading.Tasks;
using Application.Contracts.CookingAssistant.DietaryProfiles;
using Application.Contracts.CookingAssistant.DietaryProfiles.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.CookingAssistant;

[Authorize]
[EnableCors("AllowCookingAssistant")]
[Route("api/[controller]")]
public class DietaryProfilesController : BaseController
{
    private readonly IDietaryProfileService _dietaryProfileService;
    private readonly IValidator<GetRecommendedDailyIntake> _getRecommendedDailyIntakeValidator;
    private readonly IValidator<UpdateDietaryProfile> _updateDietaryProfileValidator;

    public DietaryProfilesController(
        IDietaryProfileService dietaryProfileService,
        IValidator<GetRecommendedDailyIntake> getRecommendedDailyIntakeValidator,
        IValidator<UpdateDietaryProfile> updateDietaryProfileValidator)
    {
        _dietaryProfileService = dietaryProfileService;
        _getRecommendedDailyIntakeValidator = getRecommendedDailyIntakeValidator;
        _updateDietaryProfileValidator = updateDietaryProfileValidator;
    }

    [HttpGet]
    public IActionResult Get()
    {
        EditDietaryProfile dto = _dietaryProfileService.Get(CurrentUserId);

        return Ok(dto);
    }

    [HttpPost]
    public IActionResult GetDailyIntake([FromBody] GetRecommendedDailyIntake dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        RecommendedDailyIntake recommended = _dietaryProfileService.GetRecommendedDailyIntake(dto, _getRecommendedDailyIntakeValidator);

        return Ok(recommended);
    }

    [HttpPut]
    public async Task<IActionResult> CreateOrUpdate([FromBody] UpdateDietaryProfile dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        dto.UserId = CurrentUserId;

        await _dietaryProfileService.CreateOrUpdateAsync(dto, _updateDietaryProfileValidator);

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        await _dietaryProfileService.DeleteAsync(CurrentUserId);

        return NoContent();
    }
}
