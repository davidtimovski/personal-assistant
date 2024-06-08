namespace Chef.Api.Models.Recipes.Requests;

public record SetShareIsAcceptedRequest(int RecipeId, bool IsAccepted);
