using System.Data;
using Chef.Application.Contracts.Ingredients;
using Chef.Application.Entities;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Sentry;

namespace Chef.Persistence.Repositories;

public class IngredientsRepository : BaseRepository, IIngredientsRepository
{
    public IngredientsRepository(ChefContext efContext)
        : base(efContext) { }

    public IEnumerable<Ingredient> GetUserAndUsedPublicIngredients(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(IngredientsRepository)}.{nameof(GetUserAndUsedPublicIngredients)}");

        try
        {
            using IDbConnection conn = OpenConnection();

            return conn.Query<Ingredient>(@"SELECT * FROM (
	                                        SELECT i.*, it.task_id, COUNT(ri) AS recipe_count
	                                        FROM chef.ingredients AS i
	                                        LEFT JOIN chef.ingredients_tasks AS it ON i.id = it.ingredient_id AND it.user_id = @UserId
	                                        LEFT JOIN chef.recipes_ingredients AS ri ON i.id = ri.ingredient_id
	                                        WHERE i.user_id = @UserId
	                                        GROUP BY i.id, it.task_id
	
	                                        UNION ALL
	
	                                        SELECT i.*, it.task_id, COUNT(ri) AS recipe_count
	                                        FROM chef.ingredients AS i
	                                        INNER JOIN chef.recipes_ingredients AS ri ON i.id = ri.ingredient_id AND ri.recipe_id IN (
                                                SELECT id FROM chef.recipes WHERE user_id = @UserId
		                                        UNION ALL
		                                        SELECT recipe_id FROM chef.shares WHERE user_id = @UserId AND is_accepted
                                            )
	                                        LEFT JOIN chef.ingredients_tasks AS it ON i.id = it.ingredient_id AND it.user_id = @UserId
	                                        WHERE i.user_id = 1
	                                        GROUP BY i.id, it.task_id
                                        ) t
                                        ORDER BY t.modified_date DESC, t.name", new { UserId = userId });
        }
        finally
        {
            metric.Finish();
        }
    }

    public Ingredient Get(int id, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(IngredientsRepository)}.{nameof(Get)}");

        try
        {
            using IDbConnection conn = OpenConnection();
            return conn.QueryFirstOrDefault<Ingredient>("SELECT * FROM chef.ingredients WHERE id = @Id", new { Id = id });
        }
        finally
        {
            metric.Finish();
        }
    }

    public Ingredient? GetForUpdate(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(IngredientsRepository)}.{nameof(GetForUpdate)}");

        try
        {
            using IDbConnection conn = OpenConnection();

            var ingredient = conn.QueryFirstOrDefault<Ingredient>(@"SELECT DISTINCT i.*, it.task_id
                                                                FROM chef.ingredients AS i
                                                                LEFT JOIN chef.ingredients_tasks AS it ON i.id = it.ingredient_id AND it.user_id = @UserId
                                                                WHERE i.id = @Id AND i.user_id = @UserId", new { Id = id, UserId = userId });

            if (ingredient is null)
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
                                           FROM chef.recipes_ingredients AS ri
                                           INNER JOIN chef.recipes AS r ON ri.recipe_id = r.id
                                           WHERE ri.ingredient_id = @IngredientId AND ri.recipe_id IN (
	                                           SELECT id FROM chef.recipes WHERE user_id = @UserId
	                                           UNION ALL
	                                           SELECT recipe_id FROM chef.shares WHERE user_id = @UserId AND is_accepted
                                           )
                                           ORDER BY r.name", new { IngredientId = id, UserId = userId });
            ingredient.Recipes.AddRange(recipes);

            return ingredient;
        }
        finally
        {
            metric.Finish();
        }
    }

    public Ingredient? GetPublic(int id, int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(IngredientsRepository)}.{nameof(GetPublic)}");

        try
        {
            using IDbConnection conn = OpenConnection();

            const string query = @"SELECT DISTINCT i.*, it.task_id, b.id, b.name
                               FROM chef.ingredients AS i
                               LEFT JOIN chef.ingredient_brands AS b ON i.brand_id = b.id
                               LEFT JOIN chef.ingredients_tasks AS it ON i.id = it.ingredient_id AND it.user_id = @UserId
                               WHERE i.id = @Id AND i.user_id = 1";

            var ingredient = conn.Query<Ingredient, IngredientBrand, Ingredient>(query, (ingredient, brand) =>
            {
                ingredient.Brand = brand;
                return ingredient;
            }, new { Id = id, UserId = userId }).FirstOrDefault();

            if (ingredient is null)
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
                                           FROM chef.recipes_ingredients AS ri
                                           INNER JOIN chef.recipes AS r ON ri.recipe_id = r.id
                                           WHERE ri.ingredient_id = @IngredientId AND ri.recipe_id IN (
	                                           SELECT id FROM chef.recipes WHERE user_id = @UserId
	                                           UNION ALL
	                                           SELECT recipe_id FROM chef.shares WHERE user_id = @UserId AND is_accepted
                                           )
                                           ORDER BY r.name", new { IngredientId = id, UserId = userId });
            ingredient.Recipes.AddRange(recipes);

            return ingredient;
        }
        finally
        {
            metric.Finish();
        }
    }

    public IEnumerable<Ingredient> GetForSuggestions(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(IngredientsRepository)}.{nameof(GetForSuggestions)}");

        try
        {
            return EFContext.Ingredients.AsNoTracking()
                .Where(x => x.UserId == userId)
                .Include(x => x.Brand)
                .Include(x => x.RecipesIngredients.Where(x => x.Unit != null))
                .ToList();
        }
        finally
        {
            metric.Finish();
        }
    }

    public IEnumerable<IngredientCategory> GetIngredientCategories(ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(IngredientsRepository)}.{nameof(GetIngredientCategories)}");

        try
        {
            using IDbConnection conn = OpenConnection();

            return conn.Query<IngredientCategory>("SELECT * FROM chef.ingredient_categories ORDER BY id");
        }
        finally
        {
            metric.Finish();
        }
    }

    public IEnumerable<ToDoTask> GetTaskSuggestions(int userId, ISpan metricsSpan)
    {
        var metric = metricsSpan.StartChild($"{nameof(IngredientsRepository)}.{nameof(GetTaskSuggestions)}");

        try
        {
            using IDbConnection conn = OpenConnection();

            const string query = @"SELECT DISTINCT t.id, t.name, l.id, l.name
                               FROM todo.tasks AS t
                               INNER JOIN todo.lists AS l ON t.list_id = l.id
                               LEFT JOIN todo.shares AS s ON l.id = s.list_id
                               LEFT JOIN chef.ingredients_tasks AS it ON t.id = it.task_id AND it.user_id = @UserId
                               LEFT JOIN chef.ingredients AS i ON t.id = it.task_id
                               LEFT JOIN chef.recipes_ingredients AS ri on i.id = ri.ingredient_id
                               WHERE l.user_id = @UserId OR (s.user_id = @UserId AND s.is_accepted)";

            return conn.Query<ToDoTask, ToDoList, ToDoTask>(query,
                (task, list) =>
                {
                    task.List = list;
                    return task;
                }, new { UserId = userId });
        }
        finally
        {
            metric.Finish();
        }
    }

    public bool Exists(int id, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>("SELECT COUNT(*) FROM chef.ingredients WHERE id = @Id AND user_id = @UserId",
            new { Id = id, UserId = userId });
    }

    public bool Exists(int id, string name, int userId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM chef.ingredients
                                          WHERE id != @Id AND UPPER(name) = UPPER(@Name) AND user_id = @UserId",
            new { Id = id, Name = name, UserId = userId });
    }

    public bool ExistsInRecipe(int id, int recipeId)
    {
        using IDbConnection conn = OpenConnection();

        return conn.ExecuteScalar<bool>(@"SELECT COUNT(*)
                                          FROM chef.recipes_ingredients
                                          WHERE recipe_id = @RecipeId AND ingredient_id = @IngredientId",
            new { IngredientId = id, RecipeId = recipeId });
    }

    public async Task UpdateAsync(Ingredient ingredient, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(IngredientsRepository)}.{nameof(UpdateAsync)}");

        try
        {
            Ingredient dbIngredient = EFContext.Ingredients.First(x => x.Id == ingredient.Id);

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

            IngredientTask? ingredientTask = EFContext.IngredientsTasks.FirstOrDefault(x => x.IngredientId == ingredient.Id && x.UserId == ingredient.UserId);
            if (ingredient.TaskId.HasValue)
            {
                if (ingredientTask is null)
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

            await EFContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task UpdatePublicAsync(int id, int? taskId, int userId, DateTime createdDate, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(IngredientsRepository)}.{nameof(UpdatePublicAsync)}");

        try
        {
            IngredientTask? ingredientTask = EFContext.IngredientsTasks.FirstOrDefault(x => x.IngredientId == id && x.UserId == userId);
            if (taskId.HasValue)
            {
                if (ingredientTask is null)
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

            await EFContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task DeleteAsync(int id, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(IngredientsRepository)}.{nameof(DeleteAsync)}");

        try
        {
            Ingredient ingredient = EFContext.Ingredients.First(x => x.Id == id);
            EFContext.Ingredients.Remove(ingredient);

            await EFContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            metric.Finish();
        }
    }

    public async Task RemoveFromRecipesAsync(int id, int userId, ISpan metricsSpan, CancellationToken cancellationToken)
    {
        var metric = metricsSpan.StartChild($"{nameof(IngredientsRepository)}.{nameof(RemoveFromRecipesAsync)}");

        try
        {
            int[] userRecipeIds = EFContext.Recipes.Where(x => x.UserId == userId).Select(x => x.Id).ToArray();

            var recipeIngredients = EFContext.RecipesIngredients.Where(x => x.IngredientId == id && userRecipeIds.Contains(x.RecipeId));
            EFContext.RecipesIngredients.RemoveRange(recipeIngredients);

            IngredientTask? ingredientTask = EFContext.IngredientsTasks.FirstOrDefault(x => x.IngredientId == id && x.UserId == userId);
            if (ingredientTask != null)
            {
                EFContext.IngredientsTasks.Remove(ingredientTask);
            }

            await EFContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            metric.Finish();
        }
    }
}
