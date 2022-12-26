using System.Data;
using CookingAssistant.Application.Contracts.Recipes;
using Dapper;
using Application.Domain.Common;
using Application.Domain.CookingAssistant;
using Application.Domain.ToDoAssistant;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories.CookingAssistant;

public class RecipesRepository : BaseRepository, IRecipesRepository
{
    public RecipesRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public IEnumerable<Recipe> GetAll(int userId)
    {
        using IDbConnection conn = OpenConnection();

        var recipes = conn.Query<Recipe>(@"SELECT r.id, r.user_id, r.name, r.image_uri, r.last_opened_date, COUNT(t.id) AS ingredients_missing
                                           FROM cooking.recipes AS r
                                           LEFT JOIN cooking.recipes_ingredients AS ri ON r.id = ri.recipe_id
                                           LEFT JOIN cooking.ingredients AS i ON ri.ingredient_id = i.id
                                           LEFT JOIN cooking.ingredients_tasks AS it ON i.id = it.ingredient_id AND it.user_id = @UserId
                                           LEFT JOIN todo.tasks AS t ON it.task_id = t.id AND t.is_completed = FALSE
                                           LEFT JOIN cooking.shares AS s ON r.id = s.recipe_id
                                           WHERE r.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted)
                                           GROUP BY r.id, r.name", new { UserId = userId }).ToList();

        var recipeIds = recipes.Select(x => x.Id).ToArray();
        var shares = conn.Query<RecipeShare>(@"SELECT * FROM cooking.shares WHERE recipe_id = ANY(@RecipeIds)", new { RecipeIds = recipeIds });

        foreach (var recipe in recipes)
        {
            recipe.Shares = shares.Where(x => x.RecipeId == recipe.Id).ToList();
        }

        return recipes;
    }

    public Recipe Get(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<Recipe>(@"SELECT * FROM cooking.recipes WHERE id = @Id", new { Id = id });
    }

    public Recipe Get(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        const string recipeSql = @"SELECT r.*, u.id, dp.*
                                   FROM cooking.recipes AS r 
                                   INNER JOIN users AS u ON r.user_id = u.id
                                   LEFT JOIN cooking.dietary_profiles AS dp ON dp.user_id = @UserId
                                   LEFT JOIN cooking.shares AS s ON r.id = s.recipe_id
                                   WHERE r.id = @Id AND (r.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))";

        var recipes = conn.Query<Recipe, User, DietaryProfile, Recipe>(recipeSql,
            (dbRecipe, user, dietaryProfile) =>
            {
                user.DietaryProfile = dietaryProfile;
                dbRecipe.User = user;
                return dbRecipe;
            }, new { Id = id, UserId = userId }, null, true, "Id,user_id");

        if (!recipes.Any())
        {
            return null;
        }

        var recipe = recipes.First();

        if (recipe.UserId == userId)
        {
            conn.Execute(@"UPDATE cooking.recipes SET last_opened_date = @LastOpenedDate WHERE id = @Id", new { Id = id, LastOpenedDate = DateTime.UtcNow });
        }
        else
        {
            conn.Execute(@"UPDATE cooking.shares SET last_opened_date = @LastOpenedDate WHERE recipe_id = @Id", new { Id = id, LastOpenedDate = DateTime.UtcNow });
        }

        recipe.Shares = conn.Query<RecipeShare>(@"SELECT * FROM cooking.shares WHERE recipe_id = @Id", new { Id = id }).ToList();

        const string recipeIngredientsSql = @"SELECT ri.amount, ri.unit, i.*, it.task_id, t.id, t.is_completed
                                              FROM cooking.recipes_ingredients AS ri
                                              INNER JOIN cooking.ingredients AS i ON ri.ingredient_id = i.id
                                              LEFT JOIN cooking.ingredients_tasks AS it ON i.id = it.ingredient_id AND it.user_id = @UserId
                                              LEFT JOIN todo.tasks AS t ON it.task_id = t.id
                                              WHERE ri.recipe_id = @RecipeId";

