﻿using PersonalAssistant.Domain.Entities.ToDoAssistant;
using System.Collections.Generic;

namespace PersonalAssistant.Domain.Entities.CookingAssistant
{
    public class Ingredient : Entity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? TaskId { get; set; }
        public string Name { get; set; }
        public short ServingSize { get; set; }
        public bool ServingSizeIsOneUnit { get; set; }
        public float? Calories { get; set; }
        public float? Fat { get; set; }
        public float? SaturatedFat { get; set; }
        public float? Carbohydrate { get; set; }
        public float? Sugars { get; set; }
        public float? AddedSugars { get; set; }
        public float? Fiber { get; set; }
        public float? Protein { get; set; }
        public float? Sodium { get; set; }
        public float? Cholesterol { get; set; }
        public float? VitaminA { get; set; }
        public float? VitaminC { get; set; }
        public float? VitaminD { get; set; }
        public float? Calcium { get; set; }
        public float? Iron { get; set; }
        public float? Potassium { get; set; }
        public float? Magnesium { get; set; }
        public short ProductSize { get; set; }
        public bool ProductSizeIsOneUnit { get; set; }
        public decimal? Price { get; set; }
        public string Currency { get; set; }

        public List<Recipe> Recipes { get; set; } = new List<Recipe>();
        public ToDoTask Task { get; set; }
    }
}
