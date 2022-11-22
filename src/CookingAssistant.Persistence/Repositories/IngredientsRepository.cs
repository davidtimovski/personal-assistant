using System.Data;
using CookingAssistant.Application.Contracts.Ingredients;
using Dapper;
using Domain.CookingAssistant;
using Domain.ToDoAssistant;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories.CookingAssistant;

public class IngredientsRepository : BaseRepository, IIngredientsRepository
{
    public IngredientsRepository(PersonalAssistantContext efContext)
        : base(efContext) { }

    public IEnumerable<Ingredient> GetUserAndUsedPublicIngredients(int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<Ingredient>(@"SELECT * FROM (
	                                        SELECT i.*, it.task_id, COUNT(ri) AS recipe_count
	                                        FROM cooking.ingredients AS i
	                                        LEFT JOIN cooking.ingredients_tasks AS it ON i.id = it.ingredient_id AND it.user_id = @UserId
	                                        LEFT JOIN cooking.recipes_ingredients AS ri ON i.id = ri.ingredient_id
	                                        WHERE i.user_id = @UserId
	                                        GROUP BY i.id, it.task_id
	
	                                        UNION ALL
	
	                                        SELECT i.*, it.task_id, COUNT(ri) AS recipe_count
	                                        FROM cooking.ingredients AS i
	                                        INNER JOIN cooking.recipes_ingredients AS ri ON i.id = ri.ingredient_id AND ri.recipe_id IN (
                                                SELECT id FROM cooking.recipes WHERE user_id = @UserId
		                                        UNION ALL
		                                        SELECT recipe_id FROM cooking.shares WHERE user_id = @UserId AND is_accepted
                                            )
	                                        LEFT JOIN cooking.ingredients_tasks AS it ON i.id = it.ingredient_id AND it.user_id = @UserId
	                                        WHERE i.user_id = 1
	                                        GROUP BY i.id, it.task_id
                                        ) t
                                        ORDER BY t.modified_date DESC, t.name", new { UserId = userId });
    }

    public Ingredient Get(int id)
    {
        using IDbConnection conn = OpenConnection();
        return conn.QueryFirstOrDefault<Ingredient>(@"SELECT * FROM cooking.ingredients WHERE id = @Id", new { Id = id });
    }

    public Ingredient GetForUpdate(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        var ingredient = conn.QueryFirstOrDefault<Ingredient>(@"SELECT DISTINCT i.*, it.task_id
                                                                FROM cooking.ingredients AS i
                                                                LEFT JOIN cooking.ingredients_tasks AS it ON i.id = it.ingredient_id AND it.user_id = @UserId
                                                                WHERE i.id = @Id AND i.user_id = @UserId", new { Id = id, UserId = userId });

        if (ingredient == null)
        {
            return null;
        }

        if (ingredient.TaskId.HasValue)
        {
            const string taskQuery = @"SELECT t.id, t.name, l.id, l.name
                                       FROM todo.tasks AS t
                                       INNER JOIN todo.lists AS l ON t.list_id = l.id
                                       WHERE t.id = @TaskId";

            var task = conn.Query<ToDoTask, ToDoList, ToDoTask>(taskQuery, (task, list) =>
            {
                task.List = list;
                return task;
            }, new { TaskId = ingredient.TaskId.Value }).First();

            ingredient.Task = task;
        }

        var recipes = conn.Query<Recipe>(@"SELECT id, name
                                           FROM cooking.recipes_ingredients AS ri
                                           INNER JOIN cooking.recipes AS r ON ri.recipe_id = r.id
                                           WHERE ri.ingredient_id = @IngredientId AND ri.recipe_id IN (
	                                           SELECT id FROM cooking.recipes WHERE user_id = @UserId
	                                           UNION ALL
	                                           SELECT recipe_id FROM cooking.shares WHERE user_id = @UserId AND is_accepted
                                           )
                                           ORDER BY r.name", new { IngredientId = id, UserId = userId });
        ingredient.Recipes.AddRange(recipes);

        return ingredient;
    }

    public Ingredient GetPublic(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT DISTINCT i.*, it.task_id, b.id, b.name
                               FROM cooking.ingredients AS i
                               LEFT JOIN cooking.ingredient_brands AS b ON i.brand_id = b.id
                               LEFT JOIN cooking.ingredients_tasks AS it ON i.id = it.ingredient_id AND it.user_id = @UserId
                               WHERE i.id = @Id AND i.user_id = 1";

        var ingredient = conn.Query<Ingredient, IngredientBrand, Ingredient>(query, (ingredient, brand) =>
        {
            ingredient.Brand = brand;
            return ingredient;
        }, new { Id = id, UserId = userId }).FirstOrDefault();

        if (ingredient == null)
        {
            return null;
        }

        if (ingredient.TaskId.HasValue)
        {
            const string taskQuery = @"SELECT t.id, t.name, l.id, l.name
                                       FROM todo.tasks AS t
                                       INNER JOIN todo.lists AS l ON t.list_id = l.id
                                       WHERE t.id = @TaskId";

            var task = conn.Query<ToDoTask, ToDoList, ToDoTask>(taskQuery, (task, list) =>
            {
                task.List = list;
                return task;
            }, new { TaskId = ingredient.TaskId.Value }).First();

            ingredient.Task = task;
        }

        var recipes = conn.Query<Recipe>(@"SELECT id, name
                                           FROM cooking.recipes_ingredients AS ri
                                           INNER JOIN cooking.recipes AS r ON ri.recipe_id = r.id
                                           WHERE ri.ingredient_id = @IngredientId AND ri.recipe_id IN (
	                                           SELECT id FROM cooking.recipes WHERE user_id = @UserId
	                                           UNION ALL
	                                           SELECT recipe_id FROM cooking.shares WHERE user_id = @UserId AND is_accepted
                                           )
                                           ORDER BY r.name", new { IngredientId = id, UserId = userId });
        ingredient.Recipes.AddRange(recipes);

        return ingredient;
    }

    public IEnumerable<Ingredient> GetForSuggestions(int userId)
    {
        return EFContext.Ingredients.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Include(x => x.Brand)
            .Include(x => x.RecipesIngredients.Where(x => x.Unit != null))
            .ToList();
    }

    public IEnumerable<IngredientCategory> GetIngredientCategories()
    {
        using IDbConnection conn = OpenConnection();

        return conn.Query<IngredientCategory>(@"SELECT * FROM cooking.ingredient_categories ORDER BY id");
    }

    public IEnumerable<ToDoTask> GetTaskSuggestions(int userId)
    {
        using IDbConnection conn = OpenConnection();

        const string query = @"SELECT DISTINCT t.id, t.name, l.id, l.name
                               FROM todo.tasks AS t
                               INNER JOIN todo.lists AS l ON t.list_id = l.id
                               LEFT JOIN todo.shares AS s ON l.id = s.list_id
                               LEFT JOIN cooking.ingredients_tasks AS it ON t.id = it.task_id AND it.user_id = @UserId
                               LEFT JOIN cooking.ingredients AS i ON t.id = it.task_id
                               LEFT JOIN cooking.recipes_ingredients AS ri on i.id = ri.ingredient_id
                               WHERE l.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted)";

        return conn.Query<ToDoTask, ToDoList, ToDoTask>(query,
            (task, list) =>
            {
                task.List = list;
                return task;
            }, new { UserId = userId });
    }

    public bool Exists(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*) FROM cooking.ingredients WHERE id = @Id AND user_id = @UserId",
            new { Id = id, UserId = userId });
    }