        var recipeIngredients = conn.Query<RecipeIngredient, Ingredient, ToDoTask, RecipeIngredient>(recipeIngredientsSql,
            (recipeIngredient, ingredient, task) =>
            {
                if (task != null)
                {
                    ingredient.Task = task;
                }
                recipeIngredient.Ingredient = ingredient;
                return recipeIngredient;
            }, new { RecipeId = id, UserId = userId }, null, true, "id,id");

        recipe.RecipeIngredients.AddRange(recipeIngredients);

        return recipe;
    }

    public Recipe GetForUpdate(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        var recipe = conn.QueryFirstOrDefault<Recipe>(@"SELECT r.* 
                                                        FROM cooking.recipes AS r 
                                                        LEFT JOIN cooking.shares AS s ON r.id = s.recipe_id
                                                        WHERE id = @Id AND (r.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))",
            new { Id = id, UserId = userId });

        if (recipe != null)
        {
            recipe.Shares = conn.Query<RecipeShare>(@"SELECT * FROM cooking.shares WHERE recipe_id = @Id", new { Id = id }).ToList();

            const string recipeIngredientsSql = @"SELECT ri.amount, ri.unit, i.*
                                                  FROM cooking.recipes_ingredients AS ri
                                                  INNER JOIN cooking.ingredients AS i ON ri.ingredient_id = i.id
                                                  WHERE ri.recipe_id = @RecipeId";

            var recipeIngredients = conn.Query<RecipeIngredient, Ingredient, RecipeIngredient>(recipeIngredientsSql,
                (recipeIngredient, ingredient) =>
                {
                    recipeIngredient.Ingredient = ingredient;
                    return recipeIngredient;
                }, new { RecipeId = id, UserId = userId }, null, true, "id");

            recipe.RecipeIngredients.AddRange(recipeIngredients);
        }

        return recipe;
    }

    public Recipe GetWithOwner(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT DISTINCT r.*, u.id, u.email, u.image_uri
                               FROM cooking.recipes AS r
                               LEFT JOIN cooking.shares AS s ON r.id = s.recipe_id
                               INNER JOIN users AS u ON r.user_id = u.id
                               WHERE r.id = @Id AND (r.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))";

        return conn.Query<Recipe, User, Recipe>(query,
            (recipe, user) =>
            {
                recipe.User = user;
                return recipe;
            }, new { Id = id, UserId = userId }).FirstOrDefault();
    }

    public IEnumerable<RecipeShare> GetShares(int id)
    {
        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT s.*, u.id, u.email, u.image_uri
                               FROM cooking.shares AS s
                               INNER JOIN users AS u ON s.user_id = u.id
                               WHERE s.recipe_id = @RecipeId AND s.is_accepted IS NOT FALSE
                               ORDER BY (CASE WHEN s.is_accepted THEN 1 ELSE 2 END) ASC, s.created_date";

        return conn.Query<RecipeShare, User, RecipeShare>(query,
            (share, user) =>
            {
                share.User = user;
                return share;
            }, new { RecipeId = id });
    }

    public IEnumerable<RecipeShare> GetShareRequests(int userId)
    {
        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT s.*, r.name, u.name
                               FROM cooking.shares AS s
                               INNER JOIN cooking.recipes AS r ON s.recipe_id = r.id
                               INNER JOIN users AS u ON r.user_id = u.id
                               WHERE s.user_id = @UserId
                               ORDER BY s.modified_date DESC";

        return conn.Query<RecipeShare, Recipe, User, RecipeShare>(query,
            (share, recipe, user) =>
            {
                share.Recipe = recipe;
                share.User = user;
                return share;
            }, new { UserId = userId }, null, true, "name,Name");
    }

    public int GetPendingShareRequestsCount(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<int>(@"SELECT COUNT(*) FROM cooking.shares WHERE user_id = @UserId AND is_accepted IS NULL",
            new { UserId = userId });
    }

    public bool CanShareWithUser(int shareWithId, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return !conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                           FROM cooking.recipes AS r
                                           INNER JOIN cooking.shares AS s on r.id = s.recipe_id
                                           WHERE r.user_id = @UserId AND s.user_id = @ShareWithId AND s.is_accepted = FALSE",
            new { ShareWithId = shareWithId, UserId = userId });
    }

    public Recipe GetForSending(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<Recipe>(@"SELECT r.id, r.name
                                                  FROM cooking.recipes AS r
                                                  LEFT JOIN cooking.shares AS s on r.id = s.recipe_id
                                                  WHERE r.id = @Id AND (r.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))",
            new { Id = id, UserId = userId });
    }

    public IEnumerable<SendRequest> GetSendRequests(int userId)
    {
        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT sr.*, r.name, u.name
                               FROM cooking.send_requests AS sr
                               INNER JOIN cooking.recipes AS r ON sr.recipe_id = r.id
                               INNER JOIN users AS u ON r.user_id = u.id
                               WHERE sr.user_id = @UserId
                               ORDER BY sr.modified_date DESC";

        return conn.Query<SendRequest, Recipe, User, SendRequest>(query,
            (sendRequest, recipe, user) =>
            {
                sendRequest.Recipe = recipe;
                sendRequest.User = user;
                return sendRequest;
            }, new { UserId = userId }, null, true, "name,Name");
    }

    public int GetPendingSendRequestsCount(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<int>(@"SELECT COUNT(*) FROM cooking.send_requests WHERE user_id = @UserId AND is_declined = FALSE",
            new { UserId = userId });
    }

    public bool SendRequestExists(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM cooking.send_requests WHERE recipe_id = @RecipeId AND user_id = @UserId AND is_declined = FALSE",
            new { RecipeId = id, UserId = userId });
    }

    public bool IngredientsReviewIsRequired(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        var userThatsImportingHasIngredients = conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM cooking.ingredients WHERE user_id = @UserId", new { UserId = userId });

        var recipeHasCustomIngredients = conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                                                    FROM cooking.recipes_ingredients AS ri
                                                                    INNER JOIN cooking.ingredients AS i ON ri.ingredient_id = i.id
                                                                    WHERE recipe_id = @RecipeId AND user_id != 1", new { RecipeId = id });

        return userThatsImportingHasIngredients && recipeHasCustomIngredients;
    }

    public Recipe GetForReview(int id)
    {
        using IDbConnection conn = OpenConnection();

        var recipe = conn.QueryFirstOrDefault<Recipe>(@"SELECT id, user_id, name, description, image_uri FROM cooking.recipes WHERE id = @Id", new { Id = id });

        if (recipe == null)
        {
            return null;
        }

        const string recipeIngredientsSql = @"SELECT ri.ingredient_id, i.*
                                              FROM cooking.recipes_ingredients AS ri
                                              INNER JOIN cooking.ingredients AS i ON ri.ingredient_id = i.id
                                              WHERE ri.recipe_id = @RecipeId AND i.user_id = @UserId";

        var recipeIngredients = conn.Query<RecipeIngredient, Ingredient, RecipeIngredient>(recipeIngredientsSql,
            (recipeIngredient, ingredient) =>
            {
                recipeIngredient.Ingredient = ingredient;
                return recipeIngredient;
            }, new { RecipeId = id, recipe.UserId });

        recipe.RecipeIngredients.AddRange(recipeIngredients);

        return recipe;
    }

    public IEnumerable<string> GetAllImageUris(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<string>(@"SELECT image_uri FROM cooking.recipes WHERE user_id = @UserId",
            new { UserId = userId });
    }

    public string GetImageUri(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<string>(@"SELECT image_uri FROM cooking.recipes WHERE id = @Id",
            new { Id = id });
    }

    public bool UserOwns(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM cooking.recipes WHERE id = @Id AND user_id = @UserId",
            new { Id = id, UserId = userId });
    }

    public bool Exists(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM cooking.recipes AS r
                                          LEFT JOIN cooking.shares AS s ON r.id = s.recipe_id
                                          WHERE r.id = @Id AND (r.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))",
            new { Id = id, UserId = userId });
    }

    public bool Exists(string name, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM cooking.recipes AS r
                                          LEFT JOIN cooking.shares AS s ON r.id = s.recipe_id
                                          WHERE UPPER(r.name) = UPPER(@Name) AND (r.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))",
            new { Name = name, UserId = userId });
    }

    public bool Exists(int id, string name, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM cooking.recipes AS r
                                          LEFT JOIN cooking.shares AS s ON r.id = s.recipe_id
                                          WHERE r.id != @Id AND UPPER(r.name) = UPPER(@Name) AND (r.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted))",
            new { Id = id, Name = name, UserId = userId });
    }

    public int Count(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<int>(@"SELECT COUNT(*) FROM cooking.recipes WHERE user_id = @UserId", new { UserId = userId });
    }

    public bool UserHasBlockedSharing(int recipeId, int userId, int sharedWithId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM cooking.shares
                                          WHERE user_id = @SharedWithId AND is_accepted = FALSE AND (SELECT user_id FROM cooking.recipes WHERE id = @RecipeId) = @SharerId",
            new { SharedWithId = sharedWithId, SharerId = userId, RecipeId = recipeId });
    }

    public (bool canSend, bool alreadySent) CheckSendRequest(int recipeId, int sendToId, int userId)
    {
        using IDbConnection conn = OpenConnection();

        bool canSend = !conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                                   FROM cooking.send_requests
                                                   WHERE user_id = @SendToId AND is_declined = TRUE AND (SELECT user_id FROM cooking.recipes WHERE id = @RecipeId) = @SenderId",
            new { SendToId = sendToId, SenderId = userId, RecipeId = recipeId });

        bool alreadySent = conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                                      FROM cooking.send_requests
                                                      WHERE recipe_id = @RecipeId AND user_id = @SendToId",
            new { RecipeId = recipeId, SendToId = sendToId });

        return (canSend, alreadySent);
    }

    public IEnumerable<User> GetUsersToBeNotifiedOfRecipeChange(int id, int excludeUserId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<User>(@"SELECT u.*
                                  FROM users AS u
                                  INNER JOIN cooking.shares AS s ON u.id = s.user_id
                                  WHERE u.id != @ExcludeUserId AND s.recipe_id = @RecipeId AND s.is_accepted AND u.todo_notifications_enabled
                                  UNION
                                  SELECT u.*
                                  FROM users AS u
                                  INNER JOIN cooking.recipes AS r ON u.id = r.user_id
                                  WHERE u.id != @ExcludeUserId AND r.id = @RecipeId AND u.todo_notifications_enabled",
            new { RecipeId = id, ExcludeUserId = excludeUserId });
    }
    public bool CheckIfUserCanBeNotifiedOfRecipeChange(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM users AS u
                                          INNER JOIN cooking.shares AS s ON u.id = s.user_id
                                          WHERE u.id = @UserId AND s.recipe_id = @RecipeId AND s.is_accepted AND u.todo_notifications_enabled",
            new { RecipeId = id, UserId = userId });
    }

    public IEnumerable<User> GetUsersToBeNotifiedOfRecipeDeletion(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<User>(@"SELECT u.*
                                  FROM users AS u
                                  INNER JOIN cooking.shares AS s ON u.id = s.user_id
                                  WHERE s.recipe_id = @RecipeId AND s.is_accepted AND u.todo_notifications_enabled",
            new { RecipeId = id });
    }

    public IEnumerable<User> GetUsersToBeNotifiedOfRecipeSent(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<User>(@"SELECT u.*
                                  FROM users AS u
                                  INNER JOIN cooking.send_requests AS sr ON u.id = sr.user_id
                                  WHERE sr.recipe_id = @RecipeId AND u.cooking_notifications_enabled",
            new { RecipeId = id });
    }

    public async Task<int> CreateAsync(Recipe recipe)
    {
        var userAndPublicIngredients = EFContext.Ingredients.Where(x => x.UserId == recipe.UserId || x.UserId == 1).ToArray();

        var existingRecipeIngredients = recipe.RecipeIngredients.Where(x => x.IngredientId != 0).ToArray();
        foreach (var recipeIngredient in existingRecipeIngredients)
        {
            recipeIngredient.Ingredient = userAndPublicIngredients.First(x => x.Id == recipeIngredient.IngredientId);
            recipe.RecipeIngredients.Add(recipeIngredient);
        }

        var newRecipeIngredients = recipe.RecipeIngredients.Where(x => x.IngredientId == 0).ToArray();
        foreach (var newRecipeIngredient in newRecipeIngredients)
        {
            Ingredient existingUserIngredient = userAndPublicIngredients.FirstOrDefault(x =>
                x.UserId == recipe.UserId &&
                string.Equals(x.Name, newRecipeIngredient.Ingredient.Name, StringComparison.OrdinalIgnoreCase));

            if (existingUserIngredient == null)
            {
                newRecipeIngredient.Ingredient.UserId = recipe.UserId;
                newRecipeIngredient.Ingredient.CreatedDate = newRecipeIngredient.Ingredient.ModifiedDate = recipe.CreatedDate;
            }
            else
            {
                newRecipeIngredient.Ingredient = existingUserIngredient;
            }

            recipe.RecipeIngredients.Add(newRecipeIngredient);
        }

        EFContext.Add(recipe);

        await EFContext.SaveChangesAsync();

        return recipe.Id;
    }

    public async Task<string> UpdateAsync(Recipe recipe, int userId)
    {
        var dbRecipe = EFContext.Recipes.Include(x => x.RecipeIngredients).First(x => x.Id == recipe.Id);

        var originalRecipeName = dbRecipe.Name;

        dbRecipe.Name = recipe.Name;
        dbRecipe.Description = recipe.Description;
        dbRecipe.Instructions = recipe.Instructions;
        dbRecipe.PrepDuration = recipe.PrepDuration;
        dbRecipe.CookDuration = recipe.CookDuration;
        dbRecipe.Servings = recipe.Servings;
        dbRecipe.ImageUri = recipe.ImageUri;
        dbRecipe.VideoUrl = recipe.VideoUrl;
        dbRecipe.ModifiedDate = recipe.ModifiedDate;
        dbRecipe.RecipeIngredients.RemoveAll(x => true);

        IEnumerable<Ingredient> ownerAndPublicIngredients = EFContext.Ingredients.Where(x => x.UserId == dbRecipe.UserId || x.UserId == 1).ToList();

        var existingRecipeIngredients = recipe.RecipeIngredients.Where(x => x.IngredientId != 0).ToList();
        foreach (var recipeIngredient in existingRecipeIngredients)
        {
            recipeIngredient.Ingredient = ownerAndPublicIngredients.First(x => x.Id == recipeIngredient.IngredientId);
            dbRecipe.RecipeIngredients.Add(recipeIngredient);
        }

        var newRecipeIngredients = recipe.RecipeIngredients.Where(x => x.IngredientId == 0).ToList();
        foreach (var newRecipeIngredient in newRecipeIngredients)
        {
            Ingredient existingUserIngredient = ownerAndPublicIngredients.FirstOrDefault(x =>
                x.UserId == dbRecipe.UserId &&
                string.Equals(x.Name, newRecipeIngredient.Ingredient.Name, StringComparison.OrdinalIgnoreCase));

            if (existingUserIngredient == null)
            {
                newRecipeIngredient.Ingredient.UserId = dbRecipe.UserId;
                newRecipeIngredient.Ingredient.CreatedDate = newRecipeIngredient.Ingredient.ModifiedDate = recipe.ModifiedDate;
            }
            else
            {
                newRecipeIngredient.Ingredient = existingUserIngredient;
            }

            dbRecipe.RecipeIngredients.Add(newRecipeIngredient);
        }

        await EFContext.SaveChangesAsync();

        return originalRecipeName;
    }

    public async Task<string> DeleteAsync(int id)
    {
        Recipe recipe = EFContext.Recipes.Find(id);
        EFContext.Recipes.Remove(recipe);

        await EFContext.SaveChangesAsync();

        return recipe.Name;
    }

    public async Task SaveSharingDetailsAsync(IEnumerable<RecipeShare> newShares, IEnumerable<RecipeShare> removedShares)
    {
        using IDbConnection conn = OpenConnection();
        var transaction = conn.BeginTransaction();

        await conn.ExecuteAsync(@"DELETE FROM cooking.shares
                                  WHERE recipe_id = @RecipeId AND user_id = @UserId AND is_accepted IS DISTINCT FROM FALSE", removedShares, transaction);

        await conn.ExecuteAsync(@"INSERT INTO cooking.shares (recipe_id, user_id, last_opened_date, created_date, modified_date) 
                                  VALUES (@RecipeId, @UserId, @LastOpenedDate, @CreatedDate, @ModifiedDate)", newShares, transaction);

        transaction.Commit();
    }

    public async Task SetShareIsAcceptedAsync(int recipeId, int userId, bool isAccepted, DateTime modifiedDate)
    {
        RecipeShare recipeShare = EFContext.RecipeShares.First(x => x.RecipeId == recipeId && x.UserId == userId && x.IsAccepted == null);
        recipeShare.IsAccepted = isAccepted;
        recipeShare.ModifiedDate = modifiedDate;

        await EFContext.SaveChangesAsync();
    }

    public async Task<RecipeShare> LeaveAsync(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        var share = conn.QueryFirstOrDefault<RecipeShare>(@"SELECT * 
                                                            FROM cooking.shares 
                                                            WHERE recipe_id = @RecipeId AND user_id = @UserId",
            new { RecipeId = id, UserId = userId });

        RecipeShare recipeShare = EFContext.RecipeShares.First(x => x.RecipeId == id && x.UserId == userId);
        EFContext.RecipeShares.Remove(recipeShare);

        await EFContext.SaveChangesAsync();

        return share;
    }

    public async Task CreateSendRequestsAsync(IEnumerable<SendRequest> sendRequests)
    {
        EFContext.AddRange(sendRequests);

        await EFContext.SaveChangesAsync();
    }

    public async Task DeclineSendRequestAsync(int recipeId, int userId, DateTime modifiedDate)
    {
        SendRequest sendRequest = EFContext.SendRequests.First(x => x.RecipeId == recipeId && x.UserId == userId);
        sendRequest.IsDeclined = true;
        sendRequest.ModifiedDate = modifiedDate;

        await EFContext.SaveChangesAsync();
    }

    public async Task DeleteSendRequestAsync(int recipeId, int userId)
    {
        SendRequest sendRequest = EFContext.SendRequests.First(x => x.RecipeId == recipeId && x.UserId == userId);
        EFContext.SendRequests.Remove(sendRequest);

        await EFContext.SaveChangesAsync();
    }

    public async Task<int> ImportAsync(int id, IEnumerable<(int Id, int ReplacementId, bool TransferNutritionData, bool TransferPriceData)> ingredientReplacements, string imageUri, int userId)
    {
        var now = DateTime.UtcNow;

        var recipeToImport = EFContext.Recipes.Find(id);

        var recipe = new Recipe
        {
            UserId = userId,
            Name = CreatePostfixedNameIfDuplicate("recipes", recipeToImport.Name, userId),
            Description = recipeToImport.Description,
            Instructions = recipeToImport.Instructions,
            PrepDuration = recipeToImport.PrepDuration,
            CookDuration = recipeToImport.CookDuration,
            Servings = recipeToImport.Servings,
            ImageUri = imageUri,
            VideoUrl = recipeToImport.VideoUrl,
            LastOpenedDate = now,
            CreatedDate = now,
            ModifiedDate = now
        };

        recipe.RecipeIngredients.AddRange(EFContext.RecipesIngredients.Where(x => x.RecipeId == id).Select(x => new RecipeIngredient
        {
            IngredientId = x.IngredientId,
            Amount = x.Amount,
            Unit = x.Unit,
            CreatedDate = now,
            ModifiedDate = now
        }));

        foreach (var recipeIngredient in recipe.RecipeIngredients)
        {
            var replacement = ingredientReplacements.FirstOrDefault(x => x.Id == recipeIngredient.IngredientId);
            if (replacement == default)
            {
                var original = EFContext.Ingredients.Find(recipeIngredient.IngredientId);
                if (original.UserId == 1)
                {
                    recipeIngredient.Ingredient = original;
                    continue;
                }

                recipeIngredient.Ingredient = new Ingredient
                {
                    UserId = userId,
                    Name = CreatePostfixedNameIfDuplicate("ingredients", original.Name, userId),
                    ServingSize = original.ServingSize,
                    ServingSizeIsOneUnit = original.ServingSizeIsOneUnit,
                    Calories = original.Calories,
                    Fat = original.Fat,
                    SaturatedFat = original.SaturatedFat,
                    Carbohydrate = original.Carbohydrate,
                    Sugars = original.Sugars,
                    AddedSugars = original.AddedSugars,
                    Fiber = original.Fiber,
                    Protein = original.Protein,
                    Sodium = original.Sodium,
                    Cholesterol = original.Cholesterol,
                    VitaminA = original.VitaminA,
                    VitaminC = original.VitaminC,
                    VitaminD = original.VitaminD,
                    Calcium = original.Calcium,
                    Iron = original.Iron,
                    Potassium = original.Potassium,
                    Magnesium = original.Magnesium,
                    ProductSize = original.ProductSize,
                    ProductSizeIsOneUnit = original.ProductSizeIsOneUnit,
                    Price = original.Price,
                    Currency = original.Currency,
                    CreatedDate = now,
                    ModifiedDate = now
                };
            }
            else
            {
                recipeIngredient.IngredientId = replacement.ReplacementId;
                recipeIngredient.Ingredient = EFContext.Ingredients.Find(replacement.ReplacementId);
                if (recipeIngredient.Ingredient.UserId == 1)
                {
                    continue;
                }

                if (replacement.TransferNutritionData || replacement.TransferPriceData)
                {
                    var original = EFContext.Ingredients.Find(recipeIngredient.IngredientId);

                    if (replacement.TransferNutritionData)
                    {
                        recipeIngredient.Ingredient.ServingSize = original.ServingSize;
                        recipeIngredient.Ingredient.ServingSizeIsOneUnit = original.ServingSizeIsOneUnit;
                        recipeIngredient.Ingredient.Calories = original.Calories;
                        recipeIngredient.Ingredient.Fat = original.Fat;
                        recipeIngredient.Ingredient.SaturatedFat = original.SaturatedFat;
                        recipeIngredient.Ingredient.Carbohydrate = original.Carbohydrate;
                        recipeIngredient.Ingredient.Sugars = original.Sugars;
                        recipeIngredient.Ingredient.AddedSugars = original.AddedSugars;
                        recipeIngredient.Ingredient.Fiber = original.Fiber;
                        recipeIngredient.Ingredient.Protein = original.Protein;
                        recipeIngredient.Ingredient.Sodium = original.Sodium;
                        recipeIngredient.Ingredient.Cholesterol = original.Cholesterol;
                        recipeIngredient.Ingredient.VitaminA = original.VitaminA;
                        recipeIngredient.Ingredient.VitaminC = original.VitaminC;
                        recipeIngredient.Ingredient.VitaminD = original.VitaminD;
                        recipeIngredient.Ingredient.Calcium = original.Calcium;
                        recipeIngredient.Ingredient.Iron = original.Iron;
                        recipeIngredient.Ingredient.Potassium = original.Potassium;
                        recipeIngredient.Ingredient.Magnesium = original.Magnesium;
                    }

                    if (replacement.TransferPriceData)
                    {
                        recipeIngredient.Ingredient.ProductSize = original.ProductSize;
                        recipeIngredient.Ingredient.ProductSizeIsOneUnit = original.ProductSizeIsOneUnit;
                        recipeIngredient.Ingredient.Price = original.Price;
                        recipeIngredient.Ingredient.Currency = original.Currency;
                    }
                }
            }
        }

        EFContext.Recipes.Add(recipe);

        var sendRequest = EFContext.SendRequests.First(x => x.RecipeId == id && x.UserId == userId);
        EFContext.SendRequests.Remove(sendRequest);

        await EFContext.SaveChangesAsync();

        return recipe.Id;
    }

    private string CreatePostfixedNameIfDuplicate(string table, string name, int userId)
    {
        using IDbConnection conn = OpenConnection();

        int postfix = 2;
        var currentName = name;

        bool exists;
        do
        {
            exists = conn.ExecuteScalar<bool>($@"SELECT COUNT(*) FROM cooking.{table} WHERE UPPER(name) = UPPER(@Name) AND user_id = @UserId",
                new { Name = currentName, UserId = userId });

            if (exists)
            {
                int lengthForName = 49 - postfix.ToString().Length;
                currentName = currentName.Length <= lengthForName
                    ? $"{name} {postfix}"
                    : $"{name.Substring(0, lengthForName)} {postfix}";

                postfix++;
            }
        }
        while (exists);

        return currentName;
    }
}
