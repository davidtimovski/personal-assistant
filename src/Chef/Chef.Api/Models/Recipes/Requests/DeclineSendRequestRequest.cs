using System.ComponentModel.DataAnnotations;

namespace Chef.Api.Models.Recipes.Requests;

public record DeclineSendRequestRequest([Required] int RecipeId);
