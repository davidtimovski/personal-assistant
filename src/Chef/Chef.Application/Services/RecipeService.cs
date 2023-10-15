using System.Text.RegularExpressions;
using AutoMapper;
using Chef.Application.Contracts.DietaryProfiles;
using Chef.Application.Contracts.Recipes;
using Chef.Application.Contracts.Recipes.Models;
using Chef.Application.Entities;
using Chef.Utility;
using Core.Application.Contracts;
using Core.Application.Contracts.Models;
using Core.Application.Entities;
using Core.Application.Utils;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Sentry;

namespace Chef.Application.Services;

public class RecipeService : IRecipeService
{
    private readonly IDietaryProfileService _dietaryProfileService;
    private readonly IConversion _conversion;
    private readonly ICdnService _cdnService;
    private readonly IUserService _userService;
    private readonly IRecipesRepository _recipesRepository;
    private readonly ICurrenciesRepository _currenciesRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<RecipeService> _logger;

    public RecipeService(
        IDietaryProfileService dietaryProfileService,
        IConversion conversion,
        ICdnService cdnService,
        IUserService userService,
        IRecipesRepository recipesRepository,
        ICurrenciesRepository currenciesRepository,
        IMapper mapper,
        ILogger<RecipeService> logger)
    {
        _dietaryProfileService = dietaryProfileService;
        _conversion = conversion;
        _cdnService = cdnService;
        _userService = userService;
        _recipesRepository = recipesRepository;
        _currenciesRepository = currenciesRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public IEnumerable<SimpleRecipe> GetAll(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(GetAll)}");

        try
        {
            List<Recipe> recipes = _recipesRepository.GetAll(userId, metric).ToList();

            var result = new List<SimpleRecipe>(recipes.Count);
            foreach (Recipe recipe in recipes)
            {
                var simpleRecipe = _mapper.Map<SimpleRecipe>(recipe, opts => opts.Items["UserId"] = userId);
                simpleRecipe.SharingState = GetSharingState(recipe, userId);

                result.Add(simpleRecipe);
            }

            return result.OrderBy(x => x.IngredientsMissing).ThenByDescending(x => x.LastOpenedDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetAll)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public RecipeToNotify Get(int id, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(Get)}");

        try
        {
            Recipe recipe = _recipesRepository.Get(id, metric);

            var result = _mapper.Map<RecipeToNotify>(recipe);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Get)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public RecipeDto? Get(int id, int userId, string currency, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(Get)}");

        try
        {
            Recipe? recipe = _recipesRepository.Get(id, userId, metric);
            if (recipe is null)
            {
                return null;
            }

            var result = _mapper.Map<RecipeDto>(recipe);

            result.NutritionSummary = _dietaryProfileService.CalculateNutritionSummary(recipe, metric);
            result.CostSummary = CalculateCostSummary(recipe, currency);
            result.SharingState = GetSharingState(recipe, userId);

            foreach (RecipeIngredientDto ingredient in result.Ingredients.Where(x => x.Amount.HasValue))
            {
                ingredient.AmountPerServing = ingredient.Amount / result.Servings;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Get)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public RecipeForUpdate? GetForUpdate(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(GetForUpdate)}");

        try
        {
            Recipe? recipe = _recipesRepository.GetForUpdate(id, userId, metric);
            if (recipe is null)
            {
                return null;
            }

            var result = _mapper.Map<RecipeForUpdate>(recipe);

            result.SharingState = GetSharingState(recipe, userId);
            result.UserIsOwner = recipe.UserId == userId;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetForUpdate)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public RecipeWithShares? GetWithShares(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(GetWithShares)}");

        try
        {
            Recipe? recipe = _recipesRepository.GetWithOwner(id, userId, metric);
            if (recipe is null)
            {
                return null;
            }

            recipe.Shares.AddRange(_recipesRepository.GetShares(id, metric));

            var result = _mapper.Map<RecipeWithShares>(recipe, opts => opts.Items["UserId"] = userId);
            result.Shares.RemoveAll(x => x.UserId == userId);

            result.SharingState = GetSharingState(recipe, userId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetWithShares)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public IEnumerable<ShareRecipeRequest> GetShareRequests(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(GetShareRequests)}");

        try
        {
            IEnumerable<RecipeShare> shareRequests = _recipesRepository.GetShareRequests(userId, metric);

            var result = shareRequests.Select(x => _mapper.Map<ShareRecipeRequest>(x, opts => opts.Items["UserId"] = userId));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetShareRequests)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public int GetPendingShareRequestsCount(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(GetPendingShareRequestsCount)}");

        try
        {
            return _recipesRepository.GetPendingShareRequestsCount(userId, metric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetPendingShareRequestsCount)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public bool CanShareWithUser(int shareWithId, int userId, ISpan metricsSpan)
    {
        if (shareWithId == userId)
        {
            return false;
        }

        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(CanShareWithUser)}");

        try
        {
            return _recipesRepository.CanShareWithUser(shareWithId, userId, metric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CanShareWithUser)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public RecipeForSending GetForSending(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(GetForSending)}");

        try
        {
            Recipe recipe = _recipesRepository.GetForSending(id, userId, metric);

            var result = _mapper.Map<RecipeForSending>(recipe);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetForSending)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public IEnumerable<SendRequestDto> GetSendRequests(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(GetSendRequests)}");

        try
        {
            IEnumerable<SendRequest> sendRequests = _recipesRepository.GetSendRequests(userId, metric);

            var result = sendRequests.Select(x => _mapper.Map<SendRequestDto>(x));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetSendRequests)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public int GetPendingSendRequestsCount(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(GetPendingSendRequestsCount)}");

        try
        {
            return _recipesRepository.GetPendingSendRequestsCount(userId, metric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetPendingSendRequestsCount)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public bool SendRequestExists(int id, int userId)
    {
        try
        {
            return _recipesRepository.SendRequestExists(id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(SendRequestExists)}");
            throw;
        }
    }

    public bool IngredientsReviewIsRequired(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(IngredientsReviewIsRequired)}");

        try
        {
            return _recipesRepository.IngredientsReviewIsRequired(id, userId, metric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(IngredientsReviewIsRequired)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public RecipeForReview? GetForReview(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(GetForReview)}");

        try
        {
            if (!SendRequestExists(id, userId))
            {
                return null;
            }

            Recipe? recipe = _recipesRepository.GetForReview(id, metric);

            var result = _mapper.Map<RecipeForReview>(recipe);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetForReview)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public IEnumerable<string> GetAllImageUris(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(GetAllImageUris)}");

        try
        {
            return _recipesRepository.GetAllImageUris(userId, metric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(GetAllImageUris)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public bool Exists(int id, int userId)
    {
        try
        {
            return _recipesRepository.Exists(id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Exists)}");
            throw;
        }
    }

    public bool Exists(string name, int userId)
    {
        try
        {
            return _recipesRepository.Exists(name.Trim(), userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Exists)}");
            throw;
        }
    }

    public bool Exists(int id, string name, int userId)
    {
        try
        {
            return _recipesRepository.Exists(id, name.Trim(), userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Exists)}");
            throw;
        }
    }

    public int Count(int userId)
    {
        try
        {
            return _recipesRepository.Count(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(Count)}");
            throw;
        }
    }

    public (bool canSend, bool alreadySent) CheckSendRequest(int recipeId, int sendToId, int userId, ISpan metricsSpan)
    {
        if (sendToId == userId)
        {
            return (false, false);
        }

        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(CheckSendRequest)}");

        try
        {
            return _recipesRepository.CheckSendRequest(recipeId, sendToId, userId, metric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CheckSendRequest)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public bool CheckIfUserCanBeNotifiedOfRecipeChange(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(CheckIfUserCanBeNotifiedOfRecipeChange)}");

        try
        {
            return _recipesRepository.CheckIfUserCanBeNotifiedOfRecipeChange(id, userId, metric);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CheckIfUserCanBeNotifiedOfRecipeChange)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<int> CreateAsync(CreateRecipe model, IValidator<CreateRecipe> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(CreateAsync)}");

        try
        {
            var now = DateTime.UtcNow;

            var recipe = _mapper.Map<Recipe>(model);
            recipe.Name = recipe.Name.Trim();

            if (!string.IsNullOrEmpty(recipe.Description))
            {
                recipe.Description = recipe.Description.Trim();
            }

            foreach (var recipeIngredient in recipe.RecipeIngredients)
            {
                recipeIngredient.Ingredient!.Name = recipeIngredient.Ingredient.Name.Trim();
                if (recipeIngredient.Amount.HasValue)
                {
                    if (recipeIngredient.Amount.Value == 0)
                    {
                        recipeIngredient.Amount = null;
                        recipeIngredient.Unit = null;
                    }
                    else if (recipeIngredient.Unit == "pinch")
                    {
                        recipeIngredient.Amount = null;
                    }
                }
                else if (recipeIngredient.Unit != "pinch")
                {
                    recipeIngredient.Unit = null;
                }

                recipeIngredient.CreatedDate = recipeIngredient.ModifiedDate = now;
                recipeIngredient.Ingredient.CreatedDate = recipeIngredient.Ingredient.ModifiedDate = now;
            }

            if (!string.IsNullOrEmpty(recipe.Instructions))
            {
                recipe.Instructions = Regex.Replace(recipe.Instructions, @"(?:\r\n|\r(?!\n)|(?<!\r)\n){2,}",
                    Environment.NewLine + Environment.NewLine).Trim();
            }

            var minute = TimeSpan.FromMinutes(1);
            recipe.PrepDuration = recipe.PrepDuration < minute ? null : recipe.PrepDuration;
            recipe.CookDuration = recipe.CookDuration < minute ? null : recipe.CookDuration;

            recipe.CreatedDate = recipe.ModifiedDate = recipe.LastOpenedDate = now;
            int id = await _recipesRepository.CreateAsync(recipe, metric, cancellationToken);

            if (model.ImageUri != null)
            {
                await _cdnService.RemoveTempTagAsync(model.ImageUri, metric, cancellationToken);
            }

            return id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task CreateSampleAsync(int userId, Dictionary<string, string> translations, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(CreateSampleAsync)}");

        try
        {
            var now = DateTime.UtcNow;

            var recipe = new Recipe
            {
                UserId = userId,
                Name = translations["SampleRecipeName"],
                Description = translations["SampleRecipeDescription"],
                Instructions = translations["SampleRecipeInstructions"],
                PrepDuration = TimeSpan.FromMinutes(10),
                CookDuration = TimeSpan.FromMinutes(15),
                Servings = 2,
                ImageUri = _cdnService.GetDefaultRecipeImageUri(),
                LastOpenedDate = now,
                CreatedDate = now,
                ModifiedDate = now
            };

            recipe.RecipeIngredients = new List<RecipeIngredient>
            {
                new RecipeIngredient
                {
                    Amount = 400,
                    Unit = "g",
                    CreatedDate = now,
                    ModifiedDate = now,
                    IngredientId = 606 // chicken_breast
                },
                new RecipeIngredient
                {
                    Amount = 3,
                    CreatedDate = now,
                    ModifiedDate = now,
                    IngredientId = 426 // potatoes
                },
                new RecipeIngredient
                {
                    Amount = 2,
                    CreatedDate = now,
                    ModifiedDate = now,
                    IngredientId = 427 // carrots
                }
            };

            await _recipesRepository.CreateAsync(recipe, metric, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CreateSampleAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<UpdateRecipeResult> UpdateAsync(UpdateRecipe model, IValidator<UpdateRecipe> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(UpdateAsync)}");

        try
        {
            var now = DateTime.UtcNow;

            string oldImageUri = _recipesRepository.GetImageUri(model.Id, metric);

            var recipe = _mapper.Map<Recipe>(model);
            recipe.Name = recipe.Name.Trim();

            if (!string.IsNullOrEmpty(recipe.Description))
            {
                recipe.Description = recipe.Description.Trim();
            }

            foreach (var recipeIngredient in recipe.RecipeIngredients)
            {
                recipeIngredient.Ingredient!.Name = recipeIngredient.Ingredient.Name.Trim();
                if (recipeIngredient.Amount.HasValue)
                {
                    if (recipeIngredient.Amount.Value == 0)
                    {
                        recipeIngredient.Amount = null;
                        recipeIngredient.Unit = null;
                    }
                    else if (recipeIngredient.Unit == "pinch")
                    {
                        recipeIngredient.Amount = null;
                    }
                }
                else if (recipeIngredient.Unit != "pinch")
                {
                    recipeIngredient.Unit = null;
                }

                recipeIngredient.RecipeId = recipe.Id;
                recipeIngredient.CreatedDate = recipeIngredient.ModifiedDate = now;
                recipeIngredient.Ingredient.CreatedDate = recipeIngredient.Ingredient.ModifiedDate = now;
            }

            if (!string.IsNullOrEmpty(recipe.Instructions))
            {
                recipe.Instructions = Regex.Replace(recipe.Instructions, @"(?:\r\n|\r(?!\n)|(?<!\r)\n){2,}",
                    Environment.NewLine + Environment.NewLine).Trim();
            }

            var minute = TimeSpan.FromMinutes(1);
            recipe.PrepDuration = recipe.PrepDuration < minute ? null : recipe.PrepDuration;
            recipe.CookDuration = recipe.CookDuration < minute ? null : recipe.CookDuration;

            recipe.ModifiedDate = now;

            string originalName = await _recipesRepository.UpdateAsync(recipe, model.UserId, metric, cancellationToken);

            // If the recipe image was changed
            if (oldImageUri != model.ImageUri)
            {
                // and it previously had one, delete it
                if (oldImageUri != null)
                {
                    await _cdnService.DeleteAsync(oldImageUri, metric, cancellationToken);
                }

                // and a new one was set, remove its temp tag
                if (model.ImageUri != null)
                {
                    await _cdnService.RemoveTempTagAsync(model.ImageUri, metric, cancellationToken);
                }
            }

            var usersToBeNotified = _recipesRepository.GetUsersToBeNotifiedOfRecipeChange(model.Id, model.UserId, metric).ToList();
            if (!usersToBeNotified.Any())
            {
                return new UpdateRecipeResult();
            }

            var userResult = _userService.Get(model.UserId);
            if (userResult.Failed)
            {
                throw new Exception("User retrieval failed");
            }

            var result = new UpdateRecipeResult
            {
                RecipeName = originalName,
                ActionUserName = userResult.Data!.Name,
                ActionUserImageUri = userResult.Data.ImageUri,
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient(x.Id, x.Language)).ToList()
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(UpdateAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<DeleteRecipeResult> DeleteAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(DeleteAsync)}");

        try
        {
            if (!_recipesRepository.UserOwns(id, userId, metric))
            {
                metric.Status = SpanStatus.PermissionDenied;
                throw new ValidationException("Unauthorized");
            }

            string imageUri = _recipesRepository.GetImageUri(id, metric);

            var recipeName = await _recipesRepository.DeleteAsync(id, metric, cancellationToken);

            if (imageUri != null)
            {
                await _cdnService.DeleteAsync($"users/{userId}/recipes/{imageUri}", metric, cancellationToken);
            }

            var usersToBeNotified = _recipesRepository.GetUsersToBeNotifiedOfRecipeDeletion(id, metric).ToList();
            if (!usersToBeNotified.Any())
            {
                return new DeleteRecipeResult();
            }

            var userResult = _userService.Get(userId);
            if (userResult.Failed)
            {
                throw new Exception("User retrieval failed");
            }

            var result = new DeleteRecipeResult
            {
                RecipeName = recipeName,
                ActionUserName = userResult.Data!.Name,
                ActionUserImageUri = userResult.Data.ImageUri,
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient(x.Id, x.Language)).ToList()
            };

            return result;
        }
        catch (Exception ex) when (ex is not ValidationException)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task ShareAsync(ShareRecipe model, IValidator<ShareRecipe> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(ShareAsync)}");

        try
        {
            var now = DateTime.UtcNow;

            var newShares = new List<RecipeShare>();
            foreach (int userId in model.NewShares)
            {
                if (_recipesRepository.UserHasBlockedSharing(model.RecipeId, model.UserId, userId, metric))
                {
                    continue;
                }

                newShares.Add(new RecipeShare
                {
                    RecipeId = model.RecipeId,
                    UserId = userId,
                    LastOpenedDate = now,
                    CreatedDate = now,
                    ModifiedDate = now
                });
            }

            var removedShares = model.RemovedShares.Select(x => new RecipeShare
            {
                RecipeId = model.RecipeId,
                UserId = x
            });

            await _recipesRepository.SaveSharingDetailsAsync(newShares, removedShares, metric, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(ShareAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<SetShareIsAcceptedResult> SetShareIsAcceptedAsync(int recipeId, int userId, bool isAccepted, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(SetShareIsAcceptedAsync)}");

        try
        {
            await _recipesRepository.SetShareIsAcceptedAsync(recipeId, userId, isAccepted, DateTime.UtcNow, metric, cancellationToken);

            var usersToBeNotified = _recipesRepository.GetUsersToBeNotifiedOfRecipeChange(recipeId, userId, metric).ToList();
            if (!usersToBeNotified.Any())
            {
                return new SetShareIsAcceptedResult();
            }

            Recipe recipe = _recipesRepository.Get(recipeId, metric);

            var userResult = _userService.Get(userId);
            if (userResult.Failed)
            {
                throw new Exception("User retrieval failed");
            }

            var result = new SetShareIsAcceptedResult
            {
                RecipeName = recipe.Name,
                ActionUserName = userResult.Data!.Name,
                ActionUserImageUri = userResult.Data.ImageUri,
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient(x.Id, x.Language)).ToList()
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(SetShareIsAcceptedAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<LeaveRecipeResult> LeaveAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(LeaveAsync)}");

        try
        {
            RecipeShare share = await _recipesRepository.LeaveAsync(id, userId, metric, cancellationToken);

            if (share.IsAccepted == false)
            {
                return new LeaveRecipeResult();
            }

            var usersToBeNotified = _recipesRepository.GetUsersToBeNotifiedOfRecipeChange(id, userId, metric).ToList();
            if (!usersToBeNotified.Any())
            {
                return new LeaveRecipeResult();
            }

            Recipe recipe = _recipesRepository.Get(id, metric);

            var userResult = _userService.Get(userId);
            if (userResult.Failed)
            {
                throw new Exception("User retrieval failed");
            }

            var result = new LeaveRecipeResult
            {
                RecipeName = recipe.Name,
                ActionUserName = userResult.Data!.Name,
                ActionUserImageUri = userResult.Data.ImageUri,
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient(x.Id, x.Language)).ToList()
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(LeaveAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<SendRecipeResult> SendAsync(CreateSendRequest model, IValidator<CreateSendRequest> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(SendAsync)}");

        try
        {
            var sendRequests = new List<SendRequest>();
            foreach (int recipientId in model.RecipientsIds)
            {
                var (canSend, alreadySent) = CheckSendRequest(model.RecipeId, recipientId, model.UserId, metric);
                if (!canSend || alreadySent)
                {
                    continue;
                }

                var recipeSendRequest = new SendRequest
                {
                    UserId = recipientId,
                    RecipeId = model.RecipeId
                };
                sendRequests.Add(recipeSendRequest);
            }

            var now = DateTime.UtcNow;
            foreach (SendRequest request in sendRequests)
            {
                request.CreatedDate = request.ModifiedDate = now;
            }

            await _recipesRepository.CreateSendRequestsAsync(sendRequests, metric, cancellationToken);

            var usersToBeNotified = _recipesRepository.GetUsersToBeNotifiedOfRecipeSent(model.RecipeId, metric).ToList();
            if (!usersToBeNotified.Any())
            {
                return new SendRecipeResult();
            }

            Recipe recipe = _recipesRepository.Get(model.RecipeId, metric);

            var userResult = _userService.Get(model.UserId);
            if (userResult.Failed)
            {
                throw new Exception("User retrieval failed");
            }

            var result = new SendRecipeResult
            {
                RecipeName = recipe.Name,
                ActionUserName = userResult.Data!.Name,
                ActionUserImageUri = userResult.Data.ImageUri,
                NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient(x.Id, x.Language)).ToList()
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(SendAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<DeclineSendRequestResult> DeclineSendRequestAsync(int recipeId, int userId, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(DeclineSendRequestAsync)}");

        try
        {
            await _recipesRepository.DeclineSendRequestAsync(recipeId, userId, DateTime.UtcNow, metric, cancellationToken);

            Recipe recipe = _recipesRepository.Get(recipeId, metric);

            var userToBeNotifiedResult = _userService.Get(recipe.UserId);
            if (userToBeNotifiedResult.Failed)
            {
                throw new Exception("User retrieval failed");
            }

            var userResult = _userService.Get(userId);
            if (userResult.Failed)
            {
                throw new Exception("User retrieval failed");
            }

            var result = new DeclineSendRequestResult
            {
                RecipeName = recipe.Name,
                ActionUserName = userResult.Data!.Name,
                ActionUserImageUri = userResult.Data.ImageUri,
                NotificationRecipients = new List<NotificationRecipient> { new NotificationRecipient(userToBeNotifiedResult.Data!.Id, userToBeNotifiedResult.Data.Language) }
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeclineSendRequestAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task DeleteSendRequestAsync(int recipeId, int userId, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(DeleteSendRequestAsync)}");

        try
        {
            await _recipesRepository.DeleteSendRequestAsync(recipeId, userId, metric, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(DeleteSendRequestAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task<int> ImportAsync(ImportRecipe model, IValidator<ImportRecipe> validator, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        ValidationUtil.ValidOrThrow(model, validator);

        var metric = metricsSpan.StartChild($"{nameof(RecipeService)}.{nameof(ImportAsync)}");

        try
        {
            var ingredientReplacements = model.IngredientReplacements
                .Select(x => (x.Id, x.ReplacementId, x.TransferNutritionData, x.TransferPriceData));

            return await _recipesRepository.ImportAsync(model.Id, ingredientReplacements, model.ImageUri, model.UserId, metric, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(ImportAsync)}");
            throw;
        }
        finally
        {
            metric.Finish();
        }
    }

    private RecipeSharingState GetSharingState(Recipe recipe, int userId)
    {
        if (recipe.Shares.Any())
        {
            bool someRequestsAccepted = recipe.Shares.Any(x => x.IsAccepted == true);
            if (someRequestsAccepted)
            {
                if (recipe.UserId == userId)
                {
                    return RecipeSharingState.Owner;
                }

                return RecipeSharingState.Member;
            }

            bool someRequestsPending = recipe.Shares.Any(x => !x.IsAccepted.HasValue);
            if (someRequestsPending)
            {
                return RecipeSharingState.PendingShare;
            }
        }

        return RecipeSharingState.NotShared;
    }

    private RecipeCostSummary CalculateCostSummary(Recipe recipe, string currency)
    {
        decimal? AddPricePerAmount(decimal? currentValue, decimal priceInGrams, short productSizeGrams, bool productSizeIsOneUnit, float amount, string? unit)
        {
            if (productSizeIsOneUnit)
            {
                return priceInGrams * (decimal)amount + (currentValue ?? 0);
            }

            float amountInGrams = _conversion.ToGrams(unit, amount);
            decimal valuePerGram = priceInGrams / productSizeGrams;
            return valuePerGram * (decimal)amountInGrams + (currentValue ?? 0);
        }

        try
        {
            RecipeIngredient[] validRecipeIngredients = recipe.RecipeIngredients
                .Where(x => x.Amount.HasValue
                            && x.Ingredient!.Price.HasValue
                            && (x.Ingredient.ProductSizeIsOneUnit && x.Unit is null || !x.Ingredient.ProductSizeIsOneUnit && x.Unit != null))
                .ToArray();

            var costSummary = new RecipeCostSummary();

            foreach (var recipeIngredient in validRecipeIngredients)
            {
                short productSize = recipeIngredient.Ingredient!.ProductSize;
                bool productSizeIsOneUnit = recipeIngredient.Ingredient.ProductSizeIsOneUnit;
                float amount = recipeIngredient.Amount!.Value;
                string? unit = recipeIngredient.Unit;

                decimal price = _currenciesRepository.Convert(recipeIngredient.Ingredient!.Price!.Value, recipeIngredient.Ingredient!.Currency!, currency, DateTime.UtcNow.Date);

                costSummary.Cost = AddPricePerAmount(costSummary.Cost, price, productSize, productSizeIsOneUnit, amount, unit);
            }

            if (costSummary.Cost.HasValue)
            {
                costSummary.IsSet = true;
                costSummary.CostPerServing = costSummary.Cost.Value / recipe.Servings;
            }

            return costSummary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error in {nameof(CalculateCostSummary)}");
            throw;
        }
    }
}
