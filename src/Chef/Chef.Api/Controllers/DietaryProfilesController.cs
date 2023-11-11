using Api.Common;
using Chef.Api.Models.DietaryProfiles.Requests;
using Chef.Application.Contracts.DietaryProfiles;
using Chef.Application.Contracts.DietaryProfiles.Models;
using Core.Application.Contracts;
using Core.Application.Utils;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sentry;

namespace Chef.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class DietaryProfilesController : BaseController
{
    private readonly IDietaryProfileService _dietaryProfileService;
    private readonly IValidator<GetRecommendedDailyIntake> _getRecommendedDailyIntakeValidator;
    private readonly IValidator<UpdateDietaryProfile> _updateDietaryProfileValidator;
    
    public DietaryProfilesController(
        IUserIdLookup? userIdLookup,
        IUsersRepository? usersRepository,
        IDietaryProfileService? dietaryProfileService,
        IValidator<GetRecommendedDailyIntake>? getRecommendedDailyIntakeValidator,
        IValidator<UpdateDietaryProfile>? updateDietaryProfileValidator) : base(userIdLookup, usersRepository)
    {
        _dietaryProfileService = ArgValidator.NotNull(dietaryProfileService);
        _getRecommendedDailyIntakeValidator = ArgValidator.NotNull(getRecommendedDailyIntakeValidator);
        _updateDietaryProfileValidator = ArgValidator.NotNull(updateDietaryProfileValidator);
    }

    [HttpGet]
    public IActionResult Get()
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/dietaryprofiles",
            $"{nameof(DietaryProfilesController)}.{nameof(Get)}",
            UserId
        );

        try
        {
            var dietaryProfile = _dietaryProfileService.Get(UserId, tr);
            return Ok(dietaryProfile);
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

    [HttpPost]
    public IActionResult GetDailyIntake([FromBody] GetRecommendedDailyIntakeRequest request)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/dietaryprofiles",
            $"{nameof(DietaryProfilesController)}.{nameof(GetDailyIntake)}",
            UserId
        );

        try
        {
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
            RecommendedDailyIntake recommended = _dietaryProfileService.GetRecommendedDailyIntake(model, _getRecommendedDailyIntakeValidator, tr);

            return Ok(recommended);
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
    public async Task<IActionResult> CreateOrUpdate([FromBody] UpdateDietaryProfileRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/dietaryprofiles",
            $"{nameof(DietaryProfilesController)}.{nameof(CreateOrUpdate)}",
            UserId
        );

        try
        {
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
            await _dietaryProfileService.CreateOrUpdateAsync(model, _updateDietaryProfileValidator, tr, cancellationToken);
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

    [HttpDelete]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/dietaryprofiles",
            $"{nameof(DietaryProfilesController)}.{nameof(Delete)}",
            UserId
        );

        try
        {
            await _dietaryProfileService.DeleteAsync(UserId, tr, cancellationToken);

            return NoContent();
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
}
