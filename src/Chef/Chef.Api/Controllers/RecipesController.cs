using System.Globalization;
using Api.Common;
using Chef.Api.Models;
using Chef.Api.Models.Recipes.Requests;
using Chef.Api.Models.Recipes.Responses;
using Chef.Application.Contracts.Ingredients;
using Chef.Application.Contracts.Recipes;
using Chef.Application.Contracts.Recipes.Models;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using Core.Application.Contracts.Models.Sender;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using User = Core.Application.Entities.User;

namespace Chef.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
public class RecipesController : BaseController
{
    private readonly IRecipeService _recipeService;
    private readonly IIngredientService _ingredientService;
    private readonly IStringLocalizer<RecipesController> _localizer;
    private readonly IStringLocalizer<IngredientsController> _ingredientsLocalizer;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ICdnService _cdnService;
    private readonly IUserService _userService;
    private readonly ISenderService _senderService;
    private readonly IValidator<CreateRecipe> _createRecipeValidator;
    private readonly IValidator<UpdateRecipe> _updateRecipeValidator;
    private readonly IValidator<ShareRecipe> _shareValidator;
    private readonly IValidator<CreateSendRequest> _createSendRequestValidator;
    private readonly IValidator<ImportRecipe> _importRecipeValidator;
    private readonly IValidator<UploadTempImage> _uploadTempImageValidator;
    private readonly AppConfiguration _config;
    private readonly ILogger<RecipesController> _logger;

    public RecipesController(
        IUserIdLookup userIdLookup,
        IUsersRepository usersRepository,
        IRecipeService recipeService,
        IIngredientService ingredientService,
        IStringLocalizer<RecipesController> localizer,
        IStringLocalizer<IngredientsController> ingredientsLocalizer,
        IWebHostEnvironment webHostEnvironment,
        ICdnService cdnService,
        IUserService userService,
        ISenderService senderService,
        IValidator<CreateRecipe> createRecipeValidator,
        IValidator<UpdateRecipe> updateRecipeValidator,
        IValidator<ShareRecipe> shareValidator,
        IValidator<CreateSendRequest> createSendRequestValidator,
        IValidator<ImportRecipe> importRecipeValidator,
        IValidator<UploadTempImage> uploadTempImageValidator,
        IOptions<AppConfiguration> config,
        ILogger<RecipesController> logger) : base(userIdLookup, usersRepository)
    {
        _recipeService = recipeService;
        _ingredientService = ingredientService;
        _localizer = localizer;
        _ingredientsLocalizer = ingredientsLocalizer;
        _webHostEnvironment = webHostEnvironment;
        _cdnService = cdnService;
        _userService = userService;
        _senderService = senderService;
        _createRecipeValidator = createRecipeValidator;
        _updateRecipeValidator = updateRecipeValidator;
        _shareValidator = shareValidator;
        _createSendRequestValidator = createSendRequestValidator;
        _importRecipeValidator = importRecipeValidator;
        _uploadTempImageValidator = uploadTempImageValidator;
        _config = config.Value;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        IEnumerable<SimpleRecipe> recipeDtos = _recipeService.GetAll(UserId);

        return Ok(recipeDtos);
    }

    [HttpGet("{id}/{currency}")]
    public IActionResult Get(int id, string currency)
    {
        RecipeDto? recipeDto = _recipeService.Get(id, UserId, currency);
        if (recipeDto is null)
        {
            return NotFound();
        }

        foreach (var ingredient in recipeDto.Ingredients.Where(x => x.IsPublic))
        {
            ingredient.Name = _ingredientsLocalizer[ingredient.Name];
        }

        return Ok(recipeDto);
    }

    [HttpGet("{id}/update")]
    public IActionResult GetForUpdate(int id)
    {
        RecipeForUpdate? recipeDto = _recipeService.GetForUpdate(id, UserId);
        if (recipeDto is null)
        {
            return NotFound();
        }

        foreach (var ingredient in recipeDto.Ingredients.Where(x => x.IsPublic))
        {
            ingredient.Name = _ingredientsLocalizer[ingredient.Name];
        }

        return Ok(recipeDto);
    }

