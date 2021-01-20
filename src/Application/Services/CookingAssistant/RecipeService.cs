using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using PersonalAssistant.Domain.Entities.CookingAssistant;

namespace PersonalAssistant.Application.Services.CookingAssistant
{
    public class RecipeService : IRecipeService
    {
        private readonly ITaskService _taskService;
        private readonly ICdnService _cdnService;
        private readonly IUserService _userService;
        private readonly IRecipesRepository _recipesRepository;
        private readonly IMapper _mapper;

        public RecipeService(
            ITaskService taskService,
            ICdnService cdnService,
            IUserService userService,
            IRecipesRepository recipesRepository,
            IMapper mapper)
        {
            _taskService = taskService;
            _cdnService = cdnService;
            _userService = userService;
            _recipesRepository = recipesRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SimpleRecipe>> GetAllAsync(int userId)
        {
            IEnumerable<Recipe> recipes = await _recipesRepository.GetAllAsync(userId);

            var result = recipes.Select(x => _mapper.Map<SimpleRecipe>(x));
            result = result.OrderBy(x => x.IngredientsMissing).ThenByDescending(x => x.LastOpenedDate);

            return result;
        }

        public async Task<RecipeToNotify> GetAsync(int id)
        {
            Recipe recipe = await _recipesRepository.GetAsync(id);

            var result = _mapper.Map<RecipeToNotify>(recipe);

            return result;
        }

        public async Task<RecipeDto> GetAsync(int id, int userId, string currency)
        {
            Recipe recipe = await _recipesRepository.GetAsync(id, userId);

            var result = _mapper.Map<RecipeDto>(recipe, opts => { opts.Items["Currency"] = currency; });

            return result;
        }

        public async Task<RecipeForUpdate> GetForUpdateAsync(int id, int userId)
        {
            Recipe recipe = await _recipesRepository.GetForUpdateAsync(id, userId);

            var result = _mapper.Map<RecipeForUpdate>(recipe);

            return result;
        }

        public async Task<RecipeForSending> GetForSendingAsync(int id, int userId)
        {
            Recipe recipe = await _recipesRepository.GetForSendingAsync(id, userId);

            var result = _mapper.Map<RecipeForSending>(recipe);

            return result;
        }

        public async Task<IEnumerable<SendRequestDto>> GetSendRequestsAsync(int userId)
        {
            IEnumerable<SendRequest> sendRequests = await _recipesRepository.GetSendRequestsAsync(userId);

            var result = sendRequests.Select(x => _mapper.Map<SendRequestDto>(x));

            return result;
        }

        public Task<int> GetPendingSendRequestsCountAsync(int userId)
        {
            return _recipesRepository.GetPendingSendRequestsCountAsync(userId);
        }

        public Task<bool> SendRequestExistsAsync(int id, int userId)
        {
            return _recipesRepository.SendRequestExistsAsync(id, userId);
        }

        public Task<bool> IngredientsReviewIsRequiredAsync(int id, int userId)
        {
            return _recipesRepository.IngredientsReviewIsRequiredAsync(id, userId);
        }

        public async Task<RecipeForReview> GetForReviewAsync(int id, int userId)
        {
            if (!await SendRequestExistsAsync(id, userId))
            {
                return null;
            }

            Recipe recipe = await _recipesRepository.GetForReviewAsync(id);

            var result = _mapper.Map<RecipeForReview>(recipe);

            return result;
        }

        public Task<IEnumerable<string>> GetAllImageUrisAsync(int userId)
        {
            return _recipesRepository.GetAllImageUrisAsync(userId);
        }

        public Task<string> GetImageUriAsync(int id)
        {
            return _recipesRepository.GetImageUriAsync(id);
        }

        public Task<bool> ExistsAsync(int id, int userId)
        {
            return _recipesRepository.ExistsAsync(id, userId);
        }

        public Task<bool> ExistsAsync(string name, int userId)
        {
            return _recipesRepository.ExistsAsync(name.Trim(), userId);
        }

        public Task<bool> ExistsAsync(int id, string name, int userId)
        {
            return _recipesRepository.ExistsAsync(id, name.Trim(), userId);
        }

        public Task<int> CountAsync(int userId)
        {
            return _recipesRepository.CountAsync(userId);
        }

        public async Task<(bool canSend, bool alreadySent)> CheckSendRequestAsync(int recipeId, int sendToId, int userId)
        {
            if (sendToId == userId)
            {
                return (false, false);
            }

            return await _recipesRepository.CheckSendRequestAsync(recipeId, sendToId, userId);
        }

        public async Task<int> CreateAsync(CreateRecipe model, IValidator<CreateRecipe> validator)
        {
            ValidateAndThrow(model, validator);

            var recipe = _mapper.Map<Recipe>(model);

            var now = DateTime.Now;

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

        public async Task UpdateAsync(UpdateRecipe model, IValidator<UpdateRecipe> validator)
        {
            ValidateAndThrow(model, validator);

            string oldImageUri = await GetImageUriAsync(model.Id);

            var recipe = _mapper.Map<Recipe>(model);

            var now = DateTime.Now;

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

            recipe.ModifiedDate = now;

            await _recipesRepository.UpdateAsync(recipe, model.IngredientIdsToRemove);

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
        }

        public async Task DeleteAsync(int id, int userId)
        {
            if (!await ExistsAsync(id, userId))
            {
                throw new ValidationException("Unauthorized");
            }

            string imageUri = await GetImageUriAsync(id);

            await _recipesRepository.DeleteAsync(id);

            if (imageUri != null)
            {
                await _cdnService.DeleteAsync($"users/{userId}/recipes/{imageUri}");
            }
        }

        public async Task SendAsync(CreateSendRequest model, IValidator<CreateSendRequest> validator)
        {
            ValidateAndThrow(model, validator);

            var sendRequests = new List<SendRequest>();
            foreach (int recipientId in model.RecipientsIds)
            {
                var (canSend, alreadySent) = await CheckSendRequestAsync(model.RecipeId, recipientId, model.UserId);
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

            var now = DateTime.Now;
            foreach (SendRequest request in sendRequests)
            {
                request.CreatedDate = request.ModifiedDate = now;
            }

            await _recipesRepository.CreateSendRequestsAsync(sendRequests);
        }

        public async Task DeclineSendRequestAsync(int id, int userId)
        {
            await _recipesRepository.DeclineSendRequestAsync(id, userId, DateTime.Now);
        }

        public async Task DeleteSendRequestAsync(int id, int userId)
        {
            await _recipesRepository.DeleteSendRequestAsync(id, userId);
        }

        public async Task<int> ImportAsync(ImportRecipe model, IValidator<ImportRecipe> validator)
        {
            var ingredientReplacements = model.IngredientReplacements
                .Select(x => (x.Id, x.ReplacementId, x.TransferNutritionData, x.TransferPriceData));

            return await _recipesRepository.ImportAsync(model.Id, ingredientReplacements, model.ImageUri, model.UserId);
        }

        private void ValidateAndThrow<T>(T model, IValidator<T> validator)
        {
            ValidationResult result = validator.Validate(model);
            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }
    }
}
