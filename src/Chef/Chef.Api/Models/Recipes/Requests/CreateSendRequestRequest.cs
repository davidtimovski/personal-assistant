namespace Chef.Api.Models.Recipes.Requests;

public record CreateSendRequestRequest(int RecipeId, List<int> RecipientsIds);