    [HttpGet("{id}/with-shares")]
    public IActionResult GetWithShares(int id)
    {
        RecipeWithShares? recipeDto = _recipeService.GetWithShares(id, UserId);
        return recipeDto is null ? NotFound() : Ok(recipeDto);
    }

    [HttpGet("share-requests")]
    public IActionResult GetShareRequests()
    {
        IEnumerable<Application.Contracts.Recipes.Models.ShareRecipeRequest> shareRequests = _recipeService.GetShareRequests(UserId);

        return Ok(shareRequests);
    }

    [HttpGet("pending-share-requests-count")]
    public IActionResult GetPendingShareRequestsCount()
    {
        int pendingShareRequestsCount = _recipeService.GetPendingShareRequestsCount(UserId);

        return Ok(pendingShareRequestsCount);
    }

    [HttpGet("{id}/sending")]
    public IActionResult GetForSending(int id)
    {
        RecipeForSending recipeDto = _recipeService.GetForSending(id, UserId);
        return recipeDto is null ? NotFound() : Ok(recipeDto);
    }

    [HttpGet("send-requests")]
    public IActionResult GetSendRequests()
    {
        IEnumerable<SendRequestDto> sendRequestDtos = _recipeService.GetSendRequests(UserId);

        return Ok(sendRequestDtos);
    }

    [HttpGet("pending-send-requests-count")]
    public IActionResult GetPendingSendRequestsCount()
    {
        int pendingSendRequestsCount = _recipeService.GetPendingSendRequestsCount(UserId);

        return Ok(pendingSendRequestsCount);
    }

    [HttpGet("{id}/review")]
    public IActionResult GetForReview(int id)
    {
        RecipeForReview? recipeDto = _recipeService.GetForReview(id, UserId);
        return recipeDto is null ? NotFound() : Ok(recipeDto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecipeRequest request)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes",
            $"{nameof(RecipesController)}.{nameof(Create)}",
            UserId
        );

        var model = new CreateRecipe
        {
            UserId = UserId,
            Name = request.Name,
            Description = request.Description,
            Ingredients = request.Ingredients.Select(x => new Application.Contracts.Recipes.Models.UpdateRecipeIngredient
            {
                Id = x.Id,
                Name = x.Name,
                Amount = x.Amount,
                Unit = x.Unit
            }).ToList(),
            Instructions = request.Instructions,
            PrepDuration = request.PrepDuration,
            CookDuration = request.CookDuration,
            Servings = request.Servings,
            ImageUri = request.ImageUri,
            VideoUrl = request.VideoUrl,
        };
        int id = await _recipeService.CreateAsync(model, _createRecipeValidator, tr);

        tr.Finish();

