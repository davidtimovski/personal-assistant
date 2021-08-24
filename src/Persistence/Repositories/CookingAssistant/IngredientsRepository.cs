using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Persistence;
using PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients;
using PersonalAssistant.Domain.Entities.CookingAssistant;
using PersonalAssistant.Domain.Entities.ToDoAssistant;

namespace PersonalAssistant.Persistence.Repositories.CookingAssistant
{
    public class IngredientsRepository : BaseRepository, IIngredientsRepository
    {
        public IngredientsRepository(PersonalAssistantContext efContext)
            : base(efContext) { }

        public IEnumerable<Ingredient> GetAll(int userId)
        {
            using IDbConnection conn = OpenConnection();

            return conn.Query<Ingredient>(@"SELECT DISTINCT i.*,
                                                CASE WHEN i.""Name"" IS NULL THEN t.""Name"" ELSE i.""Name"" END
                                            FROM ""CookingAssistant.Ingredients"" AS i
                                            LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON i.""TaskId"" = t.""Id""
                                            LEFT JOIN ""CookingAssistant.RecipesIngredients"" AS ri ON i.""Id"" = ri.""IngredientId""
                                            LEFT JOIN ""CookingAssistant.Recipes"" AS r ON ri.""RecipeId"" = r.""Id""
                                            LEFT JOIN ""CookingAssistant.Shares"" AS s ON ri.""RecipeId"" = s.""RecipeId""
                                            WHERE i.""UserId"" = @UserId OR (r.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))
                                            ORDER BY i.""ModifiedDate"" DESC, i.""Name""",
                new { UserId = userId });
        }

        public Ingredient Get(int id, int userId)
        {
            using IDbConnection conn = OpenConnection();

            var ingredient = conn.QueryFirstOrDefault<Ingredient>(@"SELECT i.*, 
                                                                        CASE WHEN i.""Name"" IS NULL THEN t.""Name"" ELSE i.""Name"" END
                                                                    FROM ""CookingAssistant.Ingredients"" AS i
                                                                    LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON i.""TaskId"" = t.""Id""
                                                                    LEFT JOIN ""CookingAssistant.RecipesIngredients"" AS ri ON i.""Id"" = ri.""IngredientId""
                                                                    LEFT JOIN ""CookingAssistant.Recipes"" AS r ON ri.""RecipeId"" = r.""Id""
                                                                    LEFT JOIN ""CookingAssistant.Shares"" AS s ON ri.""RecipeId"" = s.""RecipeId""
                                                                    WHERE i.""Id"" = @Id AND (i.""UserId"" = @UserId OR (r.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted"")))",
                                                                    new { Id = id, UserId = userId });

            if (ingredient.TaskId.HasValue)
            {
                var list = conn.QueryFirstOrDefault<ToDoList>(@"SELECT l.""Name""
                                                                FROM ""ToDoAssistant.Tasks"" AS t
                                                                INNER JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                                                                WHERE t.""Id"" = @TaskId", new { TaskId = id });
                ingredient.Task = new ToDoTask
                {
                    List = list
                };
            }

            var recipeNames = conn.Query<string>(@"SELECT ""Name""
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

        public IEnumerable<Ingredient> GetSuggestions(int recipeId, int userId)
        {
            using IDbConnection conn = OpenConnection();

            return conn.Query<Ingredient>(@"SELECT DISTINCT i.""Id"", i.""TaskId"", 
                                            CASE WHEN i.""Name"" IS NULL THEN t.""Name"" ELSE i.""Name"" END
                                            FROM ""CookingAssistant.Ingredients"" AS i
                                            LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON i.""TaskId"" = t.""Id""
                                            WHERE (i.""UserId"" = @UserId OR i.""Id"" IN (
	                                            SELECT ""IngredientId""
	                                            FROM ""CookingAssistant.RecipesIngredients"" AS ri
	                                            INNER JOIN ""CookingAssistant.Recipes"" AS r ON ri.""RecipeId"" = r.""Id""
	                                            INNER JOIN ""CookingAssistant.Shares"" AS s ON ri.""RecipeId"" = s.""RecipeId""
	                                            WHERE r.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted"")
                                            )) AND i.""Id"" NOT IN (SELECT ""IngredientId"" FROM ""CookingAssistant.RecipesIngredients"" WHERE ""RecipeId"" = @RecipeId)",
                                        new { RecipeId = recipeId, UserId = userId });
        }

        public IEnumerable<Ingredient> GetTaskSuggestions(int recipeId, int userId)
        {
            using IDbConnection conn = OpenConnection();

            var sql = @"SELECT DISTINCT i.""Id"", i.""TaskId"", t.""Name"", l.""Id"", l.""Name""
                        FROM ""ToDoAssistant.Tasks"" AS t
                        INNER JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                        LEFT JOIN ""CookingAssistant.Ingredients"" AS i ON t.""Id"" = i.""TaskId""
                        LEFT JOIN ""CookingAssistant.RecipesIngredients"" AS ri on i.""Id"" = ri.""IngredientId""
                        WHERE (@RecipeId = 0 OR ri.""RecipeId"" IS NULL OR ri.""RecipeId"" != @RecipeId) 
                            AND (i.""Id"" IS NULL OR i.""Id"" NOT IN (SELECT ""IngredientId"" FROM ""CookingAssistant.RecipesIngredients"" WHERE ""RecipeId"" = @RecipeId))
                            AND (l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))";

            return conn.Query<Ingredient, ToDoList, Ingredient>(sql,
                (ingredient, list) =>
                {
                    ingredient.Task = new ToDoTask
                    {
                        List = list
                    };
                    return ingredient;
                }, new { RecipeId = recipeId, UserId = userId }, null, true);
        }

        public IEnumerable<ToDoTask> GetTaskSuggestions(int userId)
        {
            using IDbConnection conn = OpenConnection();

            var sql = @"SELECT DISTINCT t.""Id"", t.""Name"", l.""Id"", l.""Name""
                        FROM ""ToDoAssistant.Tasks"" AS t
                        INNER JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                        LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                        LEFT JOIN ""CookingAssistant.Ingredients"" AS i ON t.""Id"" = i.""TaskId""
                        LEFT JOIN ""CookingAssistant.RecipesIngredients"" AS ri on i.""Id"" = ri.""IngredientId""
                        WHERE l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted"")";

            return conn.Query<ToDoTask, ToDoList, ToDoTask>(sql,
                (task, list) =>
                {
                    task.List = list;
                    return task;
                }, new { UserId = userId }, null, true);
        }

        public IEnumerable<Ingredient> GetIngredientSuggestions(int userId)
        {
            using IDbConnection conn = OpenConnection();

            return conn.Query<Ingredient>(@"SELECT i.*, CASE WHEN i.""Name"" IS NULL THEN t.""Name"" ELSE i.""Name"" END
                                            FROM ""CookingAssistant.Ingredients"" AS i
                                            LEFT JOIN ""ToDoAssistant.Tasks"" AS t ON i.""TaskId"" = t.""Id""
                                            WHERE ""UserId"" = @UserId
                                            ORDER BY i.""Name""",
                new { UserId = userId });
        }

