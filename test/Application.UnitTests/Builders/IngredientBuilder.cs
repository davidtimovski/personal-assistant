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
            Name = name,
            TaskId = taskId,
            PriceData = new IngredientPriceData()
        };
    }
}
