using System;
using System.Collections.Generic;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models;

namespace PersonalAssistant.Application.UnitTests.Builders
{
    public class RecipeBuilder
    {
        private string name;
        private string description;
        private List<UpdateRecipeIngredient> recipeIngredients = new List<UpdateRecipeIngredient>();
        private string instructions;
        private TimeSpan? prepDuration;
        private TimeSpan? cookDuration;

        public RecipeBuilder()
        {
            name = "Dummy name";
        }

        public RecipeBuilder WithName(string newName)
        {
            name = newName;
            return this;
        }

        public RecipeBuilder WithDescription(string newDescription)
        {
            description = newDescription;
            return this;
        }

        public RecipeBuilder WithRecipeIngredients()
        {
            return WithRecipeIngredients("Dummy 1", "Dummy 2");
        }

        public RecipeBuilder WithRecipeIngredients(params string[] ingredientNames)
        {
            recipeIngredients = new List<UpdateRecipeIngredient>();
            foreach (var ingredient in ingredientNames)
            {
                recipeIngredients.Add(new UpdateRecipeIngredient
                {
                    Name = ingredient
                });
            }
            return this;
        }

        public RecipeBuilder WithRecipeIngredientsLinkedToTasks()
        {
            recipeIngredients = new List<UpdateRecipeIngredient>();
            for (var i = 1; i < 4; i++)
            {
                recipeIngredients.Add(new UpdateRecipeIngredient
                {
                    TaskId = i
                });
            }
            return this;
        }

        public RecipeBuilder WithRecipeIngredientsWithAmounts(params float?[] amounts)
        {
            recipeIngredients = new List<UpdateRecipeIngredient>();
            foreach (var amount in amounts)
            {
                recipeIngredients.Add(new UpdateRecipeIngredient
                {
                    Name = "Dummy name",
                    Amount = amount
                });
            }
            return this;
        }

        public RecipeBuilder WithInstructions(string newInstructions)
        {
            instructions = newInstructions;
            return this;
        }

        public RecipeBuilder WithPrepDuration(TimeSpan? newPrepDuration)
        {
            prepDuration = newPrepDuration;
            return this;
        }

        public RecipeBuilder WithCookDuration(TimeSpan? newCookDuration)
        {
            cookDuration = newCookDuration;
            return this;
        }

        public CreateRecipe BuildCreateModel()
        {
            return new CreateRecipe
            {
                Name = name,
                Description = description,
                Ingredients = recipeIngredients,
                Instructions = instructions,
                PrepDuration = prepDuration,
                CookDuration = cookDuration
            };
        }

        public UpdateRecipe BuildUpdateModel()
        {
            return new UpdateRecipe
            {
                Name = name,
                Description = description,
                Ingredients = recipeIngredients,
                Instructions = instructions,
                PrepDuration = prepDuration,
                CookDuration = cookDuration
            };
        }
    }
}
