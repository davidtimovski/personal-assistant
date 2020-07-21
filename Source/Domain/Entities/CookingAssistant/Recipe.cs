using System;
using System.Collections.Generic;
using PersonalAssistant.Domain.Entities.Common;

namespace PersonalAssistant.Domain.Entities.CookingAssistant
{
    public class Recipe
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Instructions { get; set; }
        public TimeSpan? PrepDuration { get; set; }
        public TimeSpan? CookDuration { get; set; }
        public byte Servings { get; set; }
        public string ImageUri { get; set; }
        public string VideoUrl { get; set; }
        public DateTime LastOpenedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public User User { get; set; }
        public List<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();

        public short IngredientsMissing { get; set; }
    }
}
