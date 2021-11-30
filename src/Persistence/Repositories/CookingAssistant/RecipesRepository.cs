﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Persistence;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes;
using PersonalAssistant.Domain.Entities.Common;
using PersonalAssistant.Domain.Entities.CookingAssistant;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Persistence.Repositories.CookingAssistant
{
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
                                               LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON i.""TaskId"" = t.""Id"" AND t.""IsCompleted"" = FALSE
                                               LEFT JOIN ""CookingAssistant.Shares"" AS s ON r.""Id"" = s.""RecipeId""
                                               WHERE r.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted"")
                                               GROUP BY r.""Id"", r.""Name""", new { UserId = userId });

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

            var recipeIngredientsSql = @"SELECT ri.""Amount"", ri.""Unit"", i.""Id"", i.""TaskId"", 
                                             i.""Name"", i.""ServingSize"", i.""ServingSizeIsOneUnit"", i.""Calories"", 
                                             i.""Fat"", i.""SaturatedFat"", i.""Carbohydrate"", 
                                             i.""Sugars"", i.""AddedSugars"", i.""Fiber"", i.""Protein"",
                                             i.""Sodium"", i.""Cholesterol"", 
                                             i.""VitaminA"", i.""VitaminC"", i.""VitaminD"",
                                             i.""Calcium"", i.""Iron"", i.""Potassium"", i.""Magnesium"",
                                             i.""ProductSize"", i.""ProductSizeIsOneUnit"", i.""Price"", i.""Currency"", i.""ModifiedDate"",
                                             tasks.""Id"", tasks.""Name"", tasks.""IsCompleted""
                                         FROM ""CookingAssistant.RecipesIngredients"" AS ri
                                         INNER JOIN ""CookingAssistant.Ingredients"" AS i ON ri.""IngredientId"" = i.""Id""
                                         LEFT JOIN ""ToDoAssistant.Tasks"" AS tasks ON i.""TaskId"" = tasks.""Id""
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
                }, new { RecipeId = id }, null, true, "Id,Id");

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

                var recipeIngredientsSql = @"SELECT ri.""Amount"", ri.""Unit"", i.""Id"", i.""TaskId"", 
                                                i.""Name"", t.""Id"", t.""Name"", l.""Id"", l.""Name""
                                             FROM ""CookingAssistant.RecipesIngredients"" AS ri
                                             INNER JOIN ""CookingAssistant.Ingredients"" AS i ON ri.""IngredientId"" = i.""Id""
                                             LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON i.""TaskId"" = t.""Id""
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
                    }, new { RecipeId = id }, null, true, "Id,Id,Id");

                recipe.RecipeIngredients.AddRange(recipeIngredients);
            }

            return recipe;
        }

        public Recipe GetWithOwner(int id, int userId)
        {
            using IDbConnection conn = OpenConnection();

            var sql = @"SELECT DISTINCT r.*, users.""Id"", users.""Email"", users.""ImageUri""
                        FROM ""CookingAssistant.Recipes"" AS r
                        LEFT JOIN ""CookingAssistant.Shares"" AS s ON r.""Id"" = s.""RecipeId""
                        INNER JOIN ""AspNetUsers"" AS users ON r.""UserId"" = users.""Id""
                        WHERE r.""Id"" = @Id AND (r.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))";

            return conn.Query<Recipe, User, Recipe>(sql,
                (recipe, user) =>
                {
                    recipe.User = user;
                    return recipe;
                }, new { Id = id, UserId = userId }, null, true).FirstOrDefault();
        }

        public IEnumerable<RecipeShare> GetShares(int id)
        {
            using IDbConnection conn = OpenConnection();

            var sql = @"SELECT s.*, u.""Id"", u.""Email"", u.""ImageUri""
                        FROM ""CookingAssistant.Shares"" AS s
                        INNER JOIN ""AspNetUsers"" AS u ON s.""UserId"" = u.""Id""
                        WHERE s.""RecipeId"" = @RecipeId AND s.""IsAccepted"" IS NOT FALSE
                        ORDER BY (CASE WHEN s.""IsAccepted"" THEN 1 ELSE 2 END) ASC, s.""CreatedDate""";

            return conn.Query<RecipeShare, User, RecipeShare>(sql,
                (share, user) =>
                {
                    share.User = user;
                    return share;
                }, new { RecipeId = id }, null, true);
        }

        public IEnumerable<RecipeShare> GetShareRequests(int userId)
        {
            using IDbConnection conn = OpenConnection();

            var sql = @"SELECT s.*, r.""Name"", u.""Name""
                        FROM ""CookingAssistant.Shares"" AS s
                        INNER JOIN ""CookingAssistant.Recipes"" AS r ON s.""RecipeId"" = r.""Id""
                        INNER JOIN ""AspNetUsers"" AS u ON r.""UserId"" = u.""Id""
                        WHERE s.""UserId"" = @UserId
                        ORDER BY s.""ModifiedDate"" DESC";

            return conn.Query<RecipeShare, Recipe, User, RecipeShare>(sql,
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

            var sql = @"SELECT sr.*, r.""Name"", u.""Name""
                        FROM ""CookingAssistant.SendRequests"" AS sr
                        INNER JOIN ""CookingAssistant.Recipes"" AS r ON sr.""RecipeId"" = r.""Id""
                        INNER JOIN ""AspNetUsers"" AS u ON r.""UserId"" = u.""Id""
                        WHERE sr.""UserId"" = @UserId
                        ORDER BY sr.""ModifiedDate"" DESC";

            return conn.Query<SendRequest, Recipe, User, SendRequest>(sql,
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
                var recipeIngredientsSql = @"SELECT ri.""IngredientId"", i.*, t.""Id"", t.""Name""
                                                 FROM ""CookingAssistant.RecipesIngredients"" AS ri
                                                 INNER JOIN ""CookingAssistant.Ingredients"" AS i ON ri.""IngredientId"" = i.""Id""
                                                 LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON i.""TaskId"" = t.""Id""
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
                    }, new { RecipeId = id }, null, true, "Id,Id");

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
            using IDbConnection conn = OpenConnection();
            var transaction = conn.BeginTransaction();

            var existingIngredients = conn.Query<Ingredient>(@"SELECT * FROM ""CookingAssistant.Ingredients"" WHERE ""UserId"" = @UserId", new { recipe.UserId });

            var recipeId = (await conn.QueryAsync<int>(@"INSERT INTO ""CookingAssistant.Recipes"" (""UserId"", ""Name"", ""Description"", ""Instructions"", ""PrepDuration"", ""CookDuration"", ""Servings"", ""ImageUri"", ""VideoUrl"", ""LastOpenedDate"", ""CreatedDate"", ""ModifiedDate"") 
                                                         VALUES (@UserId, @Name, @Description, @Instructions, @PrepDuration, @CookDuration, @Servings, @ImageUri, @VideoUrl, @LastOpenedDate, @CreatedDate, @ModifiedDate) returning ""Id""", recipe, transaction)).Single();

            var existingRecipeIngredients = new List<RecipeIngredient>();
            var newRecipeIngredients = new List<RecipeIngredient>();
            foreach (var recipeIngredient in recipe.RecipeIngredients)
            {
                recipeIngredient.RecipeId = recipeId;

                Ingredient existingIngredient;
                if (recipeIngredient.Ingredient.TaskId.HasValue)
                {
                    existingIngredient = existingIngredients.FirstOrDefault(x => recipeIngredient.Ingredient.TaskId == x.TaskId);
                }
                else
                {
                    existingIngredient = existingIngredients.FirstOrDefault(x =>
                        string.Equals(x.Name, recipeIngredient.Ingredient.Name, StringComparison.OrdinalIgnoreCase));
                }

                if (existingIngredient == null)
                {
                    recipeIngredient.Ingredient.CreatedDate = recipeIngredient.Ingredient.ModifiedDate = recipe.CreatedDate;
                    newRecipeIngredients.Add(recipeIngredient);
                }
                else
                {
                    recipeIngredient.IngredientId = existingIngredient.Id;
                    existingRecipeIngredients.Add(recipeIngredient);
                }
            }

            // Create ingredients
            foreach (var newRecipeIngredient in newRecipeIngredients)
            {
                newRecipeIngredient.Ingredient.UserId = recipe.UserId;
                newRecipeIngredient.Ingredient.CreatedDate = newRecipeIngredient.Ingredient.ModifiedDate = recipe.CreatedDate;
                newRecipeIngredient.IngredientId = (await conn.QueryAsync<int>(@"INSERT INTO ""CookingAssistant.Ingredients"" (""UserId"", ""TaskId"", ""Name"", ""CreatedDate"", ""ModifiedDate"")
                                                                                 VALUES (@UserId, @TaskId, @Name, @CreatedDate, @ModifiedDate) returning ""Id""",
                                                                                 newRecipeIngredient.Ingredient, transaction)).Single();
            }

            // Add recipe ingredients
            var allRecipeIngredients = existingRecipeIngredients.Concat(newRecipeIngredients);
            await conn.ExecuteAsync(@"INSERT INTO ""CookingAssistant.RecipesIngredients"" (""RecipeId"", ""IngredientId"", ""Amount"", ""Unit"", ""CreatedDate"", ""ModifiedDate"")
                                      VALUES (@RecipeId, @IngredientId, @Amount, @Unit, @CreatedDate, @ModifiedDate)", allRecipeIngredients, transaction);

            transaction.Commit();

            return recipeId;
        }

        public async Task<Recipe> UpdateAsync(Recipe recipe, List<int> ingredientIdsToRemove)
        {
            using IDbConnection conn = OpenConnection();
            var transaction = conn.BeginTransaction();

            IEnumerable<Ingredient> existingIngredients;
            var recipeIsShared = conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM ""CookingAssistant.Shares"" WHERE ""RecipeId"" = @RecipeId AND ""IsAccepted""", new { RecipeId = recipe.Id });
            if (recipeIsShared)
            {
                // Include shared ingredients
                existingIngredients = conn.Query<Ingredient>(@"SELECT i.*
                                                               FROM ""CookingAssistant.Ingredients"" AS i 
                                                               LEFT JOIN ""CookingAssistant.RecipesIngredients"" AS ri ON i.""Id"" = ri.""IngredientId""
                                                               LEFT JOIN ""CookingAssistant.Shares"" AS s ON ri.""RecipeId"" = s.""RecipeId""
                                                               WHERE i.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted"")", new { recipe.UserId });
            }
            else
            {
                existingIngredients = conn.Query<Ingredient>(@"SELECT * FROM ""CookingAssistant.Ingredients"" WHERE ""UserId"" = @UserId", new { recipe.UserId });
            }

            var originalRecipe = conn.QueryFirstOrDefault<Recipe>(@"SELECT * FROM ""CookingAssistant.Recipes"" WHERE ""Id"" = @Id", new { recipe.Id });

            await conn.ExecuteAsync(@"UPDATE ""CookingAssistant.Recipes"" SET ""Name"" = @Name, ""Description"" = @Description, 
                                        ""Instructions"" = @Instructions, ""PrepDuration"" = @PrepDuration, 
                                        ""CookDuration"" = @CookDuration, ""Servings"" = @Servings, ""ImageUri"" = @ImageUri, 
                                        ""VideoUrl"" = @VideoUrl, ""ModifiedDate"" = @ModifiedDate
                                        WHERE ""Id"" = @Id", recipe, transaction);

            var existingRecipeIngredients = new List<RecipeIngredient>();
            var newRecipeIngredients = new List<RecipeIngredient>();

            foreach (var recipeIngredient in recipe.RecipeIngredients)
            {
                recipeIngredient.RecipeId = recipe.Id;

                Ingredient existingIngredient;
                if (recipeIngredient.Ingredient.TaskId.HasValue)
                {
                    existingIngredient = existingIngredients.FirstOrDefault(x => recipeIngredient.Ingredient.TaskId == x.TaskId);
                }
                else
                {
                    existingIngredient = existingIngredients.FirstOrDefault(x =>
                        string.Equals(x.Name, recipeIngredient.Ingredient.Name, StringComparison.OrdinalIgnoreCase));
                }

                if (existingIngredient == null)
                {
                    recipeIngredient.Ingredient.CreatedDate = recipeIngredient.Ingredient.ModifiedDate = recipe.CreatedDate;
                    newRecipeIngredients.Add(recipeIngredient);
                }
                else
                {
                    recipeIngredient.IngredientId = existingIngredient.Id;
                    existingRecipeIngredients.Add(recipeIngredient);
                }
            }

            if (ingredientIdsToRemove.Any())
            {
                await conn.ExecuteAsync(@"DELETE FROM ""CookingAssistant.RecipesIngredients"" WHERE ""RecipeId"" = @RecipeId AND ""IngredientId"" = ANY(@Ids)",
                    new { RecipeId = recipe.Id, Ids = ingredientIdsToRemove }, transaction);

                // Delete all ingredients that aren't used in any other recipes
                foreach (var ingredientId in ingredientIdsToRemove)
                {
                    var count = conn.ExecuteScalar<int>(@"SELECT COUNT(*) FROM ""CookingAssistant.RecipesIngredients"" WHERE ""IngredientId"" = @Id",
                        new { Id = ingredientId }, transaction);

                    if (count == 0)
                    {
                        await conn.ExecuteAsync(@"DELETE FROM ""CookingAssistant.Ingredients"" WHERE ""Id"" = @Id",
                            new { Id = ingredientId }, transaction);
                    }
                }
            }

            // Create ingredients
            foreach (var newRecipeIngredient in newRecipeIngredients)
            {
                newRecipeIngredient.Ingredient.UserId = recipe.UserId;
                newRecipeIngredient.Ingredient.CreatedDate = newRecipeIngredient.Ingredient.ModifiedDate = recipe.ModifiedDate;
                newRecipeIngredient.IngredientId = (await conn.QueryAsync<int>(@"INSERT INTO ""CookingAssistant.Ingredients"" (""UserId"", ""TaskId"", ""Name"", ""CreatedDate"", ""ModifiedDate"")
                                                                                 VALUES (@UserId, @TaskId, @Name, @CreatedDate, @ModifiedDate) returning ""Id""",
                                                                                 newRecipeIngredient.Ingredient, transaction)).Single();
            }

            // Remove previous recipe ingredients
            await conn.ExecuteAsync(@"DELETE FROM ""CookingAssistant.RecipesIngredients"" WHERE ""RecipeId"" = @Id", new { recipe.Id }, transaction);

            // Add recipe ingredients
            var allRecipeIngredients = existingRecipeIngredients.Concat(newRecipeIngredients);
            await conn.ExecuteAsync(@"INSERT INTO ""CookingAssistant.RecipesIngredients"" (""RecipeId"", ""IngredientId"", ""Amount"", ""Unit"", ""CreatedDate"", ""ModifiedDate"")
                                      VALUES (@RecipeId, @IngredientId, @Amount, @Unit, @CreatedDate, @ModifiedDate)", allRecipeIngredients, transaction);

            transaction.Commit();

            return originalRecipe;
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

            var share = await conn.QueryFirstOrDefaultAsync<RecipeShare>(@"SELECT * 
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
            foreach (SendRequest sendRequest in sendRequests)
            {
                EFContext.Add(sendRequest);
            }

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

            recipe.Name = CreatePostfixedNameIfDuplicate("Recipes", recipe.Name, userId);
            recipe.UserId = userId;
            recipe.ImageUri = imageUri;
            recipe.CreatedDate = recipe.ModifiedDate = recipe.LastOpenedDate = now;

            var createdRecipeId = (await conn.QueryAsync<int>(@"INSERT INTO ""CookingAssistant.Recipes"" (""UserId"", ""Name"", ""Description"", ""Instructions"", ""PrepDuration"", ""CookDuration"", ""Servings"", ""ImageUri"", ""VideoUrl"", ""LastOpenedDate"", ""CreatedDate"", ""ModifiedDate"") 
                                                                VALUES (@UserId, @Name, @Description, @Instructions, @PrepDuration, @CookDuration, @Servings, @ImageUri, @VideoUrl, @LastOpenedDate, @CreatedDate, @ModifiedDate) returning ""Id""", recipe, transaction)).Single();

            var recipeIngredientsSql = @"SELECT ri.*, i.*, t.""Name"" 
                                         FROM ""CookingAssistant.RecipesIngredients"" AS ri
                                         INNER JOIN ""CookingAssistant.Ingredients"" AS i ON i.""Id"" = ri.""IngredientId""
                                         LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON i.""TaskId"" = t.""Id""
                                         WHERE ""RecipeId"" = @RecipeId";

            var recipeIngredients = (await conn.QueryAsync<RecipeIngredient, Ingredient, ToDoTask, RecipeIngredient>(recipeIngredientsSql,
                    (recipeIngredient, ingredient, task) =>
                    {
                        ingredient.Task = task;
                        recipeIngredient.Ingredient = ingredient;
                        return recipeIngredient;
                    }, new { RecipeId = id }, null, true, "Id,Name")).ToList();

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
}
