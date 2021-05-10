using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Api.Config;
using Api.Models;
using Api.Models.CookingAssistant.Recipes;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.Common.Models;
using PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using PersonalAssistant.Domain.Entities.Common;
using PersonalAssistant.Infrastructure.Identity;
using PersonalAssistant.Infrastructure.Sender.Models;

namespace Api.Controllers.CookingAssistant
{
    [Authorize]
    [EnableCors("AllowCookingAssistant")]
    [Route("api/[controller]")]
    public class RecipesController : Controller
    {
        private readonly IRecipeService _recipeService;
        private readonly IIngredientService _ingredientService;
        private readonly ITaskService _tasksService;
        private readonly IStringLocalizer<RecipesController> _localizer;
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
            IRecipeService recipeService,
            IIngredientService ingredientService,
            ITaskService tasksService,
            IStringLocalizer<RecipesController> localizer,
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
            IOptions<Urls> urls)
        {
            _recipeService = recipeService;
            _ingredientService = ingredientService;
            _tasksService = tasksService;
            _localizer = localizer;
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

            IEnumerable<SimpleRecipe> recipeDtos = await _recipeService.GetAllAsync(userId);

            return Ok(recipeDtos);
        }

        [HttpGet("{id}/{currency}")]
        public async Task<IActionResult> Get(int id, string currency)
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

            RecipeDto recipeDto = await _recipeService.GetAsync(id, userId, currency);
            if (recipeDto == null)
            {
                return NotFound();
            }

