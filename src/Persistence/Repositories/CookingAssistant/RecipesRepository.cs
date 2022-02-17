using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.CookingAssistant.Recipes;
using Dapper;
using Domain.Entities.Common;
using Domain.Entities.CookingAssistant;
using Domain.Entities.ToDoAssistant;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories.CookingAssistant;

public class RecipesRepository : BaseRepository, IRecipesRepository
{
    public RecipesRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public IEnumerable<Recipe> GetAll(int userId)
    {
        using IDbConnection conn = OpenConnection();

        var recipes = conn.Query<Recipe>(@"SELECT r.""Id"", r.""UserId"", r.""Name"", r.""ImageUri"", r.""LastOpenedDate"", COUNT(t.""Id"") AS ""IngredientsMissing""
                                           FROM ""CookingAssistant.Recipes"" AS r
                                           LEFT JOIN ""CookingAssistant.RecipesIngredients"" AS ri ON r.""Id"" = ri.""RecipeId""
                                           LEFT JOIN ""CookingAssistant.Ingredients"" AS i ON ri.""IngredientId"" = i.""Id""
                                           LEFT JOIN ""CookingAssistant.IngredientsTasks"" AS it ON i.""Id"" = it.""IngredientId"" AND it.""UserId"" = @UserId
                                           LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON it.""TaskId"" = t.""Id"" AND t.""IsCompleted"" = FALSE
                                           LEFT JOIN ""CookingAssistant.Shares"" AS s ON r.""Id"" = s.""RecipeId""
                                           WHERE r.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted"")
                                           GROUP BY r.""Id"", r.""Name""", new { UserId = userId }).ToList();

        var recipeIds = recipes.Select(x => x.Id).ToArray();
        var shares = conn.Query<RecipeShare>(@"SELECT * FROM ""CookingAssistant.Shares"" WHERE ""RecipeId"" = ANY(@RecipeIds)", new { RecipeIds = recipeIds });

        foreach (var recipe in recipes)
        {
            recipe.Shares = shares.Where(x => x.RecipeId == recipe.Id).ToList();
        }

        return recipes;
    }

    public Recipe Get(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<Recipe>(@"SELECT * FROM ""CookingAssistant.Recipes"" WHERE ""Id"" = @Id", new { Id = id });
    }

    public Recipe Get(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        var recipeSql = @"SELECT r.*, u.""Id"", dp.*
                              FROM ""CookingAssistant.Recipes"" AS r 
                              INNER JOIN ""AspNetUsers"" AS u ON r.""UserId"" = u.""Id""
                              LEFT JOIN ""CookingAssistant.DietaryProfiles"" AS dp ON dp.""UserId"" = @UserId
                              LEFT JOIN ""CookingAssistant.Shares"" AS s ON r.""Id"" = s.""RecipeId""
                              WHERE r.""Id"" = @Id AND (r.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))";
        var recipe = conn.Query<Recipe, User, DietaryProfile, Recipe>(recipeSql,
            (dbRecipe, user, dietaryProfile) =>
            {
                user.DietaryProfile = dietaryProfile;
                dbRecipe.User = user;
                return dbRecipe;
            }, new { Id = id, UserId = userId }, null, true, "Id,UserId").Single();

        if (recipe == null)
        {
            return null;
        }

        if (recipe.UserId == userId)
        {
            conn.Execute(@"UPDATE ""CookingAssistant.Recipes"" SET ""LastOpenedDate"" = @LastOpenedDate WHERE ""Id"" = @Id", new { Id = id, LastOpenedDate = DateTime.UtcNow });
        }
        else
        {
            conn.Execute(@"UPDATE ""CookingAssistant.Shares"" SET ""LastOpenedDate"" = @LastOpenedDate WHERE ""RecipeId"" = @Id", new { Id = id, LastOpenedDate = DateTime.UtcNow });
        }

        recipe.Shares = conn.Query<RecipeShare>(@"SELECT * FROM ""CookingAssistant.Shares"" WHERE ""RecipeId"" = @Id", new { Id = id }).ToList();

        var recipeIngredientsSql = @"SELECT ri.""Amount"", ri.""Unit"", i.""Id"", i.""Name"", 
	                                     i.""ServingSize"", i.""ServingSizeIsOneUnit"", i.""Calories"", 
	                                     i.""Fat"", i.""SaturatedFat"", i.""Carbohydrate"", 
	                                     i.""Sugars"", i.""AddedSugars"", i.""Fiber"", i.""Protein"",
	                                     i.""Sodium"", i.""Cholesterol"", 
	                                     i.""VitaminA"", i.""VitaminC"", i.""VitaminD"",
	                                     i.""Calcium"", i.""Iron"", i.""Potassium"", i.""Magnesium"",
	                                     i.""ProductSize"", i.""ProductSizeIsOneUnit"", i.""Price"", i.""Currency"", i.""ModifiedDate"", it.""TaskId"",
	                                     t.""Id"", t.""Name"", t.""IsCompleted""
                                     FROM ""CookingAssistant.RecipesIngredients"" AS ri
                                     INNER JOIN ""CookingAssistant.Ingredients"" AS i ON ri.""IngredientId"" = i.""Id""
                                     LEFT JOIN ""CookingAssistant.IngredientsTasks"" AS it ON i.""Id"" = it.""IngredientId"" AND it.""UserId"" = @UserId
                                     LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON it.""TaskId"" = t.""Id""
                                     WHERE ri.""RecipeId"" = @RecipeId";

        var recipeIngredients = conn.Query<RecipeIngredient, Ingredient, ToDoTask, RecipeIngredient>(recipeIngredientsSql,
            (recipeIngredient, ingredient, task) =>
            {
                if (task != null)
                {
                    ingredient.Name = task.Name;
                }
                if (task != null)
                {
                    ingredient.Task = task;
                }
                recipeIngredient.Ingredient = ingredient;
                return recipeIngredient;
            }, new { RecipeId = id, UserId = userId }, null, true, "Id,Id");

        recipe.RecipeIngredients.AddRange(recipeIngredients);

        return recipe;
    }

    public Recipe GetForUpdate(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        var recipe = conn.QueryFirstOrDefault<Recipe>(@"SELECT r.* 
                                                        FROM ""CookingAssistant.Recipes"" AS r 
                                                        LEFT JOIN ""CookingAssistant.Shares"" AS s ON r.""Id"" = s.""RecipeId""
                                                        WHERE ""Id"" = @Id AND (r.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))",
            new { Id = id, UserId = userId });

        if (recipe != null)
        {
            recipe.Shares = conn.Query<RecipeShare>(@"SELECT * FROM ""CookingAssistant.Shares"" WHERE ""RecipeId"" = @Id", new { Id = id }).ToList();

            const string recipeIngredientsSql = @"SELECT ri.""Amount"", ri.""Unit"", i.""Id"", i.""Name"", it.""TaskId"", t.""Id"", t.""Name"", l.""Id"", l.""Name""
                                                  FROM ""CookingAssistant.RecipesIngredients"" AS ri
                                                  INNER JOIN ""CookingAssistant.Ingredients"" AS i ON ri.""IngredientId"" = i.""Id""
                                                  LEFT JOIN ""CookingAssistant.IngredientsTasks"" AS it ON i.""Id"" = it.""IngredientId"" AND it.""UserId"" = @UserId
                                                  LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON it.""TaskId"" = t.""Id""
                                                  LEFT JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                                                  WHERE ri.""RecipeId"" = @RecipeId";

            var recipeIngredients = conn.Query<RecipeIngredient, Ingredient, ToDoTask, ToDoList, RecipeIngredient>(recipeIngredientsSql,
                (recipeIngredient, ingredient, task, list) =>
                {
                    if (task != null)
                    {
                        ingredient.Name = task.Name;
                    }
                    if (list != null)
                    {
                        ingredient.Task = new ToDoTask
                        {
                            List = list
                        };
                    }
                    recipeIngredient.Ingredient = ingredient;
                    return recipeIngredient;
                }, new { RecipeId = id, UserId = userId }, null, true, "Id,Id,Id");

            recipe.RecipeIngredients.AddRange(recipeIngredients);
        }

        return recipe;
    }

    public Recipe GetWithOwner(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT DISTINCT r.*, users.""Id"", users.""Email"", users.""ImageUri""
                               FROM ""CookingAssistant.Recipes"" AS r
                               LEFT JOIN ""CookingAssistant.Shares"" AS s ON r.""Id"" = s.""RecipeId""
                               INNER JOIN ""AspNetUsers"" AS users ON r.""UserId"" = users.""Id""
                               WHERE r.""Id"" = @Id AND (r.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))";

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

        const string query = @"SELECT s.*, u.""Id"", u.""Email"", u.""ImageUri""
                               FROM ""CookingAssistant.Shares"" AS s
                               INNER JOIN ""AspNetUsers"" AS u ON s.""UserId"" = u.""Id""
                               WHERE s.""RecipeId"" = @RecipeId AND s.""IsAccepted"" IS NOT FALSE
                               ORDER BY (CASE WHEN s.""IsAccepted"" THEN 1 ELSE 2 END) ASC, s.""CreatedDate""";

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

        const string query = @"SELECT s.*, r.""Name"", u.""Name""
                               FROM ""CookingAssistant.Shares"" AS s
                               INNER JOIN ""CookingAssistant.Recipes"" AS r ON s.""RecipeId"" = r.""Id""
                               INNER JOIN ""AspNetUsers"" AS u ON r.""UserId"" = u.""Id""
                               WHERE s.""UserId"" = @UserId
                               ORDER BY s.""ModifiedDate"" DESC";

        return conn.Query<RecipeShare, Recipe, User, RecipeShare>(query,
            (share, recipe, user) =>
            {
                share.Recipe = recipe;
                share.User = user;
                return share;
            }, new { UserId = userId }, null, true, "Name");
    }

