using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Application.Contracts.CookingAssistant.Ingredients;
using Dapper;
using Domain.Entities.CookingAssistant;
using Domain.Entities.ToDoAssistant;

namespace Persistence.Repositories.CookingAssistant;

public class IngredientsRepository : BaseRepository, IIngredientsRepository
{
    public IngredientsRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public IEnumerable<Ingredient> GetAll(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<Ingredient>(@"SELECT i.*, it.""TaskId"", COUNT(ri) AS ""RecipeCount""
                                        FROM ""CookingAssistant.Ingredients"" AS i
                                        LEFT JOIN ""CookingAssistant.IngredientsTasks"" AS it ON i.""Id"" = it.""IngredientId"" AND it.""UserId"" = @UserId
                                        LEFT JOIN ""CookingAssistant.RecipesIngredients"" AS ri ON i.""Id"" = ri.""IngredientId""
                                        WHERE i.""UserId"" = @UserId
                                        GROUP BY i.""Id"", it.""TaskId""
                                        ORDER BY i.""ModifiedDate"" DESC, i.""Name""", new { UserId = userId });
    }

    public Ingredient Get(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        var ingredient = conn.QueryFirstOrDefault<Ingredient>(@"SELECT DISTINCT i.*, it.""TaskId""
                                                                FROM ""CookingAssistant.Ingredients"" AS i
                                                                LEFT JOIN ""CookingAssistant.IngredientsTasks"" AS it ON i.""Id"" = it.""IngredientId"" AND it.""UserId"" = @UserId
                                                                WHERE i.""Id"" = @Id AND i.""UserId"" = @UserId", new { Id = id, UserId = userId });

        if (ingredient == null)
        {
            return null;
        }

        if (ingredient.TaskId.HasValue)
        {
            const string taskQuery = @"SELECT t.""Id"", t.""Name"", l.""Id"", l.""Name""
                                       FROM ""ToDoAssistant.Tasks"" AS t
                                       INNER JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                                       WHERE t.""Id"" = @TaskId";

            var task = conn.Query<ToDoTask, ToDoList, ToDoTask>(taskQuery, (task, list) =>
            {
                task.List = list;
                return task;
            }, new { TaskId = ingredient.TaskId.Value }).First();

            ingredient.Task = task;
        }

        var recipeNames = conn.Query<string>(@"SELECT ""Name""
                                               FROM ""CookingAssistant.RecipesIngredients"" AS ri
                                               LEFT JOIN ""CookingAssistant.Recipes"" AS r ON ri.""RecipeId"" = r.""Id""
                                               WHERE ri.""IngredientId"" = @IngredientId
                                               ORDER BY r.""Name""", new { IngredientId = id });

        ingredient.Recipes.AddRange(recipeNames.Select(x => new Recipe { Name = x }));

        return ingredient;
    }

    public IEnumerable<int> GetIngredientIdsInRecipe(int recipeId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<int>(@"SELECT ""IngredientId"" FROM ""CookingAssistant.RecipesIngredients"" WHERE ""RecipeId"" = @RecipeId", new { RecipeId = recipeId });
    }