            return Ok(recipeDto);
        }

        [HttpGet("{id}/update")]
        public async Task<IActionResult> GetForUpdate(int id)
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

            RecipeForUpdate recipeDto = await _recipeService.GetForUpdateAsync(id, userId);
            if (recipeDto == null)
            {
                return NotFound();
            }

            return Ok(recipeDto);
        }

        [HttpGet("{id}/with-shares")]
        public async Task<IActionResult> GetWithShares(int id)
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

            RecipeWithShares list = await _recipeService.GetWithSharesAsync(id, userId);
            if (list == null)
            {
                return NotFound();
            }

            return Ok(list);
        }

        [HttpGet("share-requests")]
        public async Task<IActionResult> GetShareRequests()
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

            IEnumerable<ShareRecipeRequest> shareRequests = await _recipeService.GetShareRequestsAsync(userId);

            return Ok(shareRequests);
        }

        [HttpGet("pending-share-requests-count")]
        public async Task<IActionResult> GetPendingShareRequestsCount()
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

            int pendingShareRequestsCount = await _recipeService.GetPendingShareRequestsCountAsync(userId);

            return Ok(pendingShareRequestsCount);
        }

        [HttpGet("{id}/sending")]
        public async Task<IActionResult> GetForSending(int id)
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

            RecipeForSending recipeDto = await _recipeService.GetForSendingAsync(id, userId);
            if (recipeDto == null)
            {
                return NotFound();
            }

            return Ok(recipeDto);
        }

        [HttpGet("send-requests")]
        public async Task<IActionResult> GetSendRequests()
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

            IEnumerable<SendRequestDto> sendRequestDtos = await _recipeService.GetSendRequestsAsync(userId);

            return Ok(sendRequestDtos);
        }

        [HttpGet("pending-send-requests-count")]
        public async Task<IActionResult> GetPendingSendRequestsCount()
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

            int pendingSendRequestsCount = await _recipeService.GetPendingSendRequestsCountAsync(userId);

            return Ok(pendingSendRequestsCount);
        }

        [HttpGet("{id}/review")]
        public async Task<IActionResult> GetForReview(int id)
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

            RecipeForReview recipeDto = await _recipeService.GetForReviewAsync(id, userId);
            if (recipeDto == null)
            {
                return NotFound();
            }

            recipeDto.IngredientSuggestions = await _ingredientService.GetIngredientReviewSuggestionsAsync(userId);

            return Ok(recipeDto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRecipe dto)
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

            int id = await _recipeService.CreateAsync(dto, _createRecipeValidator);

            return StatusCode(201, id);
        }

        [HttpPost("upload-temp-image")]
        public async Task<IActionResult> UploadTempImage(IFormFile image)
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

            var uploadModel = new UploadTempImage(
                userId,
                Path.Combine(_webHostEnvironment.ContentRootPath, "storage", "temp"),
                $"users/{userId}/recipes",
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

            try
            {
                dto.UserId = IdentityHelper.GetUserId(User);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            RecipeToNotify original = await _recipeService.UpdateAsync(dto, _updateRecipeValidator);

            // Notify
            var usersToBeNotified = await _userService.GetToBeNotifiedOfRecipeChangeAsync(original.Id, dto.UserId);
            if (usersToBeNotified.Any())
            {
                var currentUser = await _userService.GetAsync(dto.UserId);

                foreach (var user in usersToBeNotified)
                {
                    CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                    var message = _localizer["UpdatedRecipeNotification", IdentityHelper.GetUserName(User), original.Name];

                    var pushNotification = new PushNotification
                    {
                        SenderImageUri = currentUser.ImageUri,
                        UserId = user.Id,
                        Application = "Cooking Assistant",
                        Message = message
                    };

                    _senderService.Enqueue(pushNotification);
                }
            }

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

            var deletedRecipeName = await _recipeService.DeleteAsync(id, userId);

            // Notify
            var usersToBeNotified = await _userService.GetToBeNotifiedOfRecipeDeletionAsync(id);
            if (usersToBeNotified.Any())
            {
                var currentUser = await _userService.GetAsync(userId);

                foreach (var user in usersToBeNotified)
                {
                    CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                    var message = _localizer["DeletedRecipeNotification", IdentityHelper.GetUserName(User), deletedRecipeName];

                    var pushNotification = new PushNotification
                    {
                        SenderImageUri = currentUser.ImageUri,
                        UserId = user.Id,
                        Application = "Cooking Assistant",
                        Message = message
                    };

                    _senderService.Enqueue(pushNotification);
                }
            }

            return NoContent();
        }

        [HttpGet("can-share-with-user/{email}")]
        public async Task<IActionResult> CanShareRecipeWithUser(string email)
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

            var canShareVm = new CanShareVm
            {
                CanShare = false
            };

            var user = await _userService.GetAsync(email);

            if (user != null)
            {
                canShareVm.UserId = user.Id;
                canShareVm.ImageUri = user.ImageUri;
                canShareVm.CanShare = _recipeService.CanShareWithUser(user.Id, userId);
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

            try
            {
                dto.UserId = IdentityHelper.GetUserId(User);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            foreach (int removedUserId in dto.RemovedShares)
            {
                // Notify
                if (await _userService.CheckIfUserCanBeNotifiedOfRecipeChangeAsync(dto.RecipeId, removedUserId))
                {
                    var currentUser = await _userService.GetAsync(dto.UserId);
                    var user = await _userService.GetAsync(removedUserId);
                    RecipeToNotify recipe = await _recipeService.GetAsync(dto.RecipeId);

                    CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                    var message = _localizer["RemovedShareNotification", IdentityHelper.GetUserName(User), recipe.Name];

                    var pushNotification = new PushNotification
                    {
                        SenderImageUri = currentUser.ImageUri,
                        UserId = user.Id,
                        Application = "Cooking Assistant",
                        Message = message
                    };

                    _senderService.Enqueue(pushNotification);
                }
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

            int userId;
            try
            {
                userId = IdentityHelper.GetUserId(User);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            await _recipeService.SetShareIsAcceptedAsync(dto.RecipeId, userId, dto.IsAccepted);

            // Notify
            var usersToBeNotified = await _userService.GetToBeNotifiedOfRecipeChangeAsync(dto.RecipeId, userId);
            if (usersToBeNotified.Any())
            {
                var currentUser = await _userService.GetAsync(userId);
                RecipeToNotify recipe = await _recipeService.GetAsync(dto.RecipeId);
                var localizerKey = dto.IsAccepted ? "JoinedRecipeNotification" : "DeclinedShareRequestNotification";

                foreach (var user in usersToBeNotified)
                {
                    CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                    var message = _localizer[localizerKey, IdentityHelper.GetUserName(User), recipe.Name];

                    var pushNotification = new PushNotification
                    {
                        SenderImageUri = currentUser.ImageUri,
                        UserId = user.Id,
                        Application = "Cooking Assistant",
                        Message = message
                    };

                    _senderService.Enqueue(pushNotification);
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}/leave")]
        public async Task<IActionResult> Leave(int id)
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

            bool shareWasAccepted = await _recipeService.LeaveAsync(id, userId);

            // Notify if joined in the first place
            if (shareWasAccepted)
            {
                var usersToBeNotified = await _userService.GetToBeNotifiedOfRecipeChangeAsync(id, userId);
                if (usersToBeNotified.Any())
                {
                    var currentUser = await _userService.GetAsync(userId);
                    RecipeToNotify recipe = await _recipeService.GetAsync(id);

                    foreach (var user in usersToBeNotified)
                    {
                        CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                        var message = _localizer["LeftRecipeNotification", IdentityHelper.GetUserName(User), recipe.Name];

                        var pushNotification = new PushNotification
                        {
                            SenderImageUri = currentUser.ImageUri,
                            UserId = user.Id,
                            Application = "Cooking Assistant",
                            Message = message
                        };

                        _senderService.Enqueue(pushNotification);
                    }
                }
            }

            return NoContent();
        }

        [HttpGet("can-send-recipe-to-user/{email}/{recipeId}")]
        public async Task<IActionResult> CanSendRecipeToUser(string email, int recipeId)
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

            var canSendDto = new CanSendDto();

            var user = await _userService.GetAsync(email);
            if (user != null)
            {
                canSendDto.UserId = user.Id;
                canSendDto.ImageUri = user.ImageUri;

                var (canSend, alreadySent) = _recipeService.CheckSendRequest(recipeId, user.Id, userId);
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

            try
            {
                dto.UserId = IdentityHelper.GetUserId(User);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            await _recipeService.SendAsync(dto, _createSendRequestValidator);

            // Notify
            var usersToBeNotified = await _userService.GetToBeNotifiedOfRecipeSentAsync(dto.RecipeId);
            if (usersToBeNotified.Any())
            {
                var currentUser = await _userService.GetAsync(dto.UserId);
                RecipeToNotify recipe = await _recipeService.GetAsync(dto.RecipeId);

                foreach (var user in usersToBeNotified)
                {
                    CultureInfo.CurrentCulture = new CultureInfo(user.Language, false);
                    var message = _localizer["SentRecipeNotification", IdentityHelper.GetUserName(User), recipe.Name];

                    var pushNotification = new PushNotification
                    {
                        SenderImageUri = currentUser.ImageUri,
                        UserId = user.Id,
                        Application = "Cooking Assistant",
                        Message = message,
                        OpenUrl = $"{_urls.CookingAssistant}/inbox"
                    };

                    _senderService.Enqueue(pushNotification);
                }
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

            int userId;
            try
            {
                userId = IdentityHelper.GetUserId(User);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            await _recipeService.DeclineSendRequestAsync(dto.RecipeId, userId);

            // Notify
            var currentUser = await _userService.GetAsync(userId);
            RecipeToNotify recipe = await _recipeService.GetAsync(dto.RecipeId);
            var recipeUser = await _userService.GetAsync(recipe.UserId);

            CultureInfo.CurrentCulture = new CultureInfo(recipeUser.Language, false);
            var message = _localizer["DeclinedSendRequestNotification", IdentityHelper.GetUserName(User), recipe.Name];

            var pushNotification = new PushNotification
            {
                SenderImageUri = currentUser.ImageUri,
                UserId = recipeUser.Id,
                Application = "Cooking Assistant",
                Message = message
            };

            _senderService.Enqueue(pushNotification);

            return NoContent();
        }

        [HttpDelete("{recipeId}/send-request")]
        public async Task<IActionResult> DeleteSendRequest(int recipeId)
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

            await _recipeService.DeleteSendRequestAsync(recipeId, userId);

            return NoContent();
        }

        [HttpPost("try-import")]
        public async Task<IActionResult> TryImport([FromBody] ImportRecipeDto dto)
        {
            if (dto == null)
            {
                return BadRequest();
            }

            var importModel = new ImportRecipe();

            try
            {
                importModel.UserId = IdentityHelper.GetUserId(User);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            if (dto.CheckIfReviewRequired && _recipeService.IngredientsReviewIsRequired(dto.Id, importModel.UserId))
            {
                return Ok(0);
            }

            importModel.Id = dto.Id;
            importModel.IngredientReplacements = dto.IngredientReplacements;

            // Copy recipe image if not default
            RecipeToNotify recipe = await _recipeService.GetAsync(importModel.Id);
            string tempImagePath = Path.Combine(_webHostEnvironment.ContentRootPath, "storage", "temp", Guid.NewGuid().ToString());

            importModel.ImageUri = await _cdnService.CopyAndUploadAsync(
                tempImagePath: tempImagePath,
                imageUriToCopy: recipe.ImageUri,
                uploadPath: $"users/{importModel.UserId}/recipes",
                template: "recipe"
            );

            int id = await _recipeService.ImportAsync(importModel, _importRecipeValidator);

            // Notify
            User currentUser = await _userService.GetAsync(importModel.UserId);
            User recipeUser = await _userService.GetAsync(recipe.UserId);

            CultureInfo.CurrentCulture = new CultureInfo(recipeUser.Language, false);
            var message = _localizer["AcceptedSendRequestNotification", IdentityHelper.GetUserName(User), recipe.Name];

            var pushNotification = new PushNotification
            {
                SenderImageUri = currentUser.ImageUri,
                UserId = recipeUser.Id,
                Application = "Cooking Assistant",
                Message = message
            };

            _senderService.Enqueue(pushNotification);

            return StatusCode(201, id);
        }
    }
}