    public int GetPendingShareRequestsCount(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<int>(@"SELECT COUNT(*) FROM ""CookingAssistant.Shares"" WHERE ""UserId"" = @UserId AND ""IsAccepted"" IS NULL",
            new { UserId = userId });
    }

    public bool CanShareWithUser(int shareWithId, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return !conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                           FROM ""CookingAssistant.Recipes"" AS r
                                           INNER JOIN ""CookingAssistant.Shares"" AS s on r.""Id"" = s.""RecipeId""
                                           WHERE r.""UserId"" = @UserId AND s.""UserId"" = @ShareWithId AND s.""IsAccepted"" = FALSE",
            new { ShareWithId = shareWithId, UserId = userId });
    }

    public Recipe GetForSending(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.QueryFirstOrDefault<Recipe>(@"SELECT r.""Id"", r.""Name""
                                                  FROM ""CookingAssistant.Recipes"" AS r
                                                  LEFT JOIN ""CookingAssistant.Shares"" AS s on r.""Id"" = s.""RecipeId""
                                                  WHERE r.""Id"" = @Id AND (r.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))",
            new { Id = id, UserId = userId });
    }

    public IEnumerable<SendRequest> GetSendRequests(int userId)
    {
        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT sr.*, r.""Name"", u.""Name""
                               FROM ""CookingAssistant.SendRequests"" AS sr
                               INNER JOIN ""CookingAssistant.Recipes"" AS r ON sr.""RecipeId"" = r.""Id""
                               INNER JOIN ""AspNetUsers"" AS u ON r.""UserId"" = u.""Id""
                               WHERE sr.""UserId"" = @UserId
                               ORDER BY sr.""ModifiedDate"" DESC";

        return conn.Query<SendRequest, Recipe, User, SendRequest>(query,
            (sendRequest, recipe, user) =>
            {
                sendRequest.Recipe = recipe;
                sendRequest.User = user;
                return sendRequest;
            }, new { UserId = userId }, null, true, "Name");
    }

    public int GetPendingSendRequestsCount(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<int>(@"SELECT COUNT(*) FROM ""CookingAssistant.SendRequests"" WHERE ""UserId"" = @UserId AND ""IsDeclined"" = FALSE",
            new { UserId = userId });
    }

    public bool SendRequestExists(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM ""CookingAssistant.SendRequests"" WHERE ""RecipeId"" = @RecipeId AND ""UserId"" = @UserId AND ""IsDeclined"" = FALSE",
            new { RecipeId = id, UserId = userId });
    }

    public bool IngredientsReviewIsRequired(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        var userHasIngredients = conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM ""CookingAssistant.Ingredients"" WHERE ""UserId"" = @UserId",
            new { UserId = userId });

        var recipeHasIngredients = conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM ""CookingAssistant.RecipesIngredients"" WHERE ""RecipeId"" = @RecipeId",
            new { RecipeId = id });

        return userHasIngredients && recipeHasIngredients;
    }

    public Recipe GetForReview(int id)
    {
        using IDbConnection conn = OpenConnection();

        var recipe = conn.QueryFirstOrDefault<Recipe>(@"SELECT ""Id"", ""UserId"", ""Name"", ""Description"", ""ImageUri"" FROM ""CookingAssistant.Recipes"" WHERE ""Id"" = @Id",
            new { Id = id });

        if (recipe != null)
        {
            const string recipeIngredientsSql = @"SELECT ri.""IngredientId"", i.*, t.""Id"", t.""Name""
                                                  FROM ""CookingAssistant.RecipesIngredients"" AS ri
                                                  INNER JOIN ""CookingAssistant.Ingredients"" AS i ON ri.""IngredientId"" = i.""Id""
                                                  LEFT JOIN ""CookingAssistant.IngredientsTasks"" AS it ON i.""Id"" = it.""IngredientId"" AND it.""UserId"" = @UserId
                                                  LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON it.""TaskId"" = t.""Id""
                                                  WHERE ri.""RecipeId"" = @RecipeId";

            var recipeIngredients = conn.Query<RecipeIngredient, Ingredient, ToDoTask, RecipeIngredient>(recipeIngredientsSql,
                (recipeIngredient, ingredient, task) =>
                {
                    if (task != null)
                    {
                        ingredient.Name = task.Name;
                    }
                    recipeIngredient.Ingredient = ingredient;
                    return recipeIngredient;
                }, new { RecipeId = id, recipe.UserId }, null, true, "Id,Id");

            recipe.RecipeIngredients.AddRange(recipeIngredients);
        }

        return recipe;
    }

    public IEnumerable<string> GetAllImageUris(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<string>(@"SELECT ""ImageUri"" FROM ""CookingAssistant.Recipes"" WHERE ""UserId"" = @UserId",
            new { UserId = userId });
    }

    public string GetImageUri(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<string>(@"SELECT ""ImageUri"" FROM ""CookingAssistant.Recipes"" WHERE ""Id"" = @Id",
            new { Id = id });
    }

    public bool UserOwns(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM ""CookingAssistant.Recipes"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
            new { Id = id, UserId = userId });
    }

    public bool Exists(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM ""CookingAssistant.Recipes"" AS r
                                          LEFT JOIN ""CookingAssistant.Shares"" AS s ON r.""Id"" = s.""RecipeId""
                                          WHERE r.""Id"" = @Id AND (r.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))",
            new { Id = id, UserId = userId });
    }

    public bool Exists(string name, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM ""CookingAssistant.Recipes"" AS r
                                          LEFT JOIN ""CookingAssistant.Shares"" AS s ON r.""Id"" = s.""RecipeId""
                                          WHERE UPPER(r.""Name"") = UPPER(@Name) AND (r.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))",
            new { Name = name, UserId = userId });
    }

    public bool Exists(int id, string name, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM ""CookingAssistant.Recipes"" AS r
                                          LEFT JOIN ""CookingAssistant.Shares"" AS s ON r.""Id"" = s.""RecipeId""
                                          WHERE r.""Id"" != @Id AND UPPER(r.""Name"") = UPPER(@Name) AND (r.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))",
            new { Id = id, Name = name, UserId = userId });
    }

    public int Count(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<int>(@"SELECT COUNT(*) FROM ""CookingAssistant.Recipes"" WHERE ""UserId"" = @UserId", new { UserId = userId });
    }

    public bool UserHasBlockedSharing(int recipeId, int userId, int sharedWithId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM ""CookingAssistant.Shares""
                                          WHERE ""UserId"" = @SharedWithId AND ""IsAccepted"" = FALSE AND (SELECT ""UserId"" FROM ""CookingAssistant.Recipes"" WHERE ""Id"" = @RecipeId) = @SharerId",
            new { SharedWithId = sharedWithId, SharerId = userId, RecipeId = recipeId });
    }

    public (bool canSend, bool alreadySent) CheckSendRequest(int recipeId, int sendToId, int userId)
    {
        using IDbConnection conn = OpenConnection();

        bool canSend = !conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                                   FROM ""CookingAssistant.SendRequests""
                                                   WHERE ""UserId"" = @SendToId AND ""IsDeclined"" = TRUE AND (SELECT ""UserId"" FROM ""CookingAssistant.Recipes"" WHERE ""Id"" = @RecipeId) = @SenderId",
            new { SendToId = sendToId, SenderId = userId, RecipeId = recipeId });

        bool alreadySent = conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                                      FROM ""CookingAssistant.SendRequests""
                                                      WHERE ""RecipeId"" = @RecipeId AND ""UserId"" = @SendToId",
            new { RecipeId = recipeId, SendToId = sendToId });

        return (canSend, alreadySent);
    }

    public IEnumerable<User> GetUsersToBeNotifiedOfRecipeChange(int id, int excludeUserId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<User>(@"SELECT u.*
                                  FROM ""AspNetUsers"" AS u
                                  INNER JOIN ""CookingAssistant.Shares"" AS s ON u.""Id"" = s.""UserId""
                                  WHERE u.""Id"" != @ExcludeUserId AND s.""RecipeId"" = @RecipeId AND s.""IsAccepted"" AND u.""ToDoNotificationsEnabled""
                                  UNION
                                  SELECT u.*
                                  FROM ""AspNetUsers"" AS u
                                  INNER JOIN ""CookingAssistant.Recipes"" AS r ON u.""Id"" = r.""UserId""
                                  WHERE u.""Id"" != @ExcludeUserId AND r.""Id"" = @RecipeId AND u.""ToDoNotificationsEnabled""",
            new { RecipeId = id, ExcludeUserId = excludeUserId });
    }
    public bool CheckIfUserCanBeNotifiedOfRecipeChange(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM ""AspNetUsers"" AS u
                                          INNER JOIN ""CookingAssistant.Shares"" AS s ON u.""Id"" = s.""UserId""
                                          WHERE u.""Id"" = @UserId AND s.""RecipeId"" = @RecipeId AND s.""IsAccepted"" AND u.""ToDoNotificationsEnabled""",
            new { RecipeId = id, UserId = userId });
    }

    public IEnumerable<User> GetUsersToBeNotifiedOfRecipeDeletion(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<User>(@"SELECT u.*
                                  FROM ""AspNetUsers"" AS u
                                  INNER JOIN ""CookingAssistant.Shares"" AS s ON u.""Id"" = s.""UserId""
                                  WHERE s.""RecipeId"" = @RecipeId AND s.""IsAccepted"" AND u.""ToDoNotificationsEnabled""",
            new { RecipeId = id });
    }

    public IEnumerable<User> GetUsersToBeNotifiedOfRecipeSent(int id)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<User>(@"SELECT u.*
                                  FROM ""AspNetUsers"" AS u
                                  INNER JOIN ""CookingAssistant.SendRequests"" AS sr ON u.""Id"" = sr.""UserId""
                                  WHERE sr.""RecipeId"" = @RecipeId AND u.""CookingNotificationsEnabled""",
            new { RecipeId = id });
    }

    public async Task<int> CreateAsync(Recipe recipe)
    {
        var userIngredients = EFContext.Ingredients.Where(x => x.UserId == recipe.UserId).ToArray();

        var existingRecipeIngredients = recipe.RecipeIngredients.Where(x => x.IngredientId != 0).ToArray();
        foreach (var recipeIngredient in existingRecipeIngredients)
        {
            recipeIngredient.Ingredient = userIngredients.First(x => x.Id == recipeIngredient.IngredientId);
            recipe.RecipeIngredients.Add(recipeIngredient);
        }

        var newRecipeIngredients = recipe.RecipeIngredients.Where(x => x.IngredientId == 0).ToArray();
        foreach (var recipeIngredient in newRecipeIngredients)
        {
            Ingredient ingredient;

            if (recipeIngredient.Ingredient.TaskId.HasValue)
            {
                ingredient = new Ingredient
                {
                    UserId = recipe.UserId,
                    TaskId = recipeIngredient.Ingredient.TaskId.Value,
                    CreatedDate = recipe.CreatedDate,
                    ModifiedDate = recipe.CreatedDate
                };
            }
            else
            {
                // Try to find existing by name
                ingredient = userIngredients.FirstOrDefault(x =>
                string.Equals(x.Name, recipeIngredient.Ingredient.Name, StringComparison.OrdinalIgnoreCase));
            }

            if (ingredient == null)
            {
                recipeIngredient.Ingredient.UserId = recipe.UserId;
                recipeIngredient.Ingredient.CreatedDate = recipeIngredient.Ingredient.ModifiedDate = recipe.CreatedDate;
            }
            else
            {
                recipeIngredient.Ingredient = ingredient;
            }

            recipe.RecipeIngredients.Add(recipeIngredient);
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

        var existingRecipeIngredients = recipe.RecipeIngredients.Where(x => x.IngredientId != 0).ToArray();

        if (dbRecipe.UserId == userId)
        {
            dbRecipe.RecipeIngredients.RemoveAll(x => true);

            IEnumerable<Ingredient> userIngredients = EFContext.Ingredients.Where(x => x.UserId == userId).ToArray();

            foreach (var recipeIngredient in existingRecipeIngredients)
            {
                recipeIngredient.Ingredient = userIngredients.First(x => x.Id == recipeIngredient.IngredientId);
                dbRecipe.RecipeIngredients.Add(recipeIngredient);
            }

            var newRecipeIngredients = recipe.RecipeIngredients.Where(x => x.IngredientId == 0).ToArray();
            foreach (var recipeIngredient in newRecipeIngredients)
            {
                Ingredient ingredient;

                if (recipeIngredient.Ingredient.TaskId.HasValue)
                {
                    ingredient = new Ingredient
                    {
                        UserId = userId,
                        TaskId = recipeIngredient.Ingredient.TaskId.Value,
                        CreatedDate = recipe.ModifiedDate,
                        ModifiedDate = recipe.ModifiedDate
                    };
                }
                else
                {
                    // Try to find existing by name
                    ingredient = userIngredients.FirstOrDefault(x =>
                    string.Equals(x.Name, recipeIngredient.Ingredient.Name, StringComparison.OrdinalIgnoreCase));
                }

                if (ingredient == null)
                {
                    recipeIngredient.Ingredient.UserId = userId;
                    recipeIngredient.Ingredient.CreatedDate = recipeIngredient.Ingredient.ModifiedDate = recipe.ModifiedDate;
                }
                else
                {
                    recipeIngredient.Ingredient = ingredient;
                }

                dbRecipe.RecipeIngredients.Add(recipeIngredient);
            }
        }
        else
        {
            foreach (var recipeIngredient in existingRecipeIngredients)
            {
                var dbRecipeIngredient = dbRecipe.RecipeIngredients.FirstOrDefault(x => x.RecipeId == recipeIngredient.RecipeId && x.IngredientId == recipeIngredient.IngredientId);
                if (dbRecipeIngredient == null)
                {
                    continue;
                }

                dbRecipeIngredient.Amount = recipeIngredient.Amount;
                dbRecipeIngredient.Unit = recipeIngredient.Unit;
                dbRecipeIngredient.ModifiedDate = recipe.ModifiedDate;
            }
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

        await conn.ExecuteAsync(@"DELETE FROM ""CookingAssistant.Shares""
                                  WHERE ""RecipeId"" = @RecipeId AND ""UserId"" = @UserId AND ""IsAccepted"" IS DISTINCT FROM FALSE", removedShares, transaction);

        await conn.ExecuteAsync(@"INSERT INTO ""CookingAssistant.Shares"" (""RecipeId"", ""UserId"", ""LastOpenedDate"", ""CreatedDate"", ""ModifiedDate"") 
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
                                                            FROM ""CookingAssistant.Shares"" 
                                                            WHERE ""RecipeId"" = @RecipeId AND ""UserId"" = @UserId",
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

        using IDbConnection conn = OpenConnection();
        var transaction = conn.BeginTransaction();

        var recipe = await conn.QueryFirstOrDefaultAsync<Recipe>(@"SELECT * FROM ""CookingAssistant.Recipes"" WHERE ""Id"" = @Id", new { Id = id });
        int recipeOwnerUserId = recipe.UserId;

        recipe.Name = CreatePostfixedNameIfDuplicate("Recipes", recipe.Name, userId);
        recipe.UserId = userId;
        recipe.ImageUri = imageUri;
        recipe.CreatedDate = recipe.ModifiedDate = recipe.LastOpenedDate = now;

        var createdRecipeId = (await conn.QueryAsync<int>(@"INSERT INTO ""CookingAssistant.Recipes"" (""UserId"", ""Name"", ""Description"", ""Instructions"", ""PrepDuration"", ""CookDuration"", ""Servings"", ""ImageUri"", ""VideoUrl"", ""LastOpenedDate"", ""CreatedDate"", ""ModifiedDate"") 
                                                            VALUES (@UserId, @Name, @Description, @Instructions, @PrepDuration, @CookDuration, @Servings, @ImageUri, @VideoUrl, @LastOpenedDate, @CreatedDate, @ModifiedDate) returning ""Id""", recipe, transaction)).Single();

        const string recipeIngredientsSql = @"SELECT ri.*, i.*, t.""Name"" 
                                              FROM ""CookingAssistant.RecipesIngredients"" AS ri
                                              INNER JOIN ""CookingAssistant.Ingredients"" AS i ON i.""Id"" = ri.""IngredientId""
                                              LEFT JOIN ""CookingAssistant.IngredientsTasks"" AS it ON i.""Id"" = it.""IngredientId"" AND it.""UserId"" = @UserId
                                              LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON it.""TaskId"" = t.""Id""
                                              WHERE ""RecipeId"" = @RecipeId";

        var recipeIngredients = (await conn.QueryAsync<RecipeIngredient, Ingredient, ToDoTask, RecipeIngredient>(recipeIngredientsSql,
            (recipeIngredient, ingredient, task) =>
            {
                ingredient.Task = task;
                recipeIngredient.Ingredient = ingredient;
                return recipeIngredient;
            }, new { RecipeId = id, UserId = recipeOwnerUserId }, null, true, "Id,Name")).ToList();

        recipeIngredients.ForEach(recipeIngredient =>
        {
            recipeIngredient.RecipeId = createdRecipeId;
        });

        foreach (var (Id, ReplacementId, TransferNutritionData, TransferPriceData) in ingredientReplacements)
        {
            RecipeIngredient recipeIngredient = recipeIngredients.FirstOrDefault(x => x.IngredientId == Id);
            recipeIngredient.IngredientId = recipeIngredient.Ingredient.Id = ReplacementId;

            if (TransferNutritionData || TransferPriceData)
            {
                string nutritionDataQuery = string.Empty, priceDataQuery = string.Empty;

                if (TransferNutritionData)
                {
                    nutritionDataQuery = @"""ServingSize"" = @ServingSize, ""ServingSizeIsOneUnit"" = @ServingSizeIsOneUnit, 
                                           ""Calories"" = @Calories, ""Fat"" = @Fat, ""SaturatedFat"" = @SaturatedFat, 
                                           ""Carbohydrate"" = @Carbohydrate, ""Sugars"" = @Sugars, ""AddedSugars"" = @AddedSugars, 
                                           ""Fiber"" = @Fiber, ""Protein"" = @Protein, ""Sodium"" = @Sodium, ""Cholesterol"" = @Cholesterol, 
                                           ""VitaminA"" = @VitaminA, ""VitaminC"" = @VitaminC, ""VitaminD"" = @VitaminD, 
                                           ""Calcium"" = @Calcium, ""Iron"" = @Iron, ""Potassium"" = @Potassium, ""Magnesium"" = @Magnesium,";
                }

                if (TransferPriceData)
                {
                    priceDataQuery = @"""ProductSize"" = @ProductSize, ""ProductSizeIsOneUnit"" = @ProductSizeIsOneUnit, ""Price"" = @Price, ""Currency"" = @Currency,";
                }

                recipeIngredient.Ingredient.ModifiedDate = now;

                await conn.ExecuteAsync($@"UPDATE ""CookingAssistant.Ingredients"" SET 
                                               {nutritionDataQuery}
                                               {priceDataQuery}
                                               ""ModifiedDate"" = @ModifiedDate
                                               WHERE ""Id"" = @Id", recipeIngredient.Ingredient, transaction);
            }

            recipeIngredient.Ingredient = null;
        }

        RecipeIngredient[] toCreate = recipeIngredients.Where(x => x.Ingredient != null).ToArray();
        foreach (RecipeIngredient recipeIngredient in toCreate)
        {
            if (recipeIngredient.Ingredient.TaskId.HasValue)
            {
                recipeIngredient.Ingredient.Name = recipeIngredient.Ingredient.Task.Name;
                recipeIngredient.Ingredient.TaskId = null;
            }

            recipeIngredient.Ingredient.Name = CreatePostfixedNameIfDuplicate("Ingredients", recipeIngredient.Ingredient.Name, userId);
            recipeIngredient.Ingredient.UserId = userId;
            recipeIngredient.Ingredient.CreatedDate = recipeIngredient.Ingredient.ModifiedDate = now;
            var ingredientId = (await conn.QueryAsync<int>(@"INSERT INTO ""CookingAssistant.Ingredients"" (""UserId"", ""TaskId"", ""Name"", 
                                                                ""ServingSize"", ""ServingSizeIsOneUnit"", ""Calories"", ""Fat"", ""SaturatedFat"", 
                                                                ""Carbohydrate"", ""Sugars"", ""AddedSugars"", ""Fiber"", ""Protein"",                         
                                                                ""Sodium"", ""Cholesterol"", ""VitaminA"", ""VitaminC"", ""VitaminD"",
                                                                ""Calcium"", ""Iron"", ""Potassium"", ""Magnesium"", 
                                                                ""ProductSize"", ""ProductSizeIsOneUnit"", ""Price"",
                                                                ""Currency"", ""CreatedDate"", ""ModifiedDate"")
                                                                VALUES (@UserId, @TaskId, @Name, @ServingSize, @ServingSizeIsOneUnit, 
                                                                @Calories, @Fat, @SaturatedFat, @Carbohydrate, @Sugars, @AddedSugars,
                                                                @Fiber, @Protein, @Sodium, @Cholesterol, @VitaminA, @VitaminC, @VitaminD, @Calcium,
                                                                @Iron, @Potassium, @Magnesium, @ProductSize, @ProductSizeIsOneUnit, 
                                                                @Price, @Currency, @CreatedDate, @ModifiedDate) returning ""Id""",
                recipeIngredient.Ingredient, transaction)).Single();

            recipeIngredient.IngredientId = ingredientId;
            recipeIngredient.CreatedDate = recipeIngredient.ModifiedDate = now;
        }

        await conn.ExecuteAsync(@"INSERT INTO ""CookingAssistant.RecipesIngredients"" (""RecipeId"", ""IngredientId"", ""Amount"", ""Unit"", ""CreatedDate"", ""ModifiedDate"")
                                  VALUES (@RecipeId, @IngredientId, @Amount, @Unit, @CreatedDate, @ModifiedDate)", recipeIngredients, transaction);

        await conn.ExecuteAsync(@"DELETE FROM ""CookingAssistant.SendRequests"" WHERE ""RecipeId"" = @RecipeId AND ""UserId"" = @UserId",
            new { RecipeId = id, UserId = userId }, transaction);

        transaction.Commit();

        return createdRecipeId;
    }

    private string CreatePostfixedNameIfDuplicate(string table, string name, int userId)
    {
        using IDbConnection conn = OpenConnection();

        int postfix = 2;
        var currentName = name;

        bool exists;
        do
        {
            exists = conn.ExecuteScalar<bool>($@"SELECT COUNT(*) FROM ""CookingAssistant.{table}"" WHERE UPPER(""Name"") = UPPER(@Name) AND ""UserId"" = @UserId",
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