    public bool Exists(int id, string name, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM cooking.ingredients
                                          WHERE id != @Id AND UPPER(name) = UPPER(@Name) AND user_id = @UserId",
            new { Id = id, Name = name, UserId = userId });
    }

    public bool ExistsInRecipe(int id, int recipeId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM cooking.recipes_ingredients
                                          WHERE recipe_id = @RecipeId AND ingredient_id = @IngredientId",
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
                    CreatedDate = ingredient.ModifiedDate
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
                    CreatedDate = ingredient.ModifiedDate
                });
            }
        }
        else if (ingredientTask != null)
        {
            EFContext.IngredientsTasks.Remove(ingredientTask);
        }

        await EFContext.SaveChangesAsync();
    }

    public async Task UpdatePublicAsync(int id, int? taskId, int userId, DateTime createdDate)
    {
        IngredientTask ingredientTask = EFContext.IngredientsTasks.FirstOrDefault(x => x.IngredientId == id && x.UserId == userId);
        if (taskId.HasValue)
        {
            if (ingredientTask == null)
            {
                EFContext.IngredientsTasks.Add(new IngredientTask
                {
                    IngredientId = id,
                    UserId = userId,
                    TaskId = taskId.Value,
                    CreatedDate = createdDate
                });
            }
            else if (ingredientTask.TaskId != taskId.Value)
            {
                EFContext.IngredientsTasks.Remove(ingredientTask);

                EFContext.IngredientsTasks.Add(new IngredientTask
                {
                    IngredientId = id,
                    UserId = userId,
                    TaskId = taskId.Value,
                    CreatedDate = createdDate
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

    public async Task RemoveFromRecipesAsync(int id, int userId)
    {
        int[] userRecipeIds = EFContext.Recipes.Where(x => x.UserId == userId).Select(x => x.Id).ToArray();

        var recipeIngredients = EFContext.RecipesIngredients.Where(x => x.IngredientId == id && userRecipeIds.Contains(x.RecipeId));
        EFContext.RecipesIngredients.RemoveRange(recipeIngredients);

        IngredientTask ingredientTask = EFContext.IngredientsTasks.FirstOrDefault(x => x.IngredientId == id && x.UserId == userId);
        if (ingredientTask != null)
        {
            EFContext.IngredientsTasks.Remove(ingredientTask);
        }

        await EFContext.SaveChangesAsync();
    }
}
