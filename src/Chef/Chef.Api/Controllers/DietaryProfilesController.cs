using Chef.Api.Models.DietaryProfiles.Requests;
using Chef.Application.Contracts.DietaryProfiles;
using Chef.Application.Contracts.DietaryProfiles.Models;
using Core.Application.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chef.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class DietaryProfilesController : BaseController
{
    private readonly IDietaryProfileService _dietaryProfileService;
    private readonly IValidator<GetRecommendedDailyIntake> _getRecommendedDailyIntakeValidator;
    private readonly IValidator<UpdateDietaryProfile> _updateDietaryProfileValidator;

    public DietaryProfilesController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        IDietaryProfileService dietaryProfileService,
        IValidator<GetRecommendedDailyIntake> getRecommendedDailyIntakeValidator,
        IValidator<UpdateDietaryProfile> updateDietaryProfileValidator) : base(userIdLookup, usersRepository)
    {
        _dietaryProfileService = dietaryProfileService;
        _getRecommendedDailyIntakeValidator = getRecommendedDailyIntakeValidator;
        _updateDietaryProfileValidator = updateDietaryProfileValidator;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var dietaryProfile = _dietaryProfileService.Get(UserId);
        return Ok(dietaryProfile);
    }

    [HttpPost]
    public IActionResult GetDailyIntake([FromBody] GetRecommendedDailyIntakeRequest request)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var model = new GetRecommendedDailyIntake
        {
            Birthday = request.Birthday,
            Gender = request.Gender,
            HeightCm = request.HeightCm,
            HeightFeet = request.HeightFeet,
            HeightInches = request.HeightInches,
            WeightKg = request.WeightKg,
            WeightLbs = request.WeightLbs,
            ActivityLevel = request.ActivityLevel,
            Goal = request.Goal
        };
        RecommendedDailyIntake recommended = _dietaryProfileService.GetRecommendedDailyIntake(model, _getRecommendedDailyIntakeValidator);

        return Ok(recommended);
    }

    [HttpPut]
    public async Task<IActionResult> CreateOrUpdate([FromBody] UpdateDietaryProfileRequest request)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var model = new UpdateDietaryProfile
        {
            UserId = UserId,
            Birthday = request.Birthday,
            Gender = request.Gender,
            HeightCm = request.HeightCm,
            HeightFeet = request.HeightFeet,
            HeightInches = request.HeightInches,
            WeightKg = request.WeightKg,
            WeightLbs = request.WeightLbs,
            ActivityLevel = request.ActivityLevel,
            Goal = request.Goal,
            CustomCalories = request.CustomCalories,
            TrackCalories = request.TrackCalories,
            CustomSaturatedFat = request.CustomSaturatedFat,
            TrackSaturatedFat = request.TrackSaturatedFat,
            CustomCarbohydrate = request.CustomCarbohydrate,
            TrackCarbohydrate = request.TrackCarbohydrate,
            CustomAddedSugars = request.CustomAddedSugars,
            TrackAddedSugars = request.TrackAddedSugars,
            CustomFiber = request.CustomFiber,
            TrackFiber = request.TrackFiber,
            CustomProtein = request.CustomProtein,
            TrackProtein = request.TrackProtein,
            CustomSodium = request.CustomSodium,
            TrackSodium = request.TrackSodium,
            CustomCholesterol = request.CustomCholesterol,
            TrackCholesterol = request.TrackCholesterol,
            CustomVitaminA = request.CustomVitaminA,
            TrackVitaminA = request.TrackVitaminA,
            CustomVitaminC = request.CustomVitaminC,
            TrackVitaminC = request.TrackVitaminC,
            CustomVitaminD = request.CustomVitaminD,
            TrackVitaminD = request.TrackVitaminD,
            CustomCalcium = request.CustomCalcium,
            TrackCalcium = request.TrackCalcium,
            CustomIron = request.CustomIron,
            TrackIron = request.TrackIron,
            CustomPotassium = request.CustomPotassium,
            TrackPotassium = request.TrackPotassium,
            CustomMagnesium = request.CustomMagnesium,
            TrackMagnesium = request.TrackMagnesium
        };
        await _dietaryProfileService.CreateOrUpdateAsync(model, _updateDietaryProfileValidator);

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        await _dietaryProfileService.DeleteAsync(UserId);

        return NoContent();
    }
}
