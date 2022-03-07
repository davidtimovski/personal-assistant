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

        var recipeIngredientsSql = @"SELECT ri.""Amount"", ri.""Unit"", i.*, it.""TaskId"", t.""Id"", t.""IsCompleted""
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

            const string recipeIngredientsSql = @"SELECT ri.""Amount"", ri.""Unit"", i.""Id"", i.""UserId"", i.""Name"", it.""TaskId"", l.""Id"", l.""Name""
                                                  FROM ""CookingAssistant.RecipesIngredients"" AS ri
                                                  INNER JOIN ""CookingAssistant.Ingredients"" AS i ON ri.""IngredientId"" = i.""Id""
                                                  LEFT JOIN ""CookingAssistant.IngredientsTasks"" AS it ON i.""Id"" = it.""IngredientId"" AND it.""UserId"" = @UserId
                                                  LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON it.""TaskId"" = t.""Id""
                                                  LEFT JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                                                  WHERE ri.""RecipeId"" = @RecipeId";

            var recipeIngredients = conn.Query<RecipeIngredient, Ingredient, ToDoList, RecipeIngredient>(recipeIngredientsSql,
                (recipeIngredient, ingredient, list) =>
                {
                    if (list != null)
                    {
                        ingredient.Task = new ToDoTask
                        {
                            List = list
                        };
                    }
                    recipeIngredient.Ingredient = ingredient;
                    return recipeIngredient;
                }, new { RecipeId = id, UserId = userId }, null, true, "Id,Id");

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

        var userThatsImportingHasIngredients = conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM ""CookingAssistant.Ingredients"" WHERE ""UserId"" = @UserId", new { UserId = userId });

        var recipeHasCustomIngredients = conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                                                    FROM ""CookingAssistant.RecipesIngredients"" AS ri
                                                                    INNER JOIN ""CookingAssistant.Ingredients"" AS i ON ri.""IngredientId"" = i.""Id""
                                                                    WHERE ""RecipeId"" = @RecipeId AND ""UserId"" != 1", new { RecipeId = id });

        return userThatsImportingHasIngredients && recipeHasCustomIngredients;
    }

    public Recipe GetForReview(int id)
    {
        using IDbConnection conn = OpenConnection();

        var recipe = conn.QueryFirstOrDefault<Recipe>(@"SELECT ""Id"", ""UserId"", ""Name"", ""Description"", ""ImageUri"" FROM ""CookingAssistant.Recipes"" WHERE ""Id"" = @Id", new { Id = id });

        if (recipe == null)
        {
            return null;
        }

        const string recipeIngredientsSql = @"SELECT ri.""IngredientId"", i.*
                                              FROM ""CookingAssistant.RecipesIngredients"" AS ri
                                              INNER JOIN ""CookingAssistant.Ingredients"" AS i ON ri.""IngredientId"" = i.""Id""
                                              WHERE ri.""RecipeId"" = @RecipeId AND i.""UserId"" = @UserId";

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

        var recipeToImport = EFContext.Recipes.Find(id);

        var recipe = new Recipe
        {
            UserId = userId,
            Name = CreatePostfixedNameIfDuplicate("Recipes", recipeToImport.Name, userId),
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
                    Name = CreatePostfixedNameIfDuplicate("Ingredients", original.Name, userId),
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