        public bool Exists(int id, int userId)
        {
            using IDbConnection conn = OpenConnection();

            return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                              FROM ""CookingAssistant.Ingredients"" AS i
                                              LEFT JOIN ""CookingAssistant.RecipesIngredients"" AS ri ON i.""Id"" = ri.""IngredientId""
                                              LEFT JOIN ""CookingAssistant.Recipes"" AS r ON ri.""RecipeId"" = r.""Id""
                                              LEFT JOIN ""CookingAssistant.Shares"" AS s ON ri.""RecipeId"" = s.""RecipeId""
                                              WHERE i.""Id"" = @Id AND i.""UserId"" = @UserId AND (r.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))",
                                              new { Id = id, UserId = userId });
        }

        public bool Exists(int id, string name, int userId)
        {
            using IDbConnection conn = OpenConnection();

            return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                              FROM ""CookingAssistant.Ingredients"" AS i
                                              LEFT JOIN ""CookingAssistant.RecipesIngredients"" AS ri ON i.""Id"" = ri.""IngredientId""
                                              LEFT JOIN ""CookingAssistant.Recipes"" AS r ON ri.""RecipeId"" = r.""Id""
                                              LEFT JOIN ""CookingAssistant.Shares"" AS s ON ri.""RecipeId"" = s.""RecipeId""
                                              WHERE i.""Id"" != @Id AND UPPER(i.""Name"") = UPPER(@Name) AND i.""UserId"" = @UserId AND (r.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted""))",
                                              new { Id = id, Name = name, UserId = userId });
        }

        public bool ExistsInRecipe(int id, int recipeId)
        {
            using IDbConnection conn = OpenConnection();

            return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                              FROM ""CookingAssistant.RecipesIngredients""
                                              WHERE ""RecipeId"" = @RecipeId AND ""IngredientId"" = @IngredientId",
                                              new { IngredientId = id, RecipeId = recipeId });
        }

        public async Task UpdateAsync(Ingredient ingredient)
        {
            using IDbConnection conn = OpenConnection();

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
            using IDbConnection conn = OpenConnection();

            await conn.ExecuteAsync(@"DELETE FROM ""CookingAssistant.Ingredients"" WHERE ""Id"" = @Id", new { Id = id });
        }
    }
}
