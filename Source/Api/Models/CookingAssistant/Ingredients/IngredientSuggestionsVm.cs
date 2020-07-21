using System.Collections.Generic;
using PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients.Models;

namespace Api.Models.CookingAssistant.Ingredients
{
    public class IngredientSuggestionsVm
    {
        public IEnumerable<IngredientSuggestion> Suggestions { get; set; }
        public IEnumerable<IngredientSuggestion> TaskSuggestions { get; set; }
    }
}
