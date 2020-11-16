using System.Collections.Generic;

namespace PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models
{
    public class RecipeCostSummary
    {
        public bool IsSet { get; set; }
        public short ProductSize { get; set; }
        public bool ProductSizeIsOneUnit { get; set; }
        public decimal? Cost { get; set; }
        public decimal? CostPerServing { get; set; }
        public List<int> IngredientIds { get; set; } = new List<int>();
    }
}
