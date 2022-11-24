using CookingAssistant.Application.Contracts.Ingredients.Models;

namespace CookingAssistant.Api.Models.Ingredients;

public class IngredientSuggestionsVm
{
    public IEnumerable<IngredientSuggestion> Suggestions { get; set; }
    public IEnumerable<IngredientSuggestion> TaskSuggestions { get; set; }
}
