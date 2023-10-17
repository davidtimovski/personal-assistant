using System.Globalization;
using Api.Common;
using Chef.Api.Models;
using Chef.Api.Models.Recipes.Requests;
using Chef.Api.Models.Recipes.Responses;
using Chef.Application.Contracts;
using Chef.Application.Contracts.Ingredients;
using Chef.Application.Contracts.Recipes;
using Chef.Application.Contracts.Recipes.Models;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Sentry;

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
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes",
            $"{nameof(RecipesController)}.{nameof(GetAll)}",
            UserId
        );

        try
        {
            IEnumerable<SimpleRecipe> recipeDtos = _recipeService.GetAll(UserId, tr);

            return Ok(recipeDtos);
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

    [HttpGet("{id}/{currency}")]
    public IActionResult Get(int id, string currency)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes/{id}/{currency}",
            $"{nameof(RecipesController)}.{nameof(Get)}",
            UserId
        );

        try
        {
            RecipeDto? recipeDto = _recipeService.Get(id, UserId, currency, tr);
            if (recipeDto is null)
            {
                tr.Status = SpanStatus.NotFound;
                return NotFound();
            }

            foreach (var ingredient in recipeDto.Ingredients.Where(x => x.IsPublic))
            {
                ingredient.Name = _ingredientsLocalizer[ingredient.Name];
            }

            return Ok(recipeDto);
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
            $"{Request.Method} api/recipes/{id}/update",
            $"{nameof(RecipesController)}.{nameof(GetForUpdate)}",
            UserId
        );

        try
        {
            RecipeForUpdate? recipeDto = _recipeService.GetForUpdate(id, UserId, tr);
            if (recipeDto is null)
            {
                tr.Status = SpanStatus.NotFound;
                return NotFound();
            }

            foreach (var ingredient in recipeDto.Ingredients.Where(x => x.IsPublic))
            {
                ingredient.Name = _ingredientsLocalizer[ingredient.Name];
            }

            return Ok(recipeDto);

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

    [HttpGet("{id}/with-shares")]
    public IActionResult GetWithShares(int id)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes/{id}/update",
            $"{nameof(RecipesController)}.{nameof(GetWithShares)}",
            UserId
        );

        try
        {
            RecipeWithShares? recipeDto = _recipeService.GetWithShares(id, UserId, tr);

            return recipeDto is null ? NotFound() : Ok(recipeDto);
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

    [HttpGet("share-requests")]
    public IActionResult GetShareRequests()
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes/share-requests",
            $"{nameof(RecipesController)}.{nameof(GetShareRequests)}",
            UserId
        );

        try
        {
            IEnumerable<Application.Contracts.Recipes.Models.ShareRecipeRequest> shareRequests = _recipeService.GetShareRequests(UserId, tr);

            return Ok(shareRequests);
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

    [HttpGet("pending-share-requests-count")]
    public IActionResult GetPendingShareRequestsCount()
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes/pending-share-requests-count",
            $"{nameof(RecipesController)}.{nameof(GetPendingShareRequestsCount)}",
            UserId
        );

        try
        {
            int pendingShareRequestsCount = _recipeService.GetPendingShareRequestsCount(UserId, tr);

            return Ok(pendingShareRequestsCount);
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

    [HttpGet("{id}/sending")]
    public IActionResult GetForSending(int id)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes/{id}/sending",
            $"{nameof(RecipesController)}.{nameof(GetForSending)}",
            UserId
        );

        try
        {
            RecipeForSending recipeDto = _recipeService.GetForSending(id, UserId, tr);

            return recipeDto is null ? NotFound() : Ok(recipeDto);
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

    [HttpGet("send-requests")]
    public IActionResult GetSendRequests()
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes/send-requests",
            $"{nameof(RecipesController)}.{nameof(GetSendRequests)}",
            UserId
        );

        try
        {
            IEnumerable<SendRequestDto> sendRequestDtos = _recipeService.GetSendRequests(UserId, tr);

            return Ok(sendRequestDtos);
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

    [HttpGet("pending-send-requests-count")]
    public IActionResult GetPendingSendRequestsCount()
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes/pending-send-requests-count",
            $"{nameof(RecipesController)}.{nameof(GetPendingSendRequestsCount)}",
            UserId
        );

        try
        {
            int pendingSendRequestsCount = _recipeService.GetPendingSendRequestsCount(UserId, tr);

            return Ok(pendingSendRequestsCount);
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

    [HttpGet("{id}/review")]
    public IActionResult GetForReview(int id)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes/{id}/review",
            $"{nameof(RecipesController)}.{nameof(GetForReview)}",
            UserId
        );

        try
        {
            RecipeForReview? recipeDto = _recipeService.GetForReview(id, UserId, tr);

            return recipeDto is null ? NotFound() : Ok(recipeDto);
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
    public async Task<IActionResult> Create([FromBody] CreateRecipeRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes",
            $"{nameof(RecipesController)}.{nameof(GetAll)}",
            UserId
        );

        try
        {
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
            int id = await _recipeService.CreateAsync(model, _createRecipeValidator, tr, cancellationToken);

            return StatusCode(201, id);
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

    [HttpPost("upload-temp-image")]
    public async Task<IActionResult> UploadTempImage(IFormFile image, CancellationToken cancellationToken)
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

            var tempImageUriResult = await _cdnService.UploadTempAsync(uploadModel, _uploadTempImageValidator, tr, cancellationToken);

            if (tempImageUriResult.Status == ResultStatus.Error)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            if (tempImageUriResult.Status == ResultStatus.Invalid)
            {
                tr.Status = SpanStatus.InvalidArgument;
                return StatusCode(422);
            }

            var tempImageUri = tempImageUriResult.Data!.ToString();

            return StatusCode(201, new { tempImageUri });
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
    public async Task<IActionResult> Update([FromBody] UpdateRecipeRequest request, CancellationToken cancellationToken)
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

        try
        {
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
            UpdateRecipeResult result = await _recipeService.UpdateAsync(model, _updateRecipeValidator, tr, cancellationToken);

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
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes/{id}",
            $"{nameof(RecipesController)}.{nameof(Delete)}",
            UserId
        );

        try
        {
            DeleteRecipeResult result = await _recipeService.DeleteAsync(id, UserId, tr, cancellationToken);

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

    [HttpGet("can-share-with-user/{email}")]
    public IActionResult CanShareRecipeWithUser(string email)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes/can-share-with-user/{email}",
            $"{nameof(RecipesController)}.{nameof(CanShareRecipeWithUser)}",
            UserId
        );

        try
        {
            var response = new CanShareResponse
            {
                CanShare = false
            };

            var userResult = _userService.Get(email);
            if (userResult.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            if (userResult.Data != null)
            {
                response.UserId = userResult.Data.Id;
                response.ImageUri = userResult.Data.ImageUri;
                response.CanShare = _recipeService.CanShareWithUser(userResult.Data.Id, UserId, tr);
            }

            return Ok(response);
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

    [HttpPut("share")]
    public async Task<IActionResult> Share([FromBody] Models.Recipes.Requests.ShareRecipeRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes/share",
            $"{nameof(RecipesController)}.{nameof(Share)}",
            UserId
        );

        try
        {
            foreach (int removedUserId in request.RemovedShares)
            {
                if (!_recipeService.CheckIfUserCanBeNotifiedOfRecipeChange(request.RecipeId, removedUserId, tr))
                {
                    continue;
                }

                var currentUserResult = _userService.Get(UserId);
                if (currentUserResult.Failed)
                {
                    tr.Status = SpanStatus.InternalError;
                    return StatusCode(500);
                }

                var userResult = _userService.Get(removedUserId);
                if (userResult.Failed)
                {
                    tr.Status = SpanStatus.InternalError;
                    return StatusCode(500);
                }

                RecipeToNotify recipe = _recipeService.Get(request.RecipeId, tr);

                CultureInfo.CurrentCulture = new CultureInfo(userResult.Data!.Language, false);
                var message = _localizer["RemovedShareNotification", currentUserResult.Data!.Name, recipe.Name];

                var pushNotification = new ChefPushNotification
                {
                    SenderImageUri = currentUserResult.Data.ImageUri,
                    UserId = userResult.Data.Id,
                    Message = message
                };

                _senderService.Enqueue(pushNotification);
            }

            var model = new ShareRecipe
            {
                UserId = UserId,
                RecipeId = request.RecipeId,
                NewShares = request.NewShares,
                RemovedShares = request.RemovedShares,
            };
            await _recipeService.ShareAsync(model, _shareValidator, tr, cancellationToken);
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

    [HttpPut("share-is-accepted")]
    public async Task<IActionResult> SetShareIsAccepted([FromBody] SetShareIsAcceptedRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes/share-is-accepted",
            $"{nameof(RecipesController)}.{nameof(SetShareIsAccepted)}",
            UserId
        );

        try
        {
            SetShareIsAcceptedResult result = await _recipeService.SetShareIsAcceptedAsync(request.RecipeId, UserId, request.IsAccepted, tr, cancellationToken);
            if (!result.Notify())
            {
                return NoContent();
            }

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

    [HttpDelete("{id}/leave")]
    public async Task<IActionResult> Leave(int id, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes/{id}/leave",
            $"{nameof(RecipesController)}.{nameof(Leave)}",
            UserId
        );

        try
        {
            LeaveRecipeResult result = await _recipeService.LeaveAsync(id, UserId, tr, cancellationToken);

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

    [HttpGet("can-send-recipe-to-user/{email}/{recipeId}")]
    public IActionResult CanSendRecipeToUser(string email, int recipeId)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes/can-send-recipe-to-user/{email}/{recipeId}",
            $"{nameof(RecipesController)}.{nameof(CanSendRecipeToUser)}",
            UserId
        );

        try
        {
            var response = new CanSendResponse
            {
                CanSend = false,
            };

            var userResult = _userService.Get(email);
            if (userResult.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            if (userResult.Data != null)
            {
                var (canSend, alreadySent) = _recipeService.CheckSendRequest(recipeId, userResult.Data.Id, UserId, tr);

                response.UserId = userResult.Data.Id;
                response.ImageUri = userResult.Data.ImageUri;
                response.CanSend = canSend;
                response.AlreadySent = alreadySent;
            }

            return Ok(response);
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

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] CreateSendRequestRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes/send",
            $"{nameof(RecipesController)}.{nameof(Send)}",
            UserId
        );

        try
        {
            var model = new CreateSendRequest
            {
                UserId = UserId,
                RecipeId = request.RecipeId,
                RecipientsIds = request.RecipientsIds
            };
            SendRecipeResult result = await _recipeService.SendAsync(model, _createSendRequestValidator, tr, cancellationToken);

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

            return StatusCode(201, null);
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

    [HttpPut("decline-send-request")]
    public async Task<IActionResult> DeclineSendRequest([FromBody] DeclineSendRequestRequest dto, CancellationToken cancellationToken)
    {
        if (dto is null)
        {
            return BadRequest();
        }

        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes/decline-send-request",
            $"{nameof(RecipesController)}.{nameof(DeclineSendRequest)}",
            UserId
        );

        try
        {
            DeclineSendRequestResult result = await _recipeService.DeclineSendRequestAsync(dto.RecipeId, UserId, tr, cancellationToken);
            if (!result.Notify())
            {
                return NoContent();
            }

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

    [HttpDelete("{recipeId}/send-request")]
    public async Task<IActionResult> DeleteSendRequest(int recipeId, CancellationToken cancellationToken)
    {
        var tr = Metrics.StartTransactionWithUser(
            $"{Request.Method} api/recipes/{recipeId}/send-request",
            $"{nameof(RecipesController)}.{nameof(DeleteSendRequest)}",
            UserId
        );

        try
        {
            await _recipeService.DeleteSendRequestAsync(recipeId, UserId, tr, cancellationToken);
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

    [HttpPost("try-import")]
    public async Task<IActionResult> TryImport([FromBody] ImportRecipeRequest dto, CancellationToken cancellationToken)
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

        try
        {
            var importModel = new ImportRecipe
            {
                UserId = UserId
            };

            if (dto.CheckIfReviewRequired && _recipeService.IngredientsReviewIsRequired(dto.Id, importModel.UserId, tr))
            {
                return Ok(0);
            }

            importModel.Id = dto.Id;
            importModel.IngredientReplacements = dto.IngredientReplacements;

            // Copy recipe image if not default
            RecipeToNotify recipe = _recipeService.Get(importModel.Id, tr);

            var imageUriResult = await _cdnService.CopyAndUploadAsync(
                localTempPath: Path.Combine(_webHostEnvironment.ContentRootPath, "storage", "temp"),
                imageUriToCopy: new Uri(recipe.ImageUri),
                uploadPath: $"users/{importModel.UserId}/recipes",
                template: "recipe",
                tr,
                cancellationToken
            );
            if (imageUriResult.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            importModel.ImageUri = imageUriResult.Data!.ToString();

            int id = await _recipeService.ImportAsync(importModel, _importRecipeValidator, tr, cancellationToken);

            var currentUserResult = _userService.Get(importModel.UserId);
            if (currentUserResult.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            var recipeUserResult = _userService.Get(recipe.UserId);
            if (recipeUserResult.Failed)
            {
                tr.Status = SpanStatus.InternalError;
                return StatusCode(500);
            }

            CultureInfo.CurrentCulture = new CultureInfo(recipeUserResult.Data!.Language, false);
            var message = _localizer["AcceptedSendRequestNotification", currentUserResult.Data!.Name, recipe.Name];

            var pushNotification = new ChefPushNotification
            {
                SenderImageUri = currentUserResult.Data.ImageUri,
                UserId = recipeUserResult.Data.Id,
                Message = message
            };

            _senderService.Enqueue(pushNotification);

            return StatusCode(201, id);
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
