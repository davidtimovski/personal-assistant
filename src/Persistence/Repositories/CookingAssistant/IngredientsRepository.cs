using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients;
using PersonalAssistant.Domain.Entities.CookingAssistant;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Persistence.Repositories.CookingAssistant
{
    public class IngredientsRepository : BaseRepository, IIngredientsRepository
    {
        public IngredientsRepository(IOptions<DatabaseSettings> databaseSettings)
            : base(databaseSettings.Value.DefaultConnectionString) { }

        public async Task<IEnumerable<Ingredient>> GetAllAsync(int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryAsync<Ingredient>(@"SELECT i.*,
                                                           CASE WHEN i.""Name"" IS NULL THEN t.""Name"" ELSE i.""Name"" END
                                                       FROM ""CookingAssistant.Ingredients"" AS i
                                                       LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON i.""TaskId"" = t.""Id""
                                                       WHERE ""UserId"" = @UserId
                                                       ORDER BY i.""ModifiedDate"" DESC, i.""Name""",
                new { UserId = userId });
        }

        public async Task<Ingredient> GetAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var ingredient = await conn.QueryFirstOrDefaultAsync<Ingredient>(@"SELECT i.*, 
                                                                                   CASE WHEN i.""Name"" IS NULL THEN t.""Name"" ELSE i.""Name"" END
                                                                               FROM ""CookingAssistant.Ingredients"" AS i
                                                                               LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON i.""TaskId"" = t.""Id""
                                                                               WHERE i.""Id"" = @Id AND i.""UserId"" = @UserId",
                                                                               new { Id = id, UserId = userId });

            var recipeNames = await conn.QueryAsync<string>(@"SELECT ""Name""
                                                              FROM ""CookingAssistant.RecipesIngredients"" AS ri
                                                              LEFT JOIN ""CookingAssistant.Recipes"" AS r ON ri.""RecipeId"" = r.""Id""
                                                              WHERE ri.""IngredientId"" = @IngredientId
                                                              ORDER BY r.""Name""",
                new { IngredientId = id });

            foreach (var recipeName in recipeNames)
            {
                ingredient.Recipes.Add(new Recipe
                {
                    Name = recipeName
                });
            }

            return ingredient;
        }

        public async Task<IEnumerable<Ingredient>> GetSuggestionsAsync(int recipeId, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryAsync<Ingredient>(@"SELECT DISTINCT i.""Id"", i.""TaskId"", 
                                                       CASE WHEN i.""Name"" IS NULL THEN t.""Name"" ELSE i.""Name"" END
                                                       FROM ""CookingAssistant.Ingredients"" AS i
                                                       LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON i.""TaskId"" = t.""Id""
                                                       WHERE ""UserId"" = @UserId AND i.""Id"" NOT IN 
                                                        (SELECT ""IngredientId"" FROM ""CookingAssistant.RecipesIngredients"" WHERE ""RecipeId"" = @RecipeId)",
                                                   new { RecipeId = recipeId, UserId = userId });
        }

        public async Task<IEnumerable<Ingredient>> GetTaskSuggestionsAsync(int recipeId, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var sql = @"SELECT DISTINCT i.""Id"", t.""Id"" AS ""TaskId"", t.""Name"", l.""Id"", l.""Name""
                        FROM ""ToDoAssistant.Tasks"" AS t
                        INNER JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                        LEFT JOIN ""CookingAssistant.Ingredients"" AS i ON t.""Id"" = i.""TaskId""
                        LEFT JOIN ""CookingAssistant.RecipesIngredients"" AS ri on i.""Id"" = ri.""IngredientId""
                        WHERE (@RecipeId = 0 OR ri.""RecipeId"" IS NULL OR ri.""RecipeId"" != @RecipeId) 
                            AND (i.""UserId"" IS NULL OR i.""UserId"" = @UserId)
                            AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))";

            return await conn.QueryAsync<Ingredient, ToDoList, Ingredient>(sql,
                (ingredient, list) =>
                {
                    ingredient.Task = new ToDoTask
                    {
                        List = list
                    };
                    return ingredient;
                }, new { RecipeId = recipeId, UserId = userId }, null, true);
        }

        public async Task<IEnumerable<Ingredient>> GetTaskSuggestionsAsync(int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            var sql = @"SELECT DISTINCT i.""Id"", t.""Id"" AS ""TaskId"", t.""Name"", l.""Id"", l.""Name""
                        FROM ""ToDoAssistant.Tasks"" AS t
                        INNER JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                        LEFT JOIN ""CookingAssistant.Ingredients"" AS i ON t.""Id"" = i.""TaskId""
                        LEFT JOIN ""CookingAssistant.RecipesIngredients"" AS ri on i.""Id"" = ri.""IngredientId""
                        WHERE l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted"")";

            return await conn.QueryAsync<Ingredient, ToDoList, Ingredient>(sql,
                (ingredient, list) =>
                {
                    ingredient.Task = new ToDoTask
                    {
                        List = list
                    };
                    return ingredient;
                }, new { UserId = userId }, null, true);
        }

        public async Task<IEnumerable<Ingredient>> GetIngredientSuggestionsAsync(int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.QueryAsync<Ingredient>(@"SELECT i.*, CASE WHEN i.""Name"" IS NULL THEN t.""Name"" ELSE i.""Name"" END
                                                       FROM ""CookingAssistant.Ingredients"" AS i
                                                       LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON i.""TaskId"" = t.""Id""
                                                       WHERE ""UserId"" = @UserId
                                                       ORDER BY i.""Name""",
                new { UserId = userId });
        }

        public async Task<bool> ExistsAsync(int id, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                         FROM ""CookingAssistant.Ingredients""
                                                         WHERE ""Id"" = @Id AND ""UserId"" = @UserId", 
                                                         new { Id = id, UserId = userId });
        }

        public async Task<bool> ExistsAsync(int id, string name, int userId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                         FROM ""CookingAssistant.Ingredients""
                                                         WHERE ""Id"" != @Id AND UPPER(""Name"") = UPPER(@Name) AND ""UserId"" = @UserId", 
                                                         new { Id = id, Name = name, UserId = userId });
        }

        public async Task<bool> ExistsInRecipeAsync(int id, int recipeId)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            return await conn.ExecuteScalarAsync<bool>(@"SELECT COUNT(*)
                                                         FROM ""CookingAssistant.RecipesIngredients""
                                                         WHERE ""RecipeId"" = @RecipeId AND ""IngredientId"" = @IngredientId", 
                                                         new { IngredientId = id, RecipeId = recipeId });
        }

        public async Task UpdateAsync(Ingredient ingredient)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"UPDATE ""CookingAssistant.Ingredients"" SET ""TaskId"" = @TaskId, ""Name"" = @Name,  
                                    ""ServingSize"" = @ServingSize, ""ServingSizeIsOneUnit"" = @ServingSizeIsOneUnit, 
                                    ""Calories"" = @Calories, ""Fat"" = @Fat, ""SaturatedFat"" = @SaturatedFat, 
                                    ""Carbohydrate"" = @Carbohydrate, ""Sugars"" = @Sugars, ""AddedSugars"" = @AddedSugars, 
                                    ""Fiber"" = @Fiber, ""Protein"" = @Protein, ""Sodium"" = @Sodium,  
                                    ""Cholesterol"" = @Cholesterol, ""VitaminA"" = @VitaminA, ""VitaminC"" = @VitaminC, ""VitaminD"" = @VitaminD, 
                                    ""Calcium"" = @Calcium, ""Iron"" = @Iron, ""Potassium"" = @Potassium, ""Magnesium"" = @Magnesium, 
                                    ""ProductSize"" = @ProductSize, ""ProductSizeIsOneUnit"" = @ProductSizeIsOneUnit, 
                                    ""Price"" = @Price, ""Currency"" = @Currency, ""ModifiedDate"" = @ModifiedDate
                                    WHERE ""Id"" = @Id", ingredient);
        }

        public async Task DeleteAsync(int id)
        {
            using DbConnection conn = Connection;
            await conn.OpenAsync();

            await conn.ExecuteAsync(@"DELETE FROM ""CookingAssistant.Ingredients"" WHERE ""Id"" = @Id", new { Id = id });
        }
    }
}