        return StatusCode(201, id);
    }

    [HttpPost("upload-temp-image")]
    public async Task<IActionResult> UploadTempImage(IFormFile image)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes/upload-temp-image",
            $"{nameof(RecipesController)}.{nameof(UploadTempImage)}",
            UserId
        );

        try
        {
            var uploadModel = new UploadTempImage(
                UserId,
                Path.Combine(_webHostEnvironment.ContentRootPath, "storage", "temp"),
                $"users/{UserId}/recipes",
                "recipe",
                image.Length,
                image.FileName);

            await image.CopyToAsync(uploadModel.File);

            string tempImageUri = await _cdnService.UploadTempAsync(uploadModel, _uploadTempImageValidator, tr);

            return StatusCode(201, new { tempImageUri });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UploadTempImage)}");
            throw;
        }
        finally
        {
            tr.Finish();
        }
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateRecipeRequest request)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes",
            $"{nameof(RecipesController)}.{nameof(Update)}",
            UserId
        );

        var model = new UpdateRecipe
        {
            UserId = UserId,
            Id = request.Id,
            Name = request.Name,
            Description = request.Description,
            Ingredients = request.Ingredients.Select(x => new Application.Contracts.Recipes.Models.UpdateRecipeIngredient
            {
                Id = x.Id,
                Name = x.Name,
                Amount = x.Amount,
                Unit = x.Unit
            }).ToList(),
            Instructions = request.Instructions,
            PrepDuration = request.PrepDuration,
            CookDuration = request.CookDuration,
            Servings = request.Servings,
            ImageUri = request.ImageUri,
            VideoUrl = request.VideoUrl,
        };
        UpdateRecipeResult result = await _recipeService.UpdateAsync(model, _updateRecipeValidator, tr);

        try
        {
            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["UpdatedRecipeNotification", result.ActionUserName, result.RecipeName];

                var pushNotification = new ChefPushNotification
                {
                    SenderImageUri = result.ActionUserImageUri,
                    UserId = recipient.Id,
                    Message = message
                };

                _senderService.Enqueue(pushNotification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Update)}");
            throw;
        }
        finally
        {
            tr.Finish();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes",
            $"{nameof(RecipesController)}.{nameof(Delete)}",
            UserId
        );

        DeleteRecipeResult result = await _recipeService.DeleteAsync(id, UserId, tr);

        try
        {
            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["DeletedRecipeNotification", result.ActionUserName, result.RecipeName];

                var pushNotification = new ChefPushNotification
                {
                    SenderImageUri = result.ActionUserImageUri,
                    UserId = recipient.Id,
                    Message = message
                };

                _senderService.Enqueue(pushNotification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Delete)}");
            throw;
        }
        finally
        {
            tr.Finish();
        }

        return NoContent();
    }

    [HttpGet("can-share-with-user/{email}")]
    public IActionResult CanShareRecipeWithUser(string email)
    {
        var response = new CanShareResponse
        {
            CanShare = false
        };

        var user = _userService.Get(email);

        if (user != null)
        {
            response.UserId = user.Id;
            response.ImageUri = user.ImageUri;
            response.CanShare = _recipeService.CanShareWithUser(user.Id, UserId);
        }

        return Ok(response);
    }

    [HttpPut("share")]
    public async Task<IActionResult> Share([FromBody] Models.Recipes.Requests.ShareRecipeRequest request)
    {
        if (request is null)
        {
            return BadRequest();
        }

        try
        {
            foreach (int removedUserId in request.RemovedShares)
            {
                if (!_recipeService.CheckIfUserCanBeNotifiedOfRecipeChange(request.RecipeId, removedUserId))
                {
                    continue;
                }

                var currentUser = _userService.Get(UserId);
                var user = _userService.Get(removedUserId);
                RecipeToNotify recipe = _recipeService.Get(request.RecipeId);

                CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                var message = _localizer["RemovedShareNotification", currentUser.Name, recipe.Name];

                var pushNotification = new ChefPushNotification
                {
                    SenderImageUri = currentUser.ImageUri,
                    UserId = user.Id,
                    Message = message
                };

                _senderService.Enqueue(pushNotification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Share)}");
            throw;
        }

        var model = new ShareRecipe
        {
            UserId = UserId,
            RecipeId = request.RecipeId,
            NewShares = request.NewShares,
            RemovedShares = request.RemovedShares,
        };
        await _recipeService.ShareAsync(model, _shareValidator);

        return NoContent();
    }

    [HttpPut("share-is-accepted")]
    public async Task<IActionResult> SetShareIsAccepted([FromBody] SetShareIsAcceptedRequest request)
    {
        if (request is null)
        {
            return BadRequest();
        }

        SetShareIsAcceptedResult result = await _recipeService.SetShareIsAcceptedAsync(request.RecipeId, UserId, request.IsAccepted);
        if (!result.Notify())
        {
            return NoContent();
        }

        try
        {
            var localizerKey = request.IsAccepted ? "JoinedRecipeNotification" : "DeclinedShareRequestNotification";
            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer[localizerKey, result.ActionUserName, result.RecipeName];

                var pushNotification = new ChefPushNotification
                {
                    SenderImageUri = result.ActionUserImageUri,
                    UserId = recipient.Id,
                    Message = message
                };

                _senderService.Enqueue(pushNotification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(SetShareIsAccepted)}");
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}/leave")]
    public async Task<IActionResult> Leave(int id)
    {
        LeaveRecipeResult result = await _recipeService.LeaveAsync(id, UserId);

        try
        {
            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["LeftRecipeNotification", result.ActionUserName, result.RecipeName];

                var pushNotification = new ChefPushNotification
                {
                    SenderImageUri = result.ActionUserImageUri,
                    UserId = recipient.Id,
                    Message = message
                };

                _senderService.Enqueue(pushNotification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Leave)}");
            throw;
        }

        return NoContent();
    }

    [HttpGet("can-send-recipe-to-user/{email}/{recipeId}")]
    public IActionResult CanSendRecipeToUser(string email, int recipeId)
    {
        var response = new CanSendResponse
        {
            CanSend = false,
        };

        var user = _userService.Get(email);
        if (user != null)
        {
            var (canSend, alreadySent) = _recipeService.CheckSendRequest(recipeId, user.Id, UserId);

            response.UserId = user.Id;
            response.ImageUri = user.ImageUri;
            response.CanSend = canSend;
            response.AlreadySent = alreadySent;
        }

        return Ok(response);
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] CreateSendRequestRequest request)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var model = new CreateSendRequest
        {
            UserId = UserId,
            RecipeId = request.RecipeId,
            RecipientsIds = request.RecipientsIds
        };
        SendRecipeResult result = await _recipeService.SendAsync(model, _createSendRequestValidator);

        try
        {
            foreach (var recipient in result.NotificationRecipients)
            {
                CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
                var message = _localizer["SentRecipeNotification", result.ActionUserName, result.RecipeName];

                var pushNotification = new ChefPushNotification
                {
                    SenderImageUri = result.ActionUserImageUri,
                    UserId = recipient.Id,
                    Message = message,
                    OpenUrl = $"{_config.Url}/inbox"
                };

                _senderService.Enqueue(pushNotification);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Send)}");
            throw;
        }

        return StatusCode(201, null);
    }

    [HttpPut("decline-send-request")]
    public async Task<IActionResult> DeclineSendRequest([FromBody] DeclineSendRequestRequest dto)
    {
        if (dto is null)
        {
            return BadRequest();
        }

        DeclineSendRequestResult result = await _recipeService.DeclineSendRequestAsync(dto.RecipeId, UserId);
        if (!result.Notify())
        {
            return NoContent();
        }

        try
        {
            NotificationRecipient recipient = result.NotificationRecipients.First();
            CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
            var message = _localizer["DeclinedSendRequestNotification", result.ActionUserName, result.RecipeName];

            var pushNotification = new ChefPushNotification
            {
                SenderImageUri = result.ActionUserImageUri,
                UserId = recipient.Id,
                Message = message
            };

            _senderService.Enqueue(pushNotification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeclineSendRequest)}");
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{recipeId}/send-request")]
    public async Task<IActionResult> DeleteSendRequest(int recipeId)
    {
        await _recipeService.DeleteSendRequestAsync(recipeId, UserId);

        return NoContent();
    }

    [HttpPost("try-import")]
    public async Task<IActionResult> TryImport([FromBody] ImportRecipeRequest dto)
    {
        if (dto is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes/try-import",
            $"{nameof(RecipesController)}.{nameof(TryImport)}",
            UserId
        );

        var importModel = new ImportRecipe
        {
            UserId = UserId
        };

        if (dto.CheckIfReviewRequired && _recipeService.IngredientsReviewIsRequired(dto.Id, importModel.UserId))
        {
            return Ok(0);
        }

        importModel.Id = dto.Id;
        importModel.IngredientReplacements = dto.IngredientReplacements;

        // Copy recipe image if not default
        RecipeToNotify recipe = _recipeService.Get(importModel.Id);

        importModel.ImageUri = await _cdnService.CopyAndUploadAsync(
            localTempPath: Path.Combine(_webHostEnvironment.ContentRootPath, "storage", "temp"),
            imageUriToCopy: recipe.ImageUri,
            uploadPath: $"users/{importModel.UserId}/recipes",
            template: "recipe",
            tr
        );

        int id = await _recipeService.ImportAsync(importModel, _importRecipeValidator);

        // Notify
        User currentUser = _userService.Get(importModel.UserId);
        User recipeUser = _userService.Get(recipe.UserId);

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(recipeUser.Language, false);
            var message = _localizer["AcceptedSendRequestNotification", currentUser.Name, recipe.Name];

            var pushNotification = new ChefPushNotification
            {
                SenderImageUri = currentUser.ImageUri,
                UserId = recipeUser.Id,
                Message = message
            };

            _senderService.Enqueue(pushNotification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(TryImport)}");
            throw;
        }
        finally
        {
            tr.Finish();
        }

        return StatusCode(201, id);
    }
}
