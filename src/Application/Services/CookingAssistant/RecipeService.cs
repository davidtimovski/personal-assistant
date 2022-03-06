using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Application.Contracts.Common;
using Application.Contracts.Common.Models;
using Application.Contracts.CookingAssistant.DietaryProfiles;
using Application.Contracts.CookingAssistant.Recipes;
using Application.Contracts.CookingAssistant.Recipes.Models;
using AutoMapper;
using Domain.Entities.CookingAssistant;
using FluentValidation;
using FluentValidation.Results;
using Utility;

namespace Application.Services.CookingAssistant;

public class RecipeService : IRecipeService
{
    private readonly IDietaryProfileService _dietaryProfileService;
    private readonly IConversion _conversion;
    private readonly ICurrencyService _currencyService;
    private readonly ICdnService _cdnService;
    private readonly IUserService _userService;
    private readonly IRecipesRepository _recipesRepository;
    private readonly IMapper _mapper;

    public RecipeService(
        IDietaryProfileService dietaryProfileService,
        IConversion conversion,
        ICurrencyService currencyService,
        ICdnService cdnService,
        IUserService userService,
        IRecipesRepository recipesRepository,
        IMapper mapper)
    {
        _dietaryProfileService = dietaryProfileService;
        _conversion = conversion;
        _currencyService = currencyService;
        _cdnService = cdnService;
        _userService = userService;
        _recipesRepository = recipesRepository;
        _mapper = mapper;
    }

    public IEnumerable<SimpleRecipe> GetAll(int userId)
    {
        List<Recipe> recipes = _recipesRepository.GetAll(userId).ToList();

        var result = new List<SimpleRecipe>(recipes.Count);
        foreach (Recipe recipe in recipes)
        {
            var simpleRecipe = _mapper.Map<SimpleRecipe>(recipe, opts => { opts.Items["UserId"] = userId; });
            simpleRecipe.SharingState = GetSharingState(recipe, userId);

            result.Add(simpleRecipe);
        }

        return result.OrderBy(x => x.IngredientsMissing).ThenByDescending(x => x.LastOpenedDate);
    }

    public RecipeToNotify Get(int id)
    {
        Recipe recipe = _recipesRepository.Get(id);

        var result = _mapper.Map<RecipeToNotify>(recipe);

        return result;
    }

    public RecipeDto Get(int id, int userId, string currency)
    {
        Recipe recipe = _recipesRepository.Get(id, userId);

        var result = _mapper.Map<RecipeDto>(recipe);

        result.NutritionSummary = _dietaryProfileService.CalculateNutritionSummary(recipe);
        result.CostSummary = CalculateCostSummary(recipe, currency);
        result.SharingState = GetSharingState(recipe, userId);

        foreach (RecipeIngredientDto ingredient in result.Ingredients.Where(x => x.Amount.HasValue))
        {
            ingredient.AmountPerServing = ingredient.Amount / result.Servings;
        }

        return result;
    }

    public RecipeForUpdate GetForUpdate(int id, int userId)
    {
        Recipe recipe = _recipesRepository.GetForUpdate(id, userId);

        var result = _mapper.Map<RecipeForUpdate>(recipe);

        result.SharingState = GetSharingState(recipe, userId);
        result.UserIsOwner = recipe.UserId == userId;

        return result;
    }

    public RecipeWithShares GetWithShares(int id, int userId)
    {
        Recipe recipe = _recipesRepository.GetWithOwner(id, userId);
        if (recipe == null)
        {
            return null;
        }

        recipe.Shares.AddRange(_recipesRepository.GetShares(id));

        var result = _mapper.Map<RecipeWithShares>(recipe, opts => { opts.Items["UserId"] = userId; });
        result.Shares.RemoveAll(x => x.UserId == userId);

        result.SharingState = GetSharingState(recipe, userId);

        return result;
    }

    public IEnumerable<ShareRecipeRequest> GetShareRequests(int userId)
    {
        IEnumerable<RecipeShare> shareRequests = _recipesRepository.GetShareRequests(userId);

        var result = shareRequests.Select(x => _mapper.Map<ShareRecipeRequest>(x, opts => { opts.Items["UserId"] = userId; }));

        return result;
    }

    public int GetPendingShareRequestsCount(int userId)
    {
        return _recipesRepository.GetPendingShareRequestsCount(userId);
    }

    public bool CanShareWithUser(int shareWithId, int userId)
    {
        if (shareWithId == userId)
        {
            return false;
        }

        return _recipesRepository.CanShareWithUser(shareWithId, userId);
    }

