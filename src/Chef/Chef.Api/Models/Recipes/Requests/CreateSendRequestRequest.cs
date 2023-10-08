using System.ComponentModel.DataAnnotations;

namespace Chef.Api.Models.Recipes.Requests;

public record CreateSendRequestRequest([Required] int RecipeId, [Required] List<int> RecipientsIds);
