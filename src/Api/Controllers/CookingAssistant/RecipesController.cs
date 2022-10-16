using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Api.Config;
using Api.Models;
using Api.Models.CookingAssistant.Recipes;
using Application.Contracts.Common;
using Application.Contracts.Common.Models;
using Application.Contracts.CookingAssistant.Ingredients;
using Application.Contracts.CookingAssistant.Recipes;
using Application.Contracts.CookingAssistant.Recipes.Models;
using Domain.Entities.Common;
using FluentValidation;
using Infrastructure.Sender.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Api.Controllers.CookingAssistant;

[Authorize]
[EnableCors("AllowCookingAssistant")]
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
    private readonly Urls _urls;

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
        IOptions<Urls> urls) : base(userIdLookup, usersRepository)
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
        _urls = urls.Value;
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
        RecipeDto recipeDto = _recipeService.Get(id, UserId, currency);
        if (recipeDto == null)
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
        RecipeForUpdate recipeDto = _recipeService.GetForUpdate(id, UserId);
        if (recipeDto == null)
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
        RecipeWithShares recipeDto = _recipeService.GetWithShares(id, UserId);
        if (recipeDto == null)
        {
            return NotFound();
        }

        return Ok(recipeDto);
    }

    [HttpGet("share-requests")]
    public IActionResult GetShareRequests()
    {
        IEnumerable<ShareRecipeRequest> shareRequests = _recipeService.GetShareRequests(UserId);

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
        if (recipeDto == null)
        {
            return NotFound();
        }

        return Ok(recipeDto);
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
        int pendingSendRequestsCount =  _recipeService.GetPendingSendRequestsCount(UserId);

        return Ok(pendingSendRequestsCount);
    }

    [HttpGet("{id}/review")]
    public IActionResult GetForReview(int id)
    {
        RecipeForReview recipeDto = _recipeService.GetForReview(id, UserId);
        if (recipeDto == null)
        {
            return NotFound();
        }

        return Ok(recipeDto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecipe dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        dto.UserId = UserId;

        int id = await _recipeService.CreateAsync(dto, _createRecipeValidator);

        return StatusCode(201, id);
    }

    [HttpPost("upload-temp-image")]
    public async Task<IActionResult> UploadTempImage(IFormFile image)
    {
        var uploadModel = new UploadTempImage(
            UserId,
            Path.Combine(_webHostEnvironment.ContentRootPath, "storage", "temp"),
            $"users/{UserId}/recipes",
            "recipe")
        {
            Length = image.Length,
            FileName = image.FileName
        };
        await image.CopyToAsync(uploadModel.File);

        string tempImageUri = await _cdnService.UploadTempAsync(uploadModel, _uploadTempImageValidator);

        return StatusCode(201, new { tempImageUri });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateRecipe dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        dto.UserId = UserId;

        UpdateRecipeResult result = await _recipeService.UpdateAsync(dto, _updateRecipeValidator);

        foreach (var recipient in result.NotificationRecipients)
        {
            CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
            var message = _localizer["UpdatedRecipeNotification", CurrentUserName, result.RecipeName];

            var pushNotification = new CookingAssistantPushNotification
            {
                SenderImageUri = result.ActionUserImageUri,
                UserId = recipient.Id,
                Message = message
            };

            _senderService.Enqueue(pushNotification);
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        DeleteRecipeResult result = await _recipeService.DeleteAsync(id, UserId);

        foreach (var recipient in result.NotificationRecipients)
        {
            CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
            var message = _localizer["DeletedRecipeNotification", CurrentUserName, result.RecipeName];

            var pushNotification = new CookingAssistantPushNotification
            {
                SenderImageUri = result.ActionUserImageUri,
                UserId = recipient.Id,
                Message = message
            };

            _senderService.Enqueue(pushNotification);
        }

        return NoContent();
    }

    [HttpGet("can-share-with-user/{email}")]
    public IActionResult CanShareRecipeWithUser(string email)
    {
        var canShareVm = new CanShareVm
        {
            CanShare = false
        };

        var user = _userService.Get(email);

        if (user != null)
        {
            canShareVm.UserId = user.Id;
            canShareVm.ImageUri = user.ImageUri;
            canShareVm.CanShare = _recipeService.CanShareWithUser(user.Id, UserId);
        }

        return Ok(canShareVm);
    }

    [HttpPut("share")]
    public async Task<IActionResult> Share([FromBody] ShareRecipe dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        dto.UserId = UserId;

        foreach (int removedUserId in dto.RemovedShares)
        {
            if (!_recipeService.CheckIfUserCanBeNotifiedOfRecipeChange(dto.RecipeId, removedUserId))
            {
                continue;
            }

            var currentUser = _userService.Get(dto.UserId);
            var user = _userService.Get(removedUserId);
            RecipeToNotify recipe = _recipeService.Get(dto.RecipeId);

            CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
            var message = _localizer["RemovedShareNotification", CurrentUserName, recipe.Name];

            var pushNotification = new CookingAssistantPushNotification
            {
                SenderImageUri = currentUser.ImageUri,
                UserId = user.Id,
                Message = message
            };

            _senderService.Enqueue(pushNotification);
        }

        await _recipeService.ShareAsync(dto, _shareValidator);

        return NoContent();
    }

    [HttpPut("share-is-accepted")]
    public async Task<IActionResult> SetShareIsAccepted([FromBody] SetShareIsAcceptedDto dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        SetShareIsAcceptedResult result = await _recipeService.SetShareIsAcceptedAsync(dto.RecipeId, UserId, dto.IsAccepted);
        if (!result.Notify())
        {
            return NoContent();
        }

        var localizerKey = dto.IsAccepted ? "JoinedRecipeNotification" : "DeclinedShareRequestNotification";
        foreach (var recipient in result.NotificationRecipients)
        {
            CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
            var message = _localizer[localizerKey, CurrentUserName, result.RecipeName];

            var pushNotification = new CookingAssistantPushNotification
            {
                SenderImageUri = result.ActionUserImageUri,
                UserId = recipient.Id,
                Message = message
            };

            _senderService.Enqueue(pushNotification);
        }

        return NoContent();
    }

    [HttpDelete("{id}/leave")]
    public async Task<IActionResult> Leave(int id)
    {
        LeaveRecipeResult result = await _recipeService.LeaveAsync(id, UserId);

        foreach (var recipient in result.NotificationRecipients)
        {
            CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
            var message = _localizer["LeftRecipeNotification", CurrentUserName, result.RecipeName];

            var pushNotification = new CookingAssistantPushNotification
            {
                SenderImageUri = result.ActionUserImageUri,
                UserId = recipient.Id,
                Message = message
            };

            _senderService.Enqueue(pushNotification);
        }

        return NoContent();
    }

    [HttpGet("can-send-recipe-to-user/{email}/{recipeId}")]
    public IActionResult CanSendRecipeToUser(string email, int recipeId)
    {
        var canSendDto = new CanSendDto();

        var user = _userService.Get(email);
        if (user != null)
        {
            canSendDto.UserId = user.Id;
            canSendDto.ImageUri = user.ImageUri;

            var (canSend, alreadySent) = _recipeService.CheckSendRequest(recipeId, user.Id, UserId);
            canSendDto.CanSend = canSend;
            canSendDto.AlreadySent = alreadySent;
        }

        return Ok(canSendDto);
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] CreateSendRequest dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        dto.UserId = UserId;

        SendRecipeResult result = await _recipeService.SendAsync(dto, _createSendRequestValidator);

        foreach (var recipient in result.NotificationRecipients)
        {
            CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
            var message = _localizer["SentRecipeNotification", CurrentUserName, result.RecipeName];

            var pushNotification = new CookingAssistantPushNotification
            {
                SenderImageUri = result.ActionUserImageUri,
                UserId = recipient.Id,
                Message = message,
                OpenUrl = $"{_urls.CookingAssistant}/inbox"
            };

            _senderService.Enqueue(pushNotification);
        }

        return StatusCode(201, null);
    }

    [HttpPut("decline-send-request")]
    public async Task<IActionResult> DeclineSendRequest([FromBody] DeclineSendRequestDto dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

        DeclineSendRequestResult result = await _recipeService.DeclineSendRequestAsync(dto.RecipeId, UserId);
        if (!result.Notify())
        {
            return NoContent();
        }

        NotificationRecipient recipient = result.NotificationRecipients.First();
        CultureInfo.CurrentCulture = new CultureInfo(recipient.Language, false);
        var message = _localizer["DeclinedSendRequestNotification", CurrentUserName, result.RecipeName];

        var pushNotification = new CookingAssistantPushNotification
        {
            SenderImageUri = result.ActionUserImageUri,
            UserId = recipient.Id,
            Message = message
        };

        _senderService.Enqueue(pushNotification);

        return NoContent();
    }

    [HttpDelete("{recipeId}/send-request")]
    public async Task<IActionResult> DeleteSendRequest(int recipeId)
    {
        await _recipeService.DeleteSendRequestAsync(recipeId, UserId);

        return NoContent();
    }

    [HttpPost("try-import")]
    public async Task<IActionResult> TryImport([FromBody] ImportRecipeDto dto)
    {
        if (dto == null)
        {
            return BadRequest();
        }

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
            template: "recipe"
        );

        int id = await _recipeService.ImportAsync(importModel, _importRecipeValidator);

        // Notify
        User currentUser = _userService.Get(importModel.UserId);
        User recipeUser = _userService.Get(recipe.UserId);

        CultureInfo.CurrentCulture = new CultureInfo(recipeUser.Language, false);
        var message = _localizer["AcceptedSendRequestNotification", CurrentUserName, recipe.Name];

        var pushNotification = new CookingAssistantPushNotification
        {
            SenderImageUri = currentUser.ImageUri,
            UserId = recipeUser.Id,
            Message = message
        };

        _senderService.Enqueue(pushNotification);

        return StatusCode(201, id);
    }
}