    public RecipeForSending GetForSending(int id, int userId)
    {
        Recipe recipe = _recipesRepository.GetForSending(id, userId);

        var result = _mapper.Map<RecipeForSending>(recipe);

        return result;
    }

    public IEnumerable<SendRequestDto> GetSendRequests(int userId)
    {
        IEnumerable<SendRequest> sendRequests = _recipesRepository.GetSendRequests(userId);

        var result = sendRequests.Select(x => _mapper.Map<SendRequestDto>(x));

        return result;
    }

    public int GetPendingSendRequestsCount(int userId)
    {
        return _recipesRepository.GetPendingSendRequestsCount(userId);
    }

    public bool SendRequestExists(int id, int userId)
    {
        return _recipesRepository.SendRequestExists(id, userId);
    }

    public bool IngredientsReviewIsRequired(int id, int userId)
    {
        return _recipesRepository.IngredientsReviewIsRequired(id, userId);
    }

    public RecipeForReview GetForReview(int id, int userId)
    {
        if (!SendRequestExists(id, userId))
        {
            return null;
        }

        Recipe recipe = _recipesRepository.GetForReview(id);

        var result = _mapper.Map<RecipeForReview>(recipe);

        return result;
    }

    public IEnumerable<string> GetAllImageUris(int userId)
    {
        return _recipesRepository.GetAllImageUris(userId);
    }
        
    public bool Exists(int id, int userId)
    {
        return _recipesRepository.Exists(id, userId);
    }

    public bool Exists(string name, int userId)
    {
        return _recipesRepository.Exists(name.Trim(), userId);
    }

    public bool Exists(int id, string name, int userId)
    {
        return _recipesRepository.Exists(id, name.Trim(), userId);
    }

    public int Count(int userId)
    {
        return _recipesRepository.Count(userId);
    }

    public (bool canSend, bool alreadySent) CheckSendRequest(int recipeId, int sendToId, int userId)
    {
        if (sendToId == userId)
        {
            return (false, false);
        }

        return _recipesRepository.CheckSendRequest(recipeId, sendToId, userId);
    }

    public bool CheckIfUserCanBeNotifiedOfRecipeChange(int id, int userId)
    {
        return _recipesRepository.CheckIfUserCanBeNotifiedOfRecipeChange(id, userId);
    }