    public async Task<IEnumerable<Ingredient>> GetPublicAndUserIngredientsAsync(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return await conn.QueryAsync<Ingredient>(@"SELECT DISTINCT i.""Id"", i.""Name"", it.""TaskId""
                                             FROM ""CookingAssistant.Ingredients"" AS i
                                             LEFT JOIN ""CookingAssistant.IngredientsTasks"" AS it ON i.""Id"" = it.""IngredientId"" AND it.""UserId"" = @UserId
                                             WHERE i.""UserId"" = @UserId OR i.""UserId"" = 1", new { UserId = userId });
    }

    public async Task<IEnumerable<IngredientCategory>> GetIngredientCategoriesAsync()
    {
        using IDbConnection conn = OpenConnection();

        return await conn.QueryAsync<IngredientCategory>(@"SELECT * FROM ""CookingAssistant.IngredientCategories""");
    }

    public IEnumerable<ToDoTask> GetTaskSuggestions(int userId)
    {
        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT DISTINCT t.""Id"", t.""Name"", l.""Id"", l.""Name""
                               FROM ""ToDoAssistant.Tasks"" AS t
                               INNER JOIN ""ToDoAssistant.Lists"" AS l ON t.""ListId"" = l.""Id""
                               LEFT JOIN ""ToDoAssistant.Shares"" AS s ON l.""Id"" = s.""ListId""
                               LEFT JOIN ""CookingAssistant.IngredientsTasks"" AS it ON t.""Id"" = it.""TaskId"" AND it.""UserId"" = @UserId
                               LEFT JOIN ""CookingAssistant.Ingredients"" AS i ON t.""Id"" = it.""TaskId""
                               LEFT JOIN ""CookingAssistant.RecipesIngredients"" AS ri on i.""Id"" = ri.""IngredientId""
                               WHERE l.""UserId"" = @UserId OR (s.""UserId"" = @UserId AND s.""IsAccepted"")";

        return conn.Query<ToDoTask, ToDoList, ToDoTask>(query,
            (task, list) =>
            {
                task.List = list;
                return task;
            }, new { UserId = userId });
    }

    public IEnumerable<Ingredient> GetIngredientSuggestions(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<Ingredient>(@"SELECT i.*
                                        FROM ""CookingAssistant.Ingredients"" AS i
                                        LEFT JOIN ""CookingAssistant.IngredientsTasks"" AS it ON i.""Id"" = it.""IngredientId"" AND it.""UserId"" = @UserId
                                        WHERE i.""UserId"" = @UserId
                                        ORDER BY i.""Name""", new { UserId = userId });
    }

    public bool Exists(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM ""CookingAssistant.Ingredients"" WHERE ""Id"" = @Id AND ""UserId"" = @UserId",
            new { Id = id, UserId = userId });
    }

    public bool Exists(int id, string name, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM ""CookingAssistant.Ingredients""
                                          WHERE ""Id"" != @Id AND UPPER(""Name"") = UPPER(@Name) AND ""UserId"" = @UserId",
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
        Ingredient dbIngredient = EFContext.Ingredients.Find(ingredient.Id);

        dbIngredient.Name = ingredient.Name;
        dbIngredient.ServingSize = ingredient.ServingSize;
        dbIngredient.ServingSizeIsOneUnit = ingredient.ServingSizeIsOneUnit;
        dbIngredient.Calories = ingredient.Calories;
        dbIngredient.Fat = ingredient.Fat;
        dbIngredient.SaturatedFat = ingredient.SaturatedFat;
        dbIngredient.Carbohydrate = ingredient.Carbohydrate;
        dbIngredient.Sugars = ingredient.Sugars;
        dbIngredient.AddedSugars = ingredient.AddedSugars;
        dbIngredient.Fiber = ingredient.Fiber;
        dbIngredient.Protein = ingredient.Protein;
        dbIngredient.Sodium = ingredient.Sodium;
        dbIngredient.Cholesterol = ingredient.Cholesterol;
        dbIngredient.VitaminA = ingredient.VitaminA;
        dbIngredient.VitaminC = ingredient.VitaminC;
        dbIngredient.VitaminD = ingredient.VitaminD;
        dbIngredient.Calcium = ingredient.Calcium;
        dbIngredient.Iron = ingredient.Iron;
        dbIngredient.Potassium = ingredient.Potassium;
        dbIngredient.Magnesium = ingredient.Magnesium;
        dbIngredient.ProductSize = ingredient.ProductSize;
        dbIngredient.ProductSizeIsOneUnit = ingredient.ProductSizeIsOneUnit;
        dbIngredient.Price = ingredient.Price;
        dbIngredient.Currency = ingredient.Currency;
        dbIngredient.ModifiedDate = ingredient.ModifiedDate;

        IngredientTask ingredientTask = EFContext.IngredientsTasks.FirstOrDefault(x => x.IngredientId == ingredient.Id && x.UserId == ingredient.UserId);
        if (ingredient.TaskId.HasValue)
        {
            if (ingredientTask == null)
            {
                EFContext.IngredientsTasks.Add(new IngredientTask
                { 
                    IngredientId = ingredient.Id,
                    UserId = ingredient.UserId,
                    TaskId = ingredient.TaskId.Value, 
                    CreatedDate = ingredient.ModifiedDate,
                    ModifiedDate = ingredient.ModifiedDate
                });
            }
            else if (ingredientTask.TaskId != ingredient.TaskId.Value)
            {
                EFContext.IngredientsTasks.Remove(ingredientTask);

                EFContext.IngredientsTasks.Add(new IngredientTask
                {
                    IngredientId = ingredient.Id,
                    UserId = ingredient.UserId,
                    TaskId = ingredient.TaskId.Value,
                    CreatedDate = ingredient.ModifiedDate,
                    ModifiedDate = ingredient.ModifiedDate
                });
            }
        }
        else if (ingredientTask != null)
        {
            EFContext.IngredientsTasks.Remove(ingredientTask);
        }

        await EFContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Ingredient ingredient = EFContext.Ingredients.Find(id);
        EFContext.Ingredients.Remove(ingredient);

        await EFContext.SaveChangesAsync();
    }
}