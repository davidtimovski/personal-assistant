using Application.Domain.CookingAssistant;
using CookingAssistant.Application.Contracts.Ingredients.Models;

namespace Application.UnitTests.Builders;

public class IngredientBuilder
{
    private string name;
    private int? taskId;
    private List<RecipeIngredient> recipesIngredients;


    public IngredientBuilder()
    {
        name = "Dummy name";
    }

    public IngredientBuilder WithName(string newName)
    {
        name = newName;
        return this;
    }

    public IngredientBuilder WithTaskId()
    {
        taskId = 1;
        return this;
    }

    public IngredientBuilder WithRecipeIngredientUnits(string[] units)
    {
        recipesIngredients = units.Select(x => new RecipeIngredient { Unit = x }).ToList();
        return this;
    }

    public Ingredient BuildModel()
    {
        return new Ingredient
        {
            Id = 1,
            RecipesIngredients = recipesIngredients
        };
    }

    public UpdateIngredient BuildUpdateModel()
    {
        return new UpdateIngredient
        {
            Name = name,
            TaskId = taskId,
            PriceData = new IngredientPriceData()
        };
    }
}