    public async Task<int> CreateAsync(CreateRecipe model, IValidator<CreateRecipe> validator)
    {
        ValidateAndThrow(model, validator);

        var recipe = _mapper.Map<Recipe>(model);

        var now = DateTime.UtcNow;

        recipe.Name = recipe.Name.Trim();

        if (!string.IsNullOrEmpty(recipe.Description))
        {
            recipe.Description = recipe.Description.Trim();
        }

        foreach (var recipeIngredient in recipe.RecipeIngredients)
        {
            recipeIngredient.Ingredient.Name = recipeIngredient.Ingredient.TaskId.HasValue ? null : recipeIngredient.Ingredient.Name.Trim();
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
            else
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
        int id = await _recipesRepository.CreateAsync(recipe);

        if (model.ImageUri != null)
        {
            await _cdnService.RemoveTempTagAsync(model.ImageUri);
        }

        return id;
    }

    public async Task CreateSampleAsync(int userId, Dictionary<string, string> translations)
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
                Amount = 300,
                Unit = "g",
                CreatedDate = recipe.CreatedDate,
                ModifiedDate = recipe.CreatedDate,
                Ingredient = new Ingredient
                {
                    UserId = userId,
                    Name = translations["SampleRecipeIngredient1"],
                    CreatedDate = recipe.CreatedDate,
                    ModifiedDate = recipe.CreatedDate
                }
            },
            new RecipeIngredient
            {
                Amount = 300,
                Unit = "g",
                CreatedDate = recipe.CreatedDate,
                ModifiedDate = recipe.CreatedDate,
                Ingredient = new Ingredient
                {
                    UserId = userId,
                    Name = translations["SampleRecipeIngredient2"],
                    CreatedDate = recipe.CreatedDate,
                    ModifiedDate = recipe.CreatedDate
                }
            },
            new RecipeIngredient
            {
                Amount = 100,
                Unit = "g",
                CreatedDate = recipe.CreatedDate,
                ModifiedDate = recipe.CreatedDate,
                Ingredient = new Ingredient
                {
                    UserId = userId,
                    Name = translations["SampleRecipeIngredient3"],
                    CreatedDate = recipe.CreatedDate,
                    ModifiedDate = recipe.CreatedDate
                }
            }
        };

        await _recipesRepository.CreateAsync(recipe);
    }

    public async Task<UpdateRecipeResult> UpdateAsync(UpdateRecipe model, IValidator<UpdateRecipe> validator)
    {
        ValidateAndThrow(model, validator);

        string oldImageUri = GetImageUri(model.Id);

        var recipe = _mapper.Map<Recipe>(model);

        var now = DateTime.UtcNow;

        recipe.Name = recipe.Name.Trim();

        if (!string.IsNullOrEmpty(recipe.Description))
        {
            recipe.Description = recipe.Description.Trim();
        }

        foreach (var recipeIngredient in recipe.RecipeIngredients)
        {
            recipeIngredient.Ingredient.Name = recipeIngredient.Ingredient.TaskId.HasValue ? null : recipeIngredient.Ingredient.Name.Trim();
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
            else
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

        string originalName = await _recipesRepository.UpdateAsync(recipe, model.UserId);

        // If the recipe image was changed
        if (oldImageUri != model.ImageUri)
        {
            // and it previously had one, delete it
            if (oldImageUri != null)
            {
                await _cdnService.DeleteAsync(oldImageUri);
            }

            // and a new one was set, remove its temp tag
            if (model.ImageUri != null)
            {
                await _cdnService.RemoveTempTagAsync(model.ImageUri);
            }
        }

        var usersToBeNotified = _recipesRepository.GetUsersToBeNotifiedOfRecipeChange(model.Id, model.UserId).ToList();
        if (!usersToBeNotified.Any())
        {
            return new UpdateRecipeResult();
        }

        var result = new UpdateRecipeResult
        {
            RecipeName = originalName,
            ActionUserImageUri = _userService.GetImageUri(model.UserId),
            NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
        };

        return result;
    }

    public async Task<DeleteRecipeResult> DeleteAsync(int id, int userId)
    {
        if (!_recipesRepository.UserOwns(id, userId))
        {
            throw new ValidationException("Unauthorized");
        }

        string imageUri = GetImageUri(id);

        var recipeName = await _recipesRepository.DeleteAsync(id);

        if (imageUri != null)
        {
            await _cdnService.DeleteAsync($"users/{userId}/recipes/{imageUri}");
        }

        var usersToBeNotified = _recipesRepository.GetUsersToBeNotifiedOfRecipeDeletion(id).ToList();;
        if (!usersToBeNotified.Any())
        {
            return new DeleteRecipeResult();
        }

        var result = new DeleteRecipeResult
        {
            RecipeName = recipeName,
            ActionUserImageUri = _userService.GetImageUri(userId),
            NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
        };

        return result;
    }

    public async Task ShareAsync(ShareRecipe model, IValidator<ShareRecipe> validator)
    {
        ValidateAndThrow(model, validator);

        var now = DateTime.UtcNow;

        var newShares = new List<RecipeShare>();
        foreach (int userId in model.NewShares)
        {
            if (_recipesRepository.UserHasBlockedSharing(model.RecipeId, model.UserId, userId))
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

        await _recipesRepository.SaveSharingDetailsAsync(newShares, removedShares);
    }

    public async Task<SetShareIsAcceptedResult> SetShareIsAcceptedAsync(int recipeId, int userId, bool isAccepted)
    {
        await _recipesRepository.SetShareIsAcceptedAsync(recipeId, userId, isAccepted, DateTime.UtcNow);

        var usersToBeNotified = _recipesRepository.GetUsersToBeNotifiedOfRecipeChange(recipeId, userId).ToList();;
        if (!usersToBeNotified.Any())
        {
            return new SetShareIsAcceptedResult();
        }

        Recipe recipe = _recipesRepository.Get(recipeId);

        var result = new SetShareIsAcceptedResult
        {
            RecipeName = recipe.Name,
            ActionUserImageUri = _userService.GetImageUri(userId),
            NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
        };

        return result;
    }

    public async Task<LeaveRecipeResult> LeaveAsync(int id, int userId)
    {
        RecipeShare share = await _recipesRepository.LeaveAsync(id, userId);

        if (share.IsAccepted == false)
        {
            return new LeaveRecipeResult();
        }

        var usersToBeNotified = _recipesRepository.GetUsersToBeNotifiedOfRecipeChange(id, userId).ToList();;
        if (!usersToBeNotified.Any())
        {
            return new LeaveRecipeResult();
        }

        Recipe recipe = _recipesRepository.Get(id);

        var result = new LeaveRecipeResult
        {
            RecipeName = recipe.Name,
            ActionUserImageUri = _userService.GetImageUri(userId),
            NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
        };

        return result;
    }

    public async Task<SendRecipeResult> SendAsync(CreateSendRequest model, IValidator<CreateSendRequest> validator)
    {
        ValidateAndThrow(model, validator);

        var sendRequests = new List<SendRequest>();
        foreach (int recipientId in model.RecipientsIds)
        {
            var (canSend, alreadySent) = CheckSendRequest(model.RecipeId, recipientId, model.UserId);
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

        await _recipesRepository.CreateSendRequestsAsync(sendRequests);

        var usersToBeNotified = _recipesRepository.GetUsersToBeNotifiedOfRecipeSent(model.RecipeId).ToList();;
        if (!usersToBeNotified.Any())
        {
            return new SendRecipeResult();
        }

        Recipe recipe = _recipesRepository.Get(model.RecipeId);

        var result = new SendRecipeResult
        {
            RecipeName = recipe.Name,
            ActionUserImageUri = _userService.GetImageUri(model.UserId),
            NotificationRecipients = usersToBeNotified.Select(x => new NotificationRecipient { Id = x.Id, Language = x.Language })
        };

        return result;
    }

    public async Task<DeclineSendRequestResult> DeclineSendRequestAsync(int recipeId, int userId)
    {
        await _recipesRepository.DeclineSendRequestAsync(recipeId, userId, DateTime.UtcNow);

        Recipe recipe = _recipesRepository.Get(recipeId);
        var userToBeNotified = _userService.Get(recipe.UserId);
           
        var result = new DeclineSendRequestResult
        {
            RecipeName = recipe.Name,
            ActionUserImageUri = _userService.GetImageUri(userId),
            NotificationRecipients = new List<NotificationRecipient> { new NotificationRecipient { Id = userToBeNotified.Id, Language = userToBeNotified.Language } }
        };

        return result;
    }

    public async Task DeleteSendRequestAsync(int recipeId, int userId)
    {
        await _recipesRepository.DeleteSendRequestAsync(recipeId, userId);
    }

    public async Task<int> ImportAsync(ImportRecipe model, IValidator<ImportRecipe> validator)
    {
        ValidateAndThrow(model, validator);
            
        var ingredientReplacements = model.IngredientReplacements
            .Select(x => (x.Id, x.ReplacementId, x.TransferNutritionData, x.TransferPriceData));

        return await _recipesRepository.ImportAsync(model.Id, ingredientReplacements, model.ImageUri, model.UserId);
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
        decimal? AddPricePerAmount(decimal? currentValue, decimal priceInGrams, short productSizeGrams, bool productSizeIsOneUnit, float amount, string unit)
        {
            if (productSizeIsOneUnit)
            {
                return priceInGrams * (decimal)amount + (currentValue ?? 0);
            }

            float amountInGrams = _conversion.ToGrams(unit, amount);
            decimal valuePerGram = priceInGrams / productSizeGrams;
            return valuePerGram * (decimal)amountInGrams + (currentValue ?? 0);
        }

        RecipeIngredient[] validRecipeIngredients = recipe.RecipeIngredients
            .Where(x => x.Amount.HasValue
                        && x.Ingredient.Price.HasValue
                        && ((x.Ingredient.ProductSizeIsOneUnit && x.Unit == null) || (!x.Ingredient.ProductSizeIsOneUnit && x.Unit != null)))
            .ToArray();

        var costSummary = new RecipeCostSummary();

        foreach (var recipeIngredient in validRecipeIngredients)
        {
            costSummary.IngredientIds.Add(recipeIngredient.Ingredient.Id);

            short productSize = recipeIngredient.Ingredient.ProductSize;
            bool productSizeIsOneUnit = recipeIngredient.Ingredient.ProductSizeIsOneUnit;
            float amount = recipeIngredient.Amount.Value;
            string unit = recipeIngredient.Unit;

            decimal price = _currencyService.Convert(recipeIngredient.Ingredient.Price.Value, recipeIngredient.Ingredient.Currency, currency, DateTime.UtcNow.Date);

            costSummary.Cost = AddPricePerAmount(costSummary.Cost, price, productSize, productSizeIsOneUnit, amount, unit);
        }

        if (costSummary.Cost.HasValue)
        {
            costSummary.IsSet = true;
            costSummary.CostPerServing = costSummary.Cost.Value / recipe.Servings;
        }

        return costSummary;
    }
        
    private string GetImageUri(int id)
    {
        return _recipesRepository.GetImageUri(id);
    }

    private static void ValidateAndThrow<T>(T model, IValidator<T> validator)
    {
        ValidationResult result = validator.Validate(model);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}
