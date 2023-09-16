using Chef.Application.Contracts.Ingredients.Models;

namespace Chef.Api.Models.Ingredients;

public class IngredientSuggestionsVm
{
    public IEnumerable<IngredientSuggestion> Suggestions { get; set; }
    public IEnumerable<IngredientSuggestion> TaskSuggestions { get; set; }
}
