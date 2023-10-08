using Chef.Application.Contracts.Ingredients.Models;
using Chef.Application.Entities;

namespace Application.UnitTests.Builders;

internal class IngredientBuilder
{
    private string name = "Dummy name";
    private int? taskId;
    private List<RecipeIngredient> recipesIngredients = new ();

    internal IngredientBuilder WithName(string newName)
    {
        name = newName;
        return this;
    }

    internal IngredientBuilder WithTaskId()
    {
        taskId = 1;
        return this;
    }

    internal IngredientBuilder WithRecipeIngredientUnits(string[] units)
    {
        recipesIngredients = units.Select(x => new RecipeIngredient { Unit = x }).ToList();
        return this;
    }

    internal Ingredient BuildModel()
    {
        return new Ingredient
        {
            Id = 1,
            RecipesIngredients = recipesIngredients
        };
    }

    internal UpdateIngredient BuildUpdateModel()
    {
        return new UpdateIngredient
        {
            Id = 0,
            Name = name,
            TaskId = taskId,
            PriceData = new UpdateIngredientPriceData
            {
                IsSet = true,
                ProductSize = 0,
                ProductSizeIsOneUnit = false,
                Price = null,
                Currency = null,
            },
            UserId = 0,
            NutritionData = new UpdateIngredientNutritionData
            {
                IsSet = true,
                ServingSize = 0,
                ServingSizeIsOneUnit = false,
                Calories = 0,
                Fat = 0,
                SaturatedFat = 0,
                Carbohydrate = 0,
                Sugars = 0,
                AddedSugars = 0,
                Fiber = 0,
                Protein = 0,
                Sodium = 0,
                Cholesterol = 0,
                VitaminA = 0,
                VitaminC = 0,
                VitaminD = 0,
                Calcium = 0,
                Iron = 0,
                Potassium = 0,
                Magnesium = 0
            }
        };
    }
}
