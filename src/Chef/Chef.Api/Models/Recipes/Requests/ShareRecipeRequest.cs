namespace Chef.Api.Models.Recipes.Requests;

public record ShareRecipeRequest(int RecipeId, List<int> NewShares, List<int> RemovedShares);
