using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes;
using PersonalAssistant.Domain.Entities.Common;
using PersonalAssistant.Domain.Entities.CookingAssistant;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Persistence.Repositories.CookingAssistant
{
    public class RecipesRepository : BaseRepository, IRecipesRepository
    {
        public RecipesRepository(IOptions<DatabaseSettings> databaseSettings)
            : base(databaseSettings.Value.DefaultConnectionString) { }

        public async Task<IEnumerable<Recipe>> GetAllAsync(int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryAsync<Recipe>(@"SELECT r.""Id"", r.""UserId"", r.""Name"", r.""ImageUri"", r.""LastOpenedDate"", COUNT(t.""Id"") AS ""IngredientsMissing""
                                                       FROM ""CookingAssistant.Recipes"" AS r
                                                       LEFT JOIN ""CookingAssistant.RecipesIngredients"" AS ri ON r.""Id"" = ri.""RecipeId""
                                                       LEFT JOIN ""CookingAssistant.Ingredients"" AS i ON ri.""IngredientId"" = i.""Id""
                                                       LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON i.""TaskId"" = t.""Id"" AND t.""IsCompleted"" = FALSE
                                                       WHERE r.""UserId"" = @UserId
                                                       GROUP BY r.""Id"", r.""Name""", new { UserId = userId });
        }

        public async Task<Recipe> GetAsync(int id)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryFirstOrDefaultAsync<Recipe>(@"SELECT * FROM ""CookingAssistant.Recipes"" WHERE ""Id"" = @Id", new { Id = id });
        }

        public async Task<Recipe> GetAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            bool exists = await ExistsAsync(id, userId);
            if (!exists)
            {
                return null;
            }

            await conn.ExecuteAsync(@"UPDATE ""CookingAssistant.Recipes"" SET ""LastOpenedDate"" = @LastOpenedDate WHERE ""Id"" = @Id",
                new { Id = id, LastOpenedDate = DateTime.Now });

            var recipeSql = @"SELECT r.*, u.""Id"", dp.*
                                FROM ""CookingAssistant.Recipes"" AS r 
                                INNER JOIN ""AspNetUsers"" AS u ON r.""UserId"" = u.""Id""
                                LEFT JOIN ""CookingAssistant.DietaryProfiles"" AS dp ON r.""UserId"" = dp.""UserId""
                                WHERE r.""Id"" = @Id AND r.""UserId"" = @UserId";
            var recipe = (await conn.QueryAsync<Recipe, User, DietaryProfile, Recipe>(recipeSql,
                (dbRecipe, user, dietaryProfile) =>
                {
                    user.DietaryProfile = dietaryProfile;
                    dbRecipe.User = user;
                    return dbRecipe;
                }, new { Id = id, UserId = userId }, null, true, "Id,UserId")).Single();

            var recipeIngredientsSql = @"SELECT ri.""Amount"", ri.""Unit"", i.""Id"", i.""TaskId"", 
                                                i.""Name"", i.""ServingSize"", i.""ServingSizeIsOneUnit"", i.""Calories"", 
                                                i.""Fat"", i.""SaturatedFat"", i.""Carbohydrate"", 
                                                i.""Sugars"", i.""AddedSugars"", i.""Fiber"", i.""Protein"",
                                                i.""Sodium"", i.""Cholesterol"", 
                                                i.""VitaminA"", i.""VitaminC"", i.""VitaminD"",
                                                i.""Calcium"", i.""Iron"", i.""Potassium"", i.""Magnesium"",
                                                i.""ProductSize"", i.""ProductSizeIsOneUnit"", i.""Price"", i.""Currency"",
                                                tasks.""Id"", tasks.""Name"", tasks.""IsCompleted""
                                            FROM ""CookingAssistant.RecipesIngredients"" AS ri
                                            INNER JOIN ""CookingAssistant.Ingredients"" AS i ON ri.""IngredientId"" = i.""Id""
                                            LEFT JOIN ""ToDoAssistant.Tasks"" AS tasks ON i.""TaskId"" = tasks.""Id""
                                            WHERE ri.""RecipeId"" = @RecipeId";

            var recipeIngredients = await conn.QueryAsync<RecipeIngredient, Ingredient, ToDoTask, RecipeIngredient>(recipeIngredientsSql,
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

        public async Task<Recipe> GetForUpdateAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var recipe = await conn.QueryFirstOrDefaultAsync<Recipe>(@"SELECT * FROM ""CookingAssistant.Recipes"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
                new { Id = id, UserId = userId });

            if (recipe != null)
            {
                var recipeIngredientsSql = @"SELECT ri.""Amount"", ri.""Unit"", i.""Id"", i.""TaskId"", 
                                                    i.""Name"", t.""Id"", t.""Name"", l.""Id"", l.""Name""
                                                 FROM ""CookingAssistant.RecipesIngredients"" AS ri
                                                 INNER JOIN ""CookingAssistant.Ingredients"" AS i ON ri.""IngredientId"" = i.""Id""
                                                 LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON i.""TaskId"" = t.""Id""
                                                 LEFT JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                                                 WHERE ri.""RecipeId"" = @RecipeId";

                var recipeIngredients = await conn.QueryAsync<RecipeIngredient, Ingredient, ToDoTask, ToDoList, RecipeIngredient>(recipeIngredientsSql,
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

            await conn.ExecuteAsync(@"UPDATE ""CookingAssistant.Recipes"" SET ""LastOpenedDate"" = @LastOpenedDate WHERE ""Id"" = @Id",
                new { Id = id, LastOpenedDate = DateTime.Now });

            return recipe;
        }

        public async Task<Recipe> GetForSendingAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryFirstOrDefaultAsync<Recipe>(@"SELECT ""Id"", ""Name"" FROM ""CookingAssistant.Recipes"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
                new { Id = id, UserId = userId });
        }

        public async Task<IEnumerable<SendRequest>> GetSendRequestsAsync(int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var sql = @"SELECT sr.*, r.""Name"", u.""Name""
                            FROM ""CookingAssistant.SendRequests"" AS sr
                            INNER JOIN ""CookingAssistant.Recipes"" AS r ON sr.""RecipeId"" = r.""Id""
                            INNER JOIN ""AspNetUsers"" AS u ON r.""UserId"" = u.""Id""
                            WHERE sr.""UserId"" = @UserId
                            ORDER BY sr.""ModifiedDate"" DESC";

            return await conn.QueryAsync<SendRequest, Recipe, User, SendRequest>(sql,
                (sendRequest, recipe, user) =>
                {
                    sendRequest.Recipe = recipe;
                    sendRequest.User = user;
                    return sendRequest;
                }, new { UserId = userId }, null, true, "Name");
        }

        public async Task<int> GetPendingSendRequestsCountAsync(int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<int>(@"SELECT COUNT(*) FROM ""CookingAssistant.SendRequests"" WHERE ""UserId"" = @UserId AND ""IsDeclined"" = FALSE",
                new { UserId = userId });
        }

        public async Task<bool> SendRequestExistsAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*) FROM ""CookingAssistant.SendRequests"" WHERE ""RecipeId"" = @RecipeId AND ""UserId"" = @UserId AND ""IsDeclined"" = FALSE",
                new { RecipeId = id, UserId = userId });
        }

        public async Task<bool> IngredientsReviewIsRequiredAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var userHasIngredients = await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*) FROM ""CookingAssistant.Ingredients"" WHERE ""UserId"" = @UserId",
                new { UserId = userId });

            var recipeHasIngredients = await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*) FROM ""CookingAssistant.RecipesIngredients"" WHERE ""RecipeId"" = @RecipeId",
                new { RecipeId = id });

            return userHasIngredients && recipeHasIngredients;
        }

        public async Task<Recipe> GetForReviewAsync(int id)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var recipe = await conn.QueryFirstOrDefaultAsync<Recipe>(@"SELECT ""Id"", ""UserId"", ""Name"", ""Description"", ""ImageUri"" FROM ""CookingAssistant.Recipes"" WHERE ""Id"" = @Id",
                new { Id = id });

            if (recipe != null)
            {
                var recipeIngredientsSql = @"SELECT ri.""IngredientId"", i.*, t.""Id"", t.""Name""
                                                 FROM ""CookingAssistant.RecipesIngredients"" AS ri
                                                 INNER JOIN ""CookingAssistant.Ingredients"" AS i ON ri.""IngredientId"" = i.""Id""
                                                 LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON i.""TaskId"" = t.""Id""
                                                 WHERE ri.""RecipeId"" = @RecipeId";

                var recipeIngredients = await conn.QueryAsync<RecipeIngredient, Ingredient, ToDoTask, RecipeIngredient>(recipeIngredientsSql,
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

        public async Task<IEnumerable<string>> GetAllImageUrisAsync(int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryAsync<string>(@"SELECT ""ImageUri"" FROM ""CookingAssistant.Recipes"" WHERE ""UserId"" = @UserId",
                new { UserId = userId });
        }

        public async Task<string> GetImageUriAsync(int id)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<string>(@"SELECT ""ImageUri"" FROM ""CookingAssistant.Recipes"" WHERE ""Id"" = @Id",
                new { Id = id });
        }

        public async Task<bool> ExistsAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*) FROM ""CookingAssistant.Recipes"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
                new { Id = id, UserId = userId });
        }

        public async Task<bool> ExistsAsync(string name, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*) FROM ""CookingAssistant.Recipes"" WHERE UPPER(""Name"") = UPPER(@Name) AND ""UserId"" = @UserId",
                new { Name = name, UserId = userId });
        }

        public async Task<bool> ExistsAsync(int id, string name, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*) FROM ""CookingAssistant.Recipes"" WHERE ""Id"" != @Id AND UPPER(""Name"") = UPPER(@Name) AND ""UserId"" = @UserId",
                new { Id = id, Name = name, UserId = userId });
        }

        public async Task<int> CountAsync(int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<int>(@"SELECT COUNT(*) FROM ""CookingAssistant.Recipes"" WHERE ""UserId"" = @UserId", new { UserId = userId });
        }

        public async Task<(bool canSend, bool alreadySent)> CheckSendRequestAsync(int recipeId, int sendToId, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            bool canSend = !await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                                      FROM ""CookingAssistant.Recipes"" AS r
                                                                      INNER JOIN ""CookingAssistant.SendRequests"" AS sr on r.""Id"" = sr.""RecipeId""
                                                                      WHERE r.""UserId"" = @UserId AND sr.""UserId"" = @SendToId AND sr.""IsDeclined"" = TRUE",
                                                                  new { SendToId = sendToId, UserId = userId });

            bool alreadySent = await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                                         FROM ""CookingAssistant.SendRequests""
                                                                         WHERE ""RecipeId"" = @RecipeId AND ""UserId"" = @SendToId",
                                                                     new { RecipeId = recipeId, SendToId = sendToId });

            return (canSend, alreadySent);
        }

        public async Task<int> CreateAsync(Recipe recipe)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            var existingIngredients = await conn.QueryAsync<Ingredient>(@"SELECT * FROM ""CookingAssistant.Ingredients"" WHERE ""UserId"" = @UserId", new { recipe.UserId });

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

                if (existingIngredient != null)
                {
                    recipeIngredient.IngredientId = existingIngredient.Id;
                    existingRecipeIngredients.Add(recipeIngredient);
                }
                else
                {
                    recipeIngredient.Ingredient.CreatedDate = recipeIngredient.Ingredient.ModifiedDate = recipe.CreatedDate;
                    newRecipeIngredients.Add(recipeIngredient);
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

        public async Task UpdateAsync(Recipe recipe, List<int> ingredientIdsToRemove)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            var existingIngredients = await conn.QueryAsync<Ingredient>(@"SELECT * FROM ""CookingAssistant.Ingredients"" WHERE ""UserId"" = @UserId", new { recipe.UserId });

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

                if (existingIngredient != null)
                {
                    recipeIngredient.IngredientId = existingIngredient.Id;
                    existingRecipeIngredients.Add(recipeIngredient);
                }
                else
                {
                    recipeIngredient.Ingredient.CreatedDate = recipeIngredient.Ingredient.ModifiedDate = recipe.CreatedDate;
                    newRecipeIngredients.Add(recipeIngredient);
                }
            }

            if (ingredientIdsToRemove.Any())
            {
                await conn.ExecuteAsync(@"DELETE FROM ""CookingAssistant.RecipesIngredients"" WHERE ""RecipeId"" = @RecipeId AND ""IngredientId"" = ANY(@Ids)",
                    new { RecipeId = recipe.Id, Ids = ingredientIdsToRemove }, transaction);

                // Delete all ingredients that aren't used in any other recipes
                foreach (var ingredientId in ingredientIdsToRemove)
                {
                    var count = await conn.ExecuteScalarAsync<int>(@"SELECT COUNT(*) FROM ""CookingAssistant.RecipesIngredients"" WHERE ""IngredientId"" = @Id",
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
        }

        public async Task DeleteAsync(int id)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"DELETE FROM ""CookingAssistant.Recipes"" WHERE ""Id"" = @Id", new { Id = id });
        }

        public async Task CreateSendRequestsAsync(IEnumerable<SendRequest> sendRequests)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"INSERT INTO ""CookingAssistant.SendRequests"" (""RecipeId"", ""UserId"", ""CreatedDate"", ""ModifiedDate"") 
                                          VALUES (@RecipeId, @UserId, @CreatedDate, @ModifiedDate)", sendRequests);
        }

        public async Task DeclineSendRequestAsync(int id, int userId, DateTime modifiedDate)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"UPDATE ""CookingAssistant.SendRequests"" SET ""IsDeclined"" = TRUE, ""ModifiedDate"" = @ModifiedDate
                                          WHERE ""RecipeId"" = @RecipeId AND ""UserId"" = @UserId AND ""IsDeclined"" = FALSE",
                                      new { RecipeId = id, UserId = userId, ModifiedDate = modifiedDate });
        }

        public async Task DeleteSendRequestAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"DELETE FROM ""CookingAssistant.SendRequests"" WHERE ""RecipeId"" = @RecipeId AND ""UserId"" = @UserId",
                new { RecipeId = id, UserId = userId });
        }

        public async Task<int> ImportAsync(int id, IEnumerable<(int Id, int ReplacementId, bool TransferNutritionData, bool TransferPriceData)> ingredientReplacements, string imageUri, int userId)
        {
            var now = DateTime.Now;

            using DbConnection conn = Connection;
            await conn.OpenAsync();
            var transaction = conn.BeginTransaction();

            var recipe = await conn.QueryFirstOrDefaultAsync<Recipe>(@"SELECT * FROM ""CookingAssistant.Recipes"" WHERE ""Id"" = @Id", new { Id = id });

            recipe.Name = await CreatePostfixedNameIfDuplicateAsync("Recipes", recipe.Name, userId, conn);
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
                        priceDataQuery = @"""ProductSize"" = @ProductSize, ""ProductSizeIsOneUnit"" = @ProductSizeIsOneUnit, 
                                                ""Price"" = @Price, ""Currency"" = @Currency,";
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

                recipeIngredient.Ingredient.Name = await CreatePostfixedNameIfDuplicateAsync("Ingredients", recipeIngredient.Ingredient.Name, userId, conn);
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

        private async Task<string> CreatePostfixedNameIfDuplicateAsync(string table, string name, int userId, DbConnection conn)
        {
            int postfix = 2;
            var currentName = name;

            bool exists;
            do
            {
                exists = await conn.ExecuteScalarAsync<bool>($@"SELECT COUNT(*) FROM ""CookingAssistant.{table}"" WHERE UPPER(""Name"") = UPPER(@Name) AND ""UserId"" = @UserId",
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
