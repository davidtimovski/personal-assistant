﻿using Chef.Application.Contracts.Recipes.Models;

namespace Application.UnitTests.Builders;

internal class RecipeBuilder
{
    private string name;
    private string? description;
    private List<UpdateRecipeIngredient> recipeIngredients = new();
    private string? instructions;
    private TimeSpan prepDuration;
    private TimeSpan cookDuration;

    internal RecipeBuilder()
    {
        name = "Dummy name";
    }

    internal RecipeBuilder WithName(string newName)
    {
        name = newName;
        return this;
    }

    internal RecipeBuilder WithDescription(string newDescription)
    {
        description = newDescription;
        return this;
    }

    internal RecipeBuilder WithRecipeIngredients(params string[] ingredientNames)
    {
        recipeIngredients = new List<UpdateRecipeIngredient>();
        foreach (var ingredient in ingredientNames)
        {
            recipeIngredients.Add(new UpdateRecipeIngredient
            {
                Name = ingredient,
                Id = 0,
                Amount = 0,
                Unit = "g"
            });
        }
        return this;
    }

    internal RecipeBuilder WithRecipeIngredientsWithAmounts(params float?[] amounts)
    {
        recipeIngredients = new List<UpdateRecipeIngredient>();
        foreach (var amount in amounts)
        {
            recipeIngredients.Add(new UpdateRecipeIngredient
            {
                Name = "Dummy name",
                Amount = amount,
                Id = 0,
                Unit = "g"
            });
        }
        return this;
    }

    internal RecipeBuilder WithInstructions(string newInstructions)
    {
        instructions = newInstructions;
        return this;
    }

    internal RecipeBuilder WithPrepDuration(TimeSpan newPrepDuration)
    {
        prepDuration = newPrepDuration;
        return this;
    }

    internal RecipeBuilder WithCookDuration(TimeSpan newCookDuration)
    {
        cookDuration = newCookDuration;
        return this;
    }

    internal CreateRecipe BuildCreateModel() => new CreateRecipe
    {
        Name = name,
        Description = description,
        Ingredients = recipeIngredients,
        Instructions = instructions,
        PrepDuration = prepDuration,
        CookDuration = cookDuration,
        UserId = 0,
        Servings = 0,
        ImageUri = null,
        VideoUrl = null
    };

    internal UpdateRecipe BuildUpdateModel() => new UpdateRecipe
    {
        Name = name,
        Description = description,
        Ingredients = recipeIngredients,
        Instructions = instructions,
        PrepDuration = prepDuration,
        CookDuration = cookDuration,
        Id = 0,
        UserId = 0,
        Servings = 0,
        ImageUri = null,
        VideoUrl = null
    };
}
