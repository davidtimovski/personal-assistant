using System;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles;
using PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles.Models;
using PersonalAssistant.Infrastructure.Identity;

namespace Api.Controllers.CookingAssistant
{
    [Authorize]
    [EnableCors("AllowCookingAssistant")]
    [Route("api/[controller]")]
    public class DietaryProfilesController : Controller
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
        public async Task<IActionResult> Get()
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

            EditDietaryProfile dto = await _dietaryProfileService.GetAsync(userId);

            return Ok(dto);
        }

        [HttpPost]
        public IActionResult GetDailyIntake([FromBody] GetRecommendedDailyIntake dto)
        {
            if (dto == null)
            {
                return BadRequest();
            }

            int userId;
            try
            {
                userId = IdentityHelper.GetUserId(User);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            RecommendedDailyIntake recommended = _dietaryProfileService.GetRecommendedDailyIntake(dto, _getRecommendedDailyIntakeValidator);

            return Ok(recommended);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateDietaryProfile dto)
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

            await _dietaryProfileService.UpdateAsync(dto, _updateDietaryProfileValidator);

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
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

            await _dietaryProfileService.DeleteAsync(userId);

            return NoContent();
        }
    }
